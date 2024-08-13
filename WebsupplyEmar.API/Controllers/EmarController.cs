using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using System.Dynamic;
using System.Security.Claims;
using WebsupplyEmar.API.Funcoes;
using WebsupplyEmar.API.Helpers;
using WebsupplyEmar.Dados.ADO;
using WebsupplyEmar.Dominio.Dto;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using WebsupplyEmar.Dominio.Model;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace WebsupplyEmar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmarController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string ambiente;

        public EmarController(IConfiguration configuration)
        {
            _configuration = configuration;
            if (_configuration["Environment"] == "Development") ambiente = "DEV";
            if (_configuration["Environment"] == "Staging") ambiente = "HOM";
            if (_configuration["Environment"] == "Production") ambiente = "PROD";
        }

        [HttpPost]
        [Route("gerar_hash")]
        public ObjectResult GERAR_HASH(ClaimsModel objClaimsRequest)
        {
            // Gera o Token
            GeradorClaimsJWT geradorClaimsJWT = new GeradorClaimsJWT(_configuration.GetValue<string>("JWT:ValidAudience"),
                                _configuration.GetValue<string>("JWT:ValidIssuer"),
                                _configuration.GetValue<string>("JWT:SecretKey"),
                                _configuration.GetValue<int>("JWT:TokenValidityInMinutes"),
                                _configuration.GetValue<int>("JWT:RefreshTokenValidityInMinutes"));

            // Cria o Token
            string Token = geradorClaimsJWT.CriaToken(objClaimsRequest);

            // Cria a Claims
            ClaimsModel JWT_CLAIMS = GeradorClaimsJWT.CarregaToken(Token);

            // Gera o retorno
            object Retorno = new
            {
                Token = "[" + Token + "]",
                DT_CRIACAO = JWT_CLAIMS.DT_CRIACAO,
                DT_EXPIRACAO = JWT_CLAIMS.DT_EXPIRACAO
            };

            // Retorna a consulta
            return APIResponseHelper.EstruturaResponse(
                "Sucesso",
                "Token Gerado com Sucesso",
                "success",
                Retorno,
                200,
                Url.Action("gerar_hash", "Emar", null, Request.Scheme));
        }

        [HttpPost]
        [Route("receber_emails")]
        public async Task<ObjectResult> RECEBER_EMAILS()
        {
            // Declara as Variaveis
            string userMailId = _configuration.GetValue<string>("EmarWebsupplyWeb:UserMailID");

            // Cria a estrutura de opções do ambiente
            dynamic ambientOptions = new {
                AmbientType = "UserAndPass",
                ClientID = _configuration.GetValue<string>("EmarWebsupplyWeb:ClientId"),
                ClientSecret = _configuration.GetValue<string>("EmarWebsupplyWeb:ClientSecret"),
                TenantID = _configuration.GetValue<string>("EmarWebsupplyWeb:TenantId"),
                AuthUser = _configuration.GetValue<string>("EmarWebsupplyWeb:AuthUser"),
                AuthPass = _configuration.GetValue<string>("EmarWebsupplyWeb:AuthPass")
            };

            // Gera a classe GraphAzure
            GraphAzure graphAzure = new GraphAzure();

            // Gera o Ambiente do Graph
            GraphServiceClient ambienteGraph = graphAzure.CRIA_AMBIENTE_GRAPH(ambientOptions);

            var arrayEmails = await ambienteGraph.Users[userMailId].MailFolders["inbox"].Messages.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Select = new string[]
                {
                    "sender",
                    "subject",
                    "hasAttachments",
                    "body",
                    "isRead"
                };
            });

            // Gera o retorno
            object Retorno = new
            {
                emails = arrayEmails.Value
            };

            // Retorna a consulta
            return APIResponseHelper.EstruturaResponse(
                "Sucesso",
                "Emails Carregados com sucesso",
                "success",
                Retorno,
                200,
                Url.Action("receber_emails", "Emar", null, Request.Scheme));
        }

        [HttpPost]
        [Route("processar_emails")]
        public async Task<ObjectResult> PROCESSAR_EMAILS()
        {
            // Declara as Variaveis de Gestão de Retorno
            string LogMensagem = "";
            bool GeraLog = false;

            // Declara a model que gerencia o log para envio via websocket
            EmarLogProcessamentoModel logProcessamentoModel = new EmarLogProcessamentoModel();

            // Declara as variaveis responsáveis pela estrutura do websocket
            Uri webSocketUri;
            ClientWebSocket webSocketCliente;
            bool webSocketMensagemEnviada = false;

            string Prefixo = Request.Scheme == "https" ? "wss://" : "ws://";
            string Servidor = _configuration.GetValue<string>("WebSockets:Host");
            string Porta = ":" + (Request.Scheme == "https" ? _configuration.GetValue<string>("WebSockets:PortaSSL") : _configuration.GetValue<string>("WebSockets:Porta")) + "/";

            // Gera o Log de operação do Robô
            LogMensagem = "Inicialização do Processamento de Anexos via E-Mail";
            GeraLog = EmarADO.GERA_LOG(
                _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                LogMensagem
                );

            // Declara as Variaveis
            string userMailId = _configuration.GetValue<string>("EmarWebsupplyWeb:UserMailID");

            // Cria a estrutura de opções do ambiente
            dynamic ambientOptions = new
            {
                AmbientType = "UserAndPass",
                ClientID = _configuration.GetValue<string>("EmarWebsupplyWeb:ClientId"),
                ClientSecret = _configuration.GetValue<string>("EmarWebsupplyWeb:ClientSecret"),
                TenantID = _configuration.GetValue<string>("EmarWebsupplyWeb:TenantId"),
                AuthUser = _configuration.GetValue<string>("EmarWebsupplyWeb:AuthUser"),
                AuthPass = _configuration.GetValue<string>("EmarWebsupplyWeb:AuthPass")
            };

            // Gera a classe GraphAzure
            GraphAzure graphAzure = new GraphAzure();

            // Gera o Ambiente do Graph
            GraphServiceClient ambienteGraph = graphAzure.CRIA_AMBIENTE_GRAPH(ambientOptions);

            var arrayEmails = await ambienteGraph.Users[userMailId].MailFolders["inbox"].Messages.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Select = new string[]
                {
                    "sender",
                    "subject",
                    "hasAttachments",
                    "body",
                    "isRead"
                };

                requestConfiguration.QueryParameters.Top = 50;
            });

            // cria as arrays para gerenciamento dos Emails
            List<Microsoft.Graph.Models.Message> emailsProcessados = new List<Microsoft.Graph.Models.Message>();
            List<Microsoft.Graph.Models.Message> emailsNaoProcessados = new List<Microsoft.Graph.Models.Message>();
            List<Microsoft.Graph.Models.Message> emailsSpam = new List<Microsoft.Graph.Models.Message>();

            // Faz a Leitura dos E-Mails e também o processamento
            if (arrayEmails.Value.Count() > 0 )
            {
                for (var i = 0;i < arrayEmails.Value.Count(); i++)
                {
                    // Declara a Variavel de Email
                    var email = arrayEmails.Value[i];

                    // Declara a variavel da pasta de destino
                    string pastaDestino = "";

                    // Verifica se o Email tem o assunto de processamento, caso não, envia direto para a pasta de lixo eletronico
                    if (email.Subject.ToUpper().IndexOf("[PROCESSAMENTO DE ANEXO]") > -1)
                    {
                        // Pega o JWT
                        string tokenEmail = email.Body.Content.ToString();
                        string regexToken = @"\[([^\]]*)\]";

                        Match match = Regex.Match(tokenEmail, regexToken);

                        // Caso exista token, prossegue e caso não, direciona para não processados.
                        if (match.Success)
                        {
                            // Extrai o Jwt
                            string jwtEmail = match.Groups[1].Value;

                            // Converte o JWT em Claims
                            ClaimsModel JWT_CLAIMS = GeradorClaimsJWT.CarregaToken(jwtEmail);

                            // Verifica se é Vazio o retorno e caso sim, não processa o e-mail
                            if(JWT_CLAIMS.CGC != null)
                            {
                                // Verifica se o Ambiente informado no Token é o mesmo que o robô esta processando
                                // Caso não, o Email permanece na caixa de entrada até que o respectivo robô
                                // Faça a leitura do Email
                                if (JWT_CLAIMS.AMBIENTE == ambiente)
                                {
                                    // Define a Uri do WebSocket
                                    webSocketUri = new Uri(Prefixo + Servidor + Porta + "?Chave=" + jwtEmail);

                                    // Define o Cliente do WebSocket
                                    webSocketCliente = new ClientWebSocket();

                                    // Realiza a Conexão com o Servidor WebSocket
                                    webSocketCliente.ConnectAsync(webSocketUri, CancellationToken.None).Wait();

                                    // Valida se o Token é valido
                                    if (GeradorClaimsJWT.ValidaToken(
                                            jwtEmail,
                                            _configuration.GetValue<string>("JWT:SecretKey"),
                                            _configuration.GetValue<string>("JWT:ValidIssuer"),
                                            _configuration.GetValue<string>("JWT:ValidAudience")))
                                    {
                                        // Valida se jwt enviado ja existe na base de dados
                                        string ValidaJWTExistente = EmarADO.CONSULTA_JWT_EXISTENTE(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), jwtEmail);

                                        if (ValidaJWTExistente == "S")
                                        {
                                            // Valida a Claims
                                            if (GeradorClaimsJWT.ValidaClaims(JWT_CLAIMS))
                                            {
                                                // Verifica se o E-Mail possui anexo
                                                if ((bool)email.HasAttachments)
                                                {
                                                    // Consulta os anexos
                                                    var consultaAnexos = await ambienteGraph.Users[userMailId].MailFolders["inbox"].Messages[email.Id].Attachments.GetAsync();

                                                    // Define o parametro para caso o email contenha somente 1 anexo e
                                                    // este anexo faça parte da assinatura de email
                                                    bool AnexoAssinaturaEmail = false;
                                                    string NomeAnexoAssinaturaEmail = null;

                                                    // Consulta os dados do ambiente
                                                    EmarAmbienteResponseDto objEmarAmbiente = EmarADO.CONSULTA_AMBIENTE_ARQUIVOS(
                                                        _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                        JWT_CLAIMS.CGCMatriz,
                                                        ambiente,
                                                        JWT_CLAIMS.TABELA
                                                        );

                                                    // Define os multiplos anexos como S somente para a cotação
                                                    string MultiplosAnexos = "N";

                                                    if (
                                                        JWT_CLAIMS.TABELA == "CL_PROCESSO_ANEXO" ||
                                                        JWT_CLAIMS.TABELA == "PEDIDOS_ARQUIVOS" ||
                                                        JWT_CLAIMS.TABELA == "PedidosItens_Temp")
                                                    {
                                                        MultiplosAnexos = "S";
                                                    }

                                                    // Contabiliza o total de anexos validos
                                                    int AnexosValidos = ArquivoHelper.ContabilizaAnexosValidos(consultaAnexos.Value);

                                                    // Verifica se o Ambiente Permite Multiplos Anexos e caso não e tenha mais de 1 anexo, ja move o email para não processados
                                                    if (MultiplosAnexos == "N" && AnexosValidos > 1)
                                                    {
                                                        // Estrutura o log do Robô
                                                        logProcessamentoModel.Data = DateTime.Now.ToString();
                                                        logProcessamentoModel.Status = "NP";
                                                        logProcessamentoModel.Descricao = "O Email não foi processado pois o ambiente de upload não permite múltiplos anexos";

                                                        // Envia a Mensagem e ja encerra a conexão com o websocket
                                                        webSocketMensagemEnviada = EmarHelper.EnviaMensagemWebSocket(logProcessamentoModel, webSocketCliente, true);

                                                        // Gera o Log de Processamento
                                                        if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                                                            _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                            JWT_CLAIMS.CGC,
                                                            JWT_CLAIMS.CCUSTO,
                                                            JWT_CLAIMS.REQUISIT,
                                                            JWT_CLAIMS.CDGPED,
                                                            JWT_CLAIMS.CL_CDG,
                                                            JWT_CLAIMS.CODPROD,
                                                            JWT_CLAIMS.CODITEM,
                                                            JWT_CLAIMS.CGCF,
                                                            email.Id,
                                                            email.Subject,
                                                            email.Sender.EmailAddress.Name,
                                                            email.Sender.EmailAddress.Address,
                                                            email.Body.Content,
                                                            (bool)email.HasAttachments ? "S" : "N",
                                                            NomeAnexoAssinaturaEmail,
                                                            jwtEmail,
                                                            GeradorClaimsJWT.ConverteClaimsParaString(JWT_CLAIMS),
                                                            logProcessamentoModel.Status,
                                                            logProcessamentoModel.Descricao))
                                                        {
                                                            // Gera o Log de operação do Robô
                                                            LogMensagem = "Não foi possível gerar o log de processamento do Email onde o ambiente de upload não permite múltiplos anexos";
                                                            GeraLog = EmarADO.GERA_LOG(
                                                                _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                LogMensagem
                                                                );

                                                            // Retorna erro
                                                            return APIResponseHelper.EstruturaResponse(
                                                                "Ops",
                                                                LogMensagem,
                                                                "error",
                                                                null,
                                                                400,
                                                                Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                                        }

                                                        // Seta a pasta de destino
                                                        pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:UnprocessedFolder");

                                                        // Adiciona o email processado a array
                                                        emailsNaoProcessados.Add(email);
                                                    }
                                                    else
                                                    {
                                                        // Verifica se a Tabela Enviada está parametrizada
                                                        if (JWT_CLAIMS.TABELA != "PedidosItens_Temp" &&
                                                            JWT_CLAIMS.TABELA != "CL_PROCESSO_ANEXO" &&
                                                            JWT_CLAIMS.TABELA != "PEDIDOS_ARQUIVOS")
                                                        {
                                                            // Estrutura o log do Robô
                                                            logProcessamentoModel.Data = DateTime.Now.ToString();
                                                            logProcessamentoModel.Status = "NP";
                                                            logProcessamentoModel.Descricao = "O Email não foi processado pois a Tabela enviada não existe";

                                                            // Envia a Mensagem e ja encerra a conexão com o websocket
                                                            webSocketMensagemEnviada = EmarHelper.EnviaMensagemWebSocket(logProcessamentoModel, webSocketCliente, true);

                                                            // Gera o Log de Processamento
                                                            if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                                                                _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                JWT_CLAIMS.CGC,
                                                                JWT_CLAIMS.CCUSTO,
                                                                JWT_CLAIMS.REQUISIT,
                                                                JWT_CLAIMS.CDGPED,
                                                                JWT_CLAIMS.CL_CDG,
                                                                JWT_CLAIMS.CODPROD,
                                                                JWT_CLAIMS.CODITEM,
                                                                JWT_CLAIMS.CGCF,
                                                                email.Id,
                                                                email.Subject,
                                                                email.Sender.EmailAddress.Name,
                                                                email.Sender.EmailAddress.Address,
                                                                email.Body.Content,
                                                                (bool)email.HasAttachments ? "S" : "N",
                                                                null,
                                                                jwtEmail,
                                                                GeradorClaimsJWT.ConverteClaimsParaString(JWT_CLAIMS),
                                                                logProcessamentoModel.Status,
                                                                logProcessamentoModel.Descricao))
                                                            {
                                                                // Gera o Log de operação do Robô
                                                                LogMensagem = "Não foi possível gerar o log de processamento do Email Processado pois a Tabela não existe";
                                                                GeraLog = EmarADO.GERA_LOG(
                                                                    _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                    LogMensagem
                                                                    );

                                                                // Retorna erro
                                                                return APIResponseHelper.EstruturaResponse(
                                                                    "Ops",
                                                                    LogMensagem,
                                                                    "error",
                                                                    null,
                                                                    400,
                                                                    Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                                            }

                                                            // Seta a pasta de destino
                                                            pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:UnprocessedFolder");

                                                            // Adiciona o email processado a array
                                                            emailsNaoProcessados.Add(email);
                                                        }
                                                        else
                                                        {
                                                            // Define o Parametro para Registrar o nome dos arquivos
                                                            string nomesAnexos = "";

                                                            // Parametro para Contabilizar anexos inválidos
                                                            int anexosInvalidos = 0;

                                                            // Faz um loop para validar os anexos
                                                            for (var j = 0; j < consultaAnexos.Value.Count(); j++)
                                                            {
                                                                bool validaTipoAnexo = HelperValidacao.ValidaArquivo(consultaAnexos.Value[j].Name);

                                                                // Adiciona o Nome do Anexo a listagem de anexos
                                                                nomesAnexos += consultaAnexos.Value[j].Name + ", ";

                                                                // Valida se é um arquivo permitido e ese não está vazio e caso não adiciona ao contador
                                                                if (!validaTipoAnexo || ((FileAttachment)consultaAnexos.Value[j]).ContentBytes == null)
                                                                {
                                                                    anexosInvalidos++;
                                                                }
                                                            }

                                                            // Verifica se tem algum arquivo inválido e caso não tenha, continua com o processamento do e-mail
                                                            if (anexosInvalidos == 0)
                                                            {
                                                                // Define o Contador de Anexos Processados
                                                                int anexosProcessados = 0;

                                                                for (var j = 0; j < consultaAnexos.Value.Count(); j++)
                                                                {
                                                                    // Define a variavel anexo
                                                                    FileAttachment anexo = (FileAttachment)consultaAnexos.Value[j];

                                                                    // Verifica se o anexo não faz parte da assinatura de email
                                                                    if (!(bool)anexo.IsInline)
                                                                    {
                                                                        // Declara as variaveis de gestão do arquivo
                                                                        string nomeOriginal = anexo.Name;
                                                                        string diretorioDestino = objEmarAmbiente.DriverFisicoArquivos;

                                                                        // Verifica se a pasta existe, e caso não, cria a pasta
                                                                        if (!Directory.Exists(diretorioDestino))
                                                                        {
                                                                            Directory.CreateDirectory(diretorioDestino);
                                                                        }

                                                                        // Adiciona a pasta de Codigo do pedido
                                                                        if (JWT_CLAIMS.TABELA == "PedidosItens_Temp" || JWT_CLAIMS.TABELA == "PEDIDOS_ARQUIVOS")
                                                                        {
                                                                            diretorioDestino += "\\" + JWT_CLAIMS.CDGPED;
                                                                        }
                                                                        else if (JWT_CLAIMS.TABELA == "CL_PROCESSO_ANEXO")
                                                                        {
                                                                            diretorioDestino += "\\" + JWT_CLAIMS.CL_CDG;
                                                                        }

                                                                        // Verifica se a pasta do pedido existe, e caso não, cria a pasta
                                                                        if (!Directory.Exists(diretorioDestino))
                                                                        {
                                                                            Directory.CreateDirectory(diretorioDestino);
                                                                        }

                                                                        // Define os Parametros bases da importação do arquivo
                                                                        string nomeArquivoUnico = null;
                                                                        string caminhoDestino = null;

                                                                        // Realiza o registro do anexo no banco de dados
                                                                        if (JWT_CLAIMS.TABELA == "PedidosItens_Temp")
                                                                        {
                                                                            // Realiza a Importação do Anexo
                                                                            if (!EmarADO.PROCESSA_ANEXO_PEDIDOITENS(
                                                                                _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                                JWT_CLAIMS.CDGPED,
                                                                                nomeOriginal,
                                                                                JWT_CLAIMS.CODPROD,
                                                                                JWT_CLAIMS.CGCF))
                                                                            {
                                                                                // Gera o Log de operação do Robô
                                                                                LogMensagem = "O Serviço foi interrompido pois não foi possível salvar o arquivo no banco.";
                                                                                GeraLog = EmarADO.GERA_LOG(
                                                                                    _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                                    LogMensagem
                                                                                    );

                                                                                // Retorna erro
                                                                                return APIResponseHelper.EstruturaResponse(
                                                                                    "Ops",
                                                                                    LogMensagem,
                                                                                    "error",
                                                                                    null,
                                                                                    400,
                                                                                    Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                                                            }

                                                                            // Verifica se este arquivo ja existe e caso sim, gera um nome unico para este arquivo
                                                                            nomeArquivoUnico = ArquivoHelper.ObterNomeUnico(diretorioDestino, nomeOriginal);

                                                                            // Seta o caminho completo de destino
                                                                            caminhoDestino = Path.Combine(diretorioDestino, nomeArquivoUnico);

                                                                            // Salva o arquivo
                                                                            System.IO.File.WriteAllBytes(caminhoDestino, anexo.ContentBytes);
                                                                        }
                                                                        else if (JWT_CLAIMS.TABELA == "CL_PROCESSO_ANEXO")
                                                                        {
                                                                            // Realiza a Importação do Anexo
                                                                            if (!EmarADO.PROCESSA_ANEXO_CL_PROCESSO_ANEXO(
                                                                                _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                                JWT_CLAIMS.CL_CDG,
                                                                                JWT_CLAIMS.TIPO,
                                                                                nomeOriginal,
                                                                                JWT_CLAIMS.DISPONIVEL_FORNEC))
                                                                            {
                                                                                // Gera o Log de operação do Robô
                                                                                LogMensagem = "O Serviço foi interrompido pois não foi possível salvar o arquivo no banco.";
                                                                                GeraLog = EmarADO.GERA_LOG(
                                                                                    _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                                    LogMensagem
                                                                                    );

                                                                                // Retorna erro
                                                                                return APIResponseHelper.EstruturaResponse(
                                                                                    "Ops",
                                                                                    LogMensagem,
                                                                                    "error",
                                                                                    null,
                                                                                    400,
                                                                                    Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                                                            }

                                                                            // Verifica se este arquivo ja existe e caso sim, gera um nome unico para este arquivo
                                                                            nomeArquivoUnico = ArquivoHelper.ObterNomeUnico(diretorioDestino, nomeOriginal);

                                                                            // Seta o caminho completo de destino
                                                                            caminhoDestino = Path.Combine(diretorioDestino, nomeArquivoUnico);

                                                                            // Salva o arquivo
                                                                            System.IO.File.WriteAllBytes(caminhoDestino, anexo.ContentBytes);
                                                                        }
                                                                        else if (JWT_CLAIMS.TABELA == "PEDIDOS_ARQUIVOS")
                                                                        {
                                                                            // Realiza a Importação do Anexo
                                                                            int ID_Arquivo = EmarADO.PROCESSA_PEDIDOS_ARQUIVOS(
                                                                                _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                                JWT_CLAIMS.CDGPED,
                                                                                JWT_CLAIMS.TIPO,
                                                                                nomeOriginal);

                                                                            if (ID_Arquivo == 0)
                                                                            {
                                                                                // Gera o Log de operação do Robô
                                                                                LogMensagem = "O Serviço foi interrompido pois não foi possível salvar o arquivo no banco.";
                                                                                GeraLog = EmarADO.GERA_LOG(
                                                                                    _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                                    LogMensagem
                                                                                    );

                                                                                // Retorna erro
                                                                                return APIResponseHelper.EstruturaResponse(
                                                                                    "Ops",
                                                                                    LogMensagem,
                                                                                    "error",
                                                                                    null,
                                                                                    400,
                                                                                    Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                                                            }

                                                                            // Pega a Extensão do Arquivo
                                                                            string[] nomeArquivoSplit = nomeOriginal.Split(".");

                                                                            // Parametriza o ID como Nome do Arquivo
                                                                            nomeArquivoUnico = ID_Arquivo.ToString() + "." + nomeArquivoSplit[nomeArquivoSplit.Count() - 1];

                                                                            // Seta o caminho completo de destino
                                                                            caminhoDestino = Path.Combine(diretorioDestino, nomeArquivoUnico);

                                                                            // Salva o arquivo
                                                                            System.IO.File.WriteAllBytes(caminhoDestino, anexo.ContentBytes);
                                                                        }

                                                                        // Adiciona ao Contador de anexosProcessados
                                                                        anexosProcessados++;
                                                                    }
                                                                }

                                                                // Valida se houve anexos processados
                                                                if (anexosProcessados > 0)
                                                                {
                                                                    // Estrutura o log do Robô
                                                                    logProcessamentoModel.Data = DateTime.Now.ToString();
                                                                    logProcessamentoModel.Status = "PR";
                                                                    logProcessamentoModel.Descricao = "O Email foi processado com sucesso";

                                                                    // Envia a Mensagem e ja encerra a conexão com o websocket
                                                                    webSocketMensagemEnviada = EmarHelper.EnviaMensagemWebSocket(logProcessamentoModel, webSocketCliente, true);

                                                                    // Gera o Log de Processamento
                                                                    if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                                                                        _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                        JWT_CLAIMS.CGC,
                                                                        JWT_CLAIMS.CCUSTO,
                                                                        JWT_CLAIMS.REQUISIT,
                                                                        JWT_CLAIMS.CDGPED,
                                                                        JWT_CLAIMS.CL_CDG,
                                                                        JWT_CLAIMS.CODPROD,
                                                                        JWT_CLAIMS.CODITEM,
                                                                        JWT_CLAIMS.CGCF,
                                                                        email.Id,
                                                                        email.Subject,
                                                                        email.Sender.EmailAddress.Name,
                                                                        email.Sender.EmailAddress.Address,
                                                                        email.Body.Content,
                                                                        (bool)email.HasAttachments ? "S" : "N",
                                                                        nomesAnexos,
                                                                        jwtEmail,
                                                                        GeradorClaimsJWT.ConverteClaimsParaString(JWT_CLAIMS),
                                                                        logProcessamentoModel.Status,
                                                                        logProcessamentoModel.Descricao))
                                                                    {
                                                                        // Gera o Log de operação do Robô
                                                                        LogMensagem = "Não foi possível gerar o log de processamento do Email Processado";
                                                                        GeraLog = EmarADO.GERA_LOG(
                                                                            _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                            LogMensagem
                                                                            );

                                                                        // Retorna erro
                                                                        return APIResponseHelper.EstruturaResponse(
                                                                            "Ops",
                                                                            LogMensagem,
                                                                            "error",
                                                                            null,
                                                                            400,
                                                                            Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                                                    }

                                                                    // Seta a pasta de destino
                                                                    pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:ProcessFolder");

                                                                    // Adiciona o email processado a array
                                                                    emailsProcessados.Add(email);
                                                                }
                                                                else
                                                                {
                                                                    // Estrutura o log do Robô
                                                                    logProcessamentoModel.Data = DateTime.Now.ToString();
                                                                    logProcessamentoModel.Status = "NP";
                                                                    logProcessamentoModel.Descricao = "O Email não foi processado pois o(s) anexo(s) foram identificados como parte da assinatura ou do corpo do Email";

                                                                    // Envia a Mensagem e ja encerra a conexão com o websocket
                                                                    webSocketMensagemEnviada = EmarHelper.EnviaMensagemWebSocket(logProcessamentoModel, webSocketCliente, true);

                                                                    // Gera o Log de Processamento
                                                                    if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                                                                        _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                        JWT_CLAIMS.CGC,
                                                                        JWT_CLAIMS.CCUSTO,
                                                                        JWT_CLAIMS.REQUISIT,
                                                                        JWT_CLAIMS.CDGPED,
                                                                        JWT_CLAIMS.CL_CDG,
                                                                        JWT_CLAIMS.CODPROD,
                                                                        JWT_CLAIMS.CODITEM,
                                                                        JWT_CLAIMS.CGCF,
                                                                        email.Id,
                                                                        email.Subject,
                                                                        email.Sender.EmailAddress.Name,
                                                                        email.Sender.EmailAddress.Address,
                                                                        email.Body.Content,
                                                                        (bool)email.HasAttachments ? "S" : "N",
                                                                        nomesAnexos,
                                                                        jwtEmail,
                                                                        GeradorClaimsJWT.ConverteClaimsParaString(JWT_CLAIMS),
                                                                        logProcessamentoModel.Status,
                                                                        logProcessamentoModel.Descricao))
                                                                    {
                                                                        // Gera o Log de operação do Robô
                                                                        LogMensagem = "Não foi possível gerar o log de processamento do Email com Anexo referente a assinatura do Email";
                                                                        GeraLog = EmarADO.GERA_LOG(
                                                                            _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                            LogMensagem
                                                                            );

                                                                        // Retorna erro
                                                                        return APIResponseHelper.EstruturaResponse(
                                                                            "Ops",
                                                                            LogMensagem,
                                                                            "error",
                                                                            null,
                                                                            400,
                                                                            Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                                                    }

                                                                    // Seta a pasta de destino
                                                                    pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:UnprocessedFolder");

                                                                    // Adiciona o email processado a array
                                                                    emailsNaoProcessados.Add(email);
                                                                }
                                                            }
                                                            // Verifica se o e-mail contém algum arquivo inválido e caso sim, gera erro e não processa o e-mail
                                                            else
                                                            {
                                                                // Estrutura o log do Robô
                                                                logProcessamentoModel.Data = DateTime.Now.ToString();
                                                                logProcessamentoModel.Status = "NP";
                                                                logProcessamentoModel.Descricao = "O Email não foi processado pois este(s) tipo(s) de anexo(s) não são permitido(s) ou estão vazio(s)";

                                                                // Envia a Mensagem e ja encerra a conexão com o websocket
                                                                webSocketMensagemEnviada = EmarHelper.EnviaMensagemWebSocket(logProcessamentoModel, webSocketCliente, true);

                                                                // Gera o Log de Processamento
                                                                if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                                                                    _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                    JWT_CLAIMS.CGC,
                                                                    JWT_CLAIMS.CCUSTO,
                                                                    JWT_CLAIMS.REQUISIT,
                                                                    JWT_CLAIMS.CDGPED,
                                                                    JWT_CLAIMS.CL_CDG,
                                                                    JWT_CLAIMS.CODPROD,
                                                                    JWT_CLAIMS.CODITEM,
                                                                    JWT_CLAIMS.CGCF,
                                                                    email.Id,
                                                                    email.Subject,
                                                                    email.Sender.EmailAddress.Name,
                                                                    email.Sender.EmailAddress.Address,
                                                                    email.Body.Content,
                                                                    (bool)email.HasAttachments ? "S" : "N",
                                                                    nomesAnexos,
                                                                    jwtEmail,
                                                                    GeradorClaimsJWT.ConverteClaimsParaString(JWT_CLAIMS),
                                                                    logProcessamentoModel.Status,
                                                                    logProcessamentoModel.Descricao))
                                                                {
                                                                    // Gera o Log de operação do Robô
                                                                    LogMensagem = "Não foi possível gerar o log de processamento do Email Processado pois o tipo de anexo não é permitido";
                                                                    GeraLog = EmarADO.GERA_LOG(
                                                                        _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                                        LogMensagem
                                                                        );

                                                                    // Retorna erro
                                                                    return APIResponseHelper.EstruturaResponse(
                                                                        "Ops",
                                                                        LogMensagem,
                                                                        "error",
                                                                        null,
                                                                        400,
                                                                        Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                                                }

                                                                // Seta a pasta de destino
                                                                pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:UnprocessedFolder");

                                                                // Adiciona o email processado a array
                                                                emailsNaoProcessados.Add(email);
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // Estrutura o log do Robô
                                                    logProcessamentoModel.Data = DateTime.Now.ToString();
                                                    logProcessamentoModel.Status = "NP";
                                                    logProcessamentoModel.Descricao = "O Email não foi processado pois está sem anexo";

                                                    // Envia a Mensagem e ja encerra a conexão com o websocket
                                                    webSocketMensagemEnviada = EmarHelper.EnviaMensagemWebSocket(logProcessamentoModel, webSocketCliente, true);

                                                    // Gera o Log de Processamento
                                                    if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                                                        _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                        JWT_CLAIMS.CGC,
                                                        JWT_CLAIMS.CCUSTO,
                                                        JWT_CLAIMS.REQUISIT,
                                                        JWT_CLAIMS.CDGPED,
                                                        JWT_CLAIMS.CL_CDG,
                                                        JWT_CLAIMS.CODPROD,
                                                        JWT_CLAIMS.CODITEM,
                                                        JWT_CLAIMS.CGCF,
                                                        email.Id,
                                                        email.Subject,
                                                        email.Sender.EmailAddress.Name,
                                                        email.Sender.EmailAddress.Address,
                                                        email.Body.Content,
                                                        (bool)email.HasAttachments ? "S" : "N",
                                                        null,
                                                        jwtEmail,
                                                        GeradorClaimsJWT.ConverteClaimsParaString(JWT_CLAIMS),
                                                        logProcessamentoModel.Status,
                                                        logProcessamentoModel.Descricao))
                                                    {
                                                        // Gera o Log de operação do Robô
                                                        LogMensagem = "Não foi possível gerar o log de processamento do Email sem Anexo";
                                                        GeraLog = EmarADO.GERA_LOG(
                                                            _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                            LogMensagem
                                                            );

                                                        // Retorna erro
                                                        return APIResponseHelper.EstruturaResponse(
                                                            "Ops",
                                                            LogMensagem,
                                                            "error",
                                                            null,
                                                            400,
                                                            Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                                    }

                                                    // Seta a pasta de destino
                                                    pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:UnprocessedFolder");

                                                    // Adiciona o email processado a array
                                                    emailsNaoProcessados.Add(email);
                                                }
                                            }
                                            else
                                            {
                                                // Estrutura o log do Robô
                                                logProcessamentoModel.Data = DateTime.Now.ToString();
                                                logProcessamentoModel.Status = "NP";
                                                logProcessamentoModel.Descricao = "O Email não foi processado pois a estrutura do Token é inválida";

                                                // Envia a Mensagem e ja encerra a conexão com o websocket
                                                webSocketMensagemEnviada = EmarHelper.EnviaMensagemWebSocket(logProcessamentoModel, webSocketCliente, true);

                                                // Gera o Log de Processamento
                                                if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                                                    _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                    null,
                                                    null,
                                                    null,
                                                    null,
                                                    null,
                                                    null,
                                                    null,
                                                    null,
                                                    email.Id,
                                                    email.Subject,
                                                    email.Sender.EmailAddress.Name,
                                                    email.Sender.EmailAddress.Address,
                                                    email.Body.Content,
                                                    (bool)email.HasAttachments ? "S" : "N",
                                                    null,
                                                    jwtEmail,
                                                    GeradorClaimsJWT.ConverteClaimsParaString(JWT_CLAIMS),
                                                    logProcessamentoModel.Status,
                                                    logProcessamentoModel.Descricao))
                                                {
                                                    // Gera o Log de operação do Robô
                                                    LogMensagem = "Não foi possível gerar o log de processamento do Email com Estrutura do Token Inválida";
                                                    GeraLog = EmarADO.GERA_LOG(
                                                        _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                        LogMensagem
                                                        );

                                                    // Retorna erro
                                                    return APIResponseHelper.EstruturaResponse(
                                                        "Ops",
                                                        LogMensagem,
                                                        "error",
                                                        null,
                                                        400,
                                                        Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                                }

                                                // Seta a pasta de destino
                                                pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:UnprocessedFolder");

                                                // Adiciona o email processado a array
                                                emailsNaoProcessados.Add(email);
                                            }
                                        }
                                        else
                                        {
                                            // Estrutura o log do Robô
                                            logProcessamentoModel.Data = DateTime.Now.ToString();
                                            logProcessamentoModel.Status = "NP";
                                            logProcessamentoModel.Descricao = "O Email não foi processado pois o Token enviado já foi utilizado anteriormente";

                                            // Envia a Mensagem e ja encerra a conexão com o websocket
                                            webSocketMensagemEnviada = EmarHelper.EnviaMensagemWebSocket(logProcessamentoModel, webSocketCliente, true);

                                            // Gera o Log de Processamento
                                            if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                                                _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                null,
                                                null,
                                                null,
                                                null,
                                                null,
                                                null,
                                                null,
                                                null,
                                                email.Id,
                                                email.Subject,
                                                email.Sender.EmailAddress.Name,
                                                email.Sender.EmailAddress.Address,
                                                email.Body.Content,
                                                (bool)email.HasAttachments ? "S" : "N",
                                                null,
                                                jwtEmail,
                                                null,
                                                logProcessamentoModel.Status,
                                                logProcessamentoModel.Descricao))
                                            {
                                                // Gera o Log de operação do Robô
                                                LogMensagem = "Não foi possível gerar o log de processamento do Email com Token Inválido";
                                                GeraLog = EmarADO.GERA_LOG(
                                                    _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                    LogMensagem
                                                    );

                                                // Retorna erro
                                                return APIResponseHelper.EstruturaResponse(
                                                    "Ops",
                                                    LogMensagem,
                                                    "error",
                                                    null,
                                                    400,
                                                    Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                            }

                                            // Seta a pasta de destino
                                            pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:UnprocessedFolder");

                                            // Adiciona o email processado a array
                                            emailsNaoProcessados.Add(email);
                                        }
                                    }
                                    else
                                    {
                                        // Estrutura o log do Robô
                                        logProcessamentoModel.Data = DateTime.Now.ToString();
                                        logProcessamentoModel.Status = "NP";
                                        logProcessamentoModel.Descricao = "O Email não foi processado pois o Token está inválido";

                                        // Envia a Mensagem e ja encerra a conexão com o websocket
                                        webSocketMensagemEnviada = EmarHelper.EnviaMensagemWebSocket(logProcessamentoModel, webSocketCliente, true);

                                        // Gera o Log de Processamento
                                        if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                                            _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                            email.Id,
                                            email.Subject,
                                            email.Sender.EmailAddress.Name,
                                            email.Sender.EmailAddress.Address,
                                            email.Body.Content,
                                            (bool)email.HasAttachments ? "S" : "N",
                                            null,
                                            jwtEmail,
                                            null,
                                            logProcessamentoModel.Status,
                                            logProcessamentoModel.Descricao))
                                        {
                                            // Gera o Log de operação do Robô
                                            LogMensagem = "Não foi possível gerar o log de processamento do Email com Token já utilizado.";
                                            GeraLog = EmarADO.GERA_LOG(
                                                _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                                LogMensagem
                                                );

                                            // Retorna erro
                                            return APIResponseHelper.EstruturaResponse(
                                                "Ops",
                                                LogMensagem,
                                                "error",
                                                null,
                                                400,
                                                Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                        }

                                        // Seta a pasta de destino
                                        pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:UnprocessedFolder");

                                        // Adiciona o email processado a array
                                        emailsNaoProcessados.Add(email);
                                    }
                                }
                            }
                            else
                            {
                                // Estrutura o log do Robô
                                logProcessamentoModel.Data = DateTime.Now.ToString();
                                logProcessamentoModel.Status = "NP";
                                logProcessamentoModel.Descricao = "O Email não foi processado pois o Token não contem informações válidas";

                                // Gera o Log de Processamento
                                if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                                    _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    email.Id,
                                    email.Subject,
                                    email.Sender.EmailAddress.Name,
                                    email.Sender.EmailAddress.Address,
                                    email.Body.Content,
                                    (bool)email.HasAttachments ? "S" : "N",
                                    null,
                                    jwtEmail,
                                    null,
                                    logProcessamentoModel.Status,
                                    logProcessamentoModel.Descricao))
                                {
                                    // Gera o Log de operação do Robô
                                    LogMensagem = "Não foi possível gerar o log de processamento do Email que contem informações invalidas";
                                    GeraLog = EmarADO.GERA_LOG(
                                        _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                        LogMensagem
                                        );

                                    // Retorna erro
                                    return APIResponseHelper.EstruturaResponse(
                                        "Ops",
                                        LogMensagem,
                                        "error",
                                        null,
                                        400,
                                        Url.Action("processar_emails", "Emar", null, Request.Scheme));
                                }

                                // Seta a pasta de destino
                                pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:UnprocessedFolder");

                                // Adiciona o email processado a array
                                emailsNaoProcessados.Add(email);
                            }
                        }
                        else
                        {
                            // Estrutura o log do Robô
                            logProcessamentoModel.Data = DateTime.Now.ToString();
                            logProcessamentoModel.Status = "NP";
                            logProcessamentoModel.Descricao = "O Email não foi processado pois esta sem Token";

                            // Gera o Log de Processamento
                            if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                                _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                null,
                                null,
                                null,
                                null,
                                null,
                                null,
                                null,
                                null,
                                email.Id,
                                email.Subject,
                                email.Sender.EmailAddress.Name,
                                email.Sender.EmailAddress.Address,
                                email.Body.Content,
                                (bool)email.HasAttachments ? "S" : "N",
                                null,
                                null,
                                null,
                                logProcessamentoModel.Status,
                                logProcessamentoModel.Descricao))
                            {
                                // Gera o Log de operação do Robô
                                LogMensagem = "Não foi possível gerar o log de processamento do Email de sem Token";
                                GeraLog = EmarADO.GERA_LOG(
                                    _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                    LogMensagem
                                    );

                                // Retorna erro
                                return APIResponseHelper.EstruturaResponse(
                                    "Ops",
                                    LogMensagem,
                                    "error",
                                    null,
                                    400,
                                    Url.Action("processar_emails", "Emar", null, Request.Scheme));
                            }

                            // Seta a pasta de destino
                            pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:UnprocessedFolder");

                            // Adiciona o email processado a array
                            emailsNaoProcessados.Add(email);
                        }
                    }
                    else
                    {
                        // Estrutura o log do Robô
                        logProcessamentoModel.Data = DateTime.Now.ToString();
                        logProcessamentoModel.Status = "SP";
                        logProcessamentoModel.Descricao = "O Email não foi processado por ser um SPAM";

                        // Gera o Log de Processamento
                        if (!EmarADO.GERA_LOG_PROCESSAMENTO(
                            _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            email.Id,
                            email.Subject,
                            email.Sender.EmailAddress.Name,
                            email.Sender.EmailAddress.Address,
                            email.Body.Content,
                            (bool)email.HasAttachments ? "S" : "N",
                            null,
                            null,
                            null,
                            logProcessamentoModel.Status,
                            logProcessamentoModel.Descricao))
                        {
                            // Gera o Log de operação do Robô
                            LogMensagem = "Não foi possível gerar o log de processamento do Email de Spam";
                            GeraLog = EmarADO.GERA_LOG(
                                _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                                LogMensagem
                                );

                            // Retorna erro
                            return APIResponseHelper.EstruturaResponse(
                                "Ops",
                                LogMensagem,
                                "error",
                                null,
                                400,
                                Url.Action("processar_emails", "Emar", null, Request.Scheme));
                        }

                        // Seta a pasta de destino
                        pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:SpamFolder");

                        // Adiciona o email spam a array
                        emailsSpam.Add(email);
                    }

                    // Try/Catch para caso o processamento do e-mail cai aqui e o email não seja do mesmo ambiente
                    try
                    {
                        // Faz a leitura do Email
                        var visualizaEmail = await ambienteGraph.Users[userMailId].MailFolders["inbox"].Messages[email.Id].PatchAsync(
                            new Microsoft.Graph.Models.Message
                            {
                                IsRead = true,
                            });

                        // Move o Email para a Pasta de Processados
                        var moveEmail = await ambienteGraph.Users[userMailId].MailFolders["inbox"].Messages[email.Id].Move.PostAsync(
                            new Microsoft.Graph.Users.Item.MailFolders.Item.Messages.Item.Move.MovePostRequestBody
                            {
                                DestinationId = pastaDestino
                            });
                    }
                    catch(Exception ex)
                    {

                    }

                }
            }

            // Gera o Log de operação do Robô
            LogMensagem = "Finalização do Processamento de Anexos via E-Mail - ";
            LogMensagem += "Total de Emails Processados: "+emailsProcessados.Count()+", ";
            LogMensagem += "Total de Emails Não Processados: " + emailsNaoProcessados.Count()+", ";
            LogMensagem += "Total de Emails de Spam: " + emailsSpam.Count()+".";

            GeraLog = EmarADO.GERA_LOG(
                _configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
                LogMensagem
                );

            // Cria a estrutura de retorno
            object Retorno = new {
                EmailsProcessados = emailsProcessados.Count(),
                EmailsNaoProcessados = emailsNaoProcessados.Count(),
                EmailsSpam = emailsSpam.Count(),
            };

            // Retorna a consulta
            return APIResponseHelper.EstruturaResponse(
                "Sucesso",
                "Emails Processados com Sucesso",
                "success",
                Retorno,
                200,
                Url.Action("processar_emails", "Emar", null, Request.Scheme));
        }
    }
}
