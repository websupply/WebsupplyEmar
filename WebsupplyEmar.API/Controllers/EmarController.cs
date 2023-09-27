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

namespace WebsupplyEmar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmarController : Controller
    {
        private readonly IConfiguration _configuration;

        public EmarController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("gerar_hash")]
        public ObjectResult GERAR_HASH(ClaimsRequestDto objClaimsRequest)
        {
            // Gera o Token
            GeradorClaimsJWT geradorClaimsJWT = new GeradorClaimsJWT(objClaimsRequest.APP_URL);

            // Gera o retorno
            object Retorno = new
            {
                Token = geradorClaimsJWT.CriaClaims(
                    objClaimsRequest.CDGPED,
                    objClaimsRequest.CODPROD,
                    objClaimsRequest.CODITEM,
                    objClaimsRequest.CGCF)
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
            });

            // cria as arrays para gerenciamento dos Emails
            List<Microsoft.Graph.Models.Message> emailsProcessados = new List<Microsoft.Graph.Models.Message>();
            List<Microsoft.Graph.Models.Message> emailsNaoProcessados = new List<Microsoft.Graph.Models.Message>();
            List<Microsoft.Graph.Models.Message> emailsSpam = new List<Microsoft.Graph.Models.Message>();

            // Faz a Leitura dos E-Mails e também o processamento
            if (arrayEmails.Value.Count() > 0 )
            {
                foreach(var email in arrayEmails.Value)
                {
                    // Declara as variaveis
                    string pastaDestino = "";

                    // Verifica se o Email tem o assunto de processamento, caso não, envia direto para a pasta de lixo eletronico
                    if (email.Subject.ToUpper().IndexOf("[PROCESSAMENTO DE ANEXO]") > -1)
                    {
                        // Verifica se o E-Mail possui anexo
                        if((bool)email.HasAttachments)
                        {
                            // Consulta os anexos
                            var consultaAnexos = await ambienteGraph.Users[userMailId].MailFolders["inbox"].Messages[email.Id].Attachments.GetAsync();

                            foreach(FileAttachment anexo in consultaAnexos.Value)
                            {
                                // Declara as variaveis de gestão do arquivo
                                string nomeOriginal = anexo.Name;
                                string diretorioDestino = @"C:\uploads\";

                                // Verifica se este arquivo ja existe e caso sim, gera um nome unico para este arquivo
                                string nomeArquivoUnico = FileHelper.ObterNomeUnico(diretorioDestino, nomeOriginal);

                                // Seta o caminho completo de destino
                                string caminhoDestino = Path.Combine(diretorioDestino, nomeArquivoUnico);

                                // Salva o arquivo
                                System.IO.File.WriteAllBytes(caminhoDestino, anexo.ContentBytes);
                            }

                            // Seta a pasta de destino
                            pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:ProcessFolder");

                            // Adiciona o email processado a array
                            emailsProcessados.Add(email);
                        }
                        else
                        {
                            // Seta a pasta de destino
                            pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:UnprocessedFolder");

                            // Adiciona o email processado a array
                            emailsNaoProcessados.Add(email);
                        }
                    }
                    else
                    {
                        // Seta a pasta de destino
                        pastaDestino = _configuration.GetValue<string>("EmarWebsupplyWeb:SpamFolder");

                        // Adiciona o email spam a array
                        emailsSpam.Add(email);
                    }

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
            }

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
