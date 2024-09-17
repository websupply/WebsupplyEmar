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
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using WebsupplyEmar.API.Services;
using Microsoft.Graph.Models.Security;
using Tavis.UriTemplates;

namespace WebsupplyEmar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CockpitController : Controller
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IConfiguration _configuration;

        public CockpitController(IConfiguration configuration, IWebSocketService webSocketService)
        {
            _configuration = configuration;
            _webSocketService = webSocketService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Caminho para o arquivo index.html dentro da pasta wwwroot
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
            return PhysicalFile(filePath, "text/html");
        }

        [HttpPost]
        [Route("cards")]
        public ObjectResult Cards(CockpitRequestDto objRequest)
        {
            // Cria o Objeto de Resposta
            CockpitResponseDto objResponse = new CockpitResponseDto
            {
                card = new List<CockpitResponseDto.Card>()
            };

            // Consulta os Cards
            objResponse = CockpitADO.CONSULTA_CARDS(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), objRequest, objResponse);

            // Monta o Retorno
            object result = new
            {
                _card = objResponse.card
            };

            // Retorna a consulta
            return APIResponseHelper.EstruturaResponse(
                "Sucesso",
                "Dados do Cockpit Gerados com Sucesso",
                "success",
                result,
                200,
                Url.Action("inicio", "Cockpit", null, Request.Scheme));
        }

        [HttpPost]
        [Route("logs_emar")]
        public ObjectResult Logs_Emar(CockpitRequestDto objRequest)
        {
            // Cria o Objeto de Resposta
            CockpitResponseDto objResponse = new CockpitResponseDto
            {
                logsEmar = new List<CockpitResponseDto.LogsEmar>()
            };

            // Consulta os Logs do Robô
            objResponse = CockpitADO.CONSULTA_EMAR_LOGS(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), objRequest, objResponse);

            // Dados Meta
            object meta = new
            {
                draw = objRequest.Draw,
                pages = objResponse.RecordsTotal / objRequest.Length,
                recordsFiltered = objResponse.RecordsFiltered,
                recordsTotal = objResponse.RecordsTotal,
            };

            // Retorna a consulta
            return APIResponseHelper.EstruturaResponseDataTable(
                "Sucesso",
                "Logs do Emar Consultados com Sucesso",
                "success",
                objResponse.logsEmar,
                meta,
                200,
                Url.Action("logs_emar", "Cockpit", null, Request.Scheme));
        }

        [HttpPost]
        [Route("logs_websocket")]
        public ObjectResult Logs_WebSocket(CockpitRequestDto objRequest)
        {
            // Cria o Objeto de Resposta
            CockpitResponseDto objResponse = new CockpitResponseDto
            {
                logsWebsocket = new List<CockpitResponseDto.LogsWebsocket>()
            };

            // Consulta os Logs do Websocket
            objResponse = CockpitADO.CONSULTA_WEBSOCKET_LOGS(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), objRequest, objResponse);

            // Dados Meta
            object meta = new
            {
                draw = objRequest.Draw,
                pages = objResponse.RecordsTotal / objRequest.Length,
                recordsFiltered = objResponse.RecordsFiltered,
                recordsTotal = objResponse.RecordsTotal,
            };

            // Retorna a consulta
            return APIResponseHelper.EstruturaResponseDataTable(
                "Sucesso",
                "Logs do WebSocket Consultados com Sucesso",
                "success",
                objResponse.logsWebsocket,
                meta,
                200,
                Url.Action("logs_websocket", "Cockpit", null, Request.Scheme));
        }

        [HttpPost]
        [Route("logs_emar_processamentos")]
        public ObjectResult Logs_Emar_Processamento(CockpitRequestDto objRequest)
        {
            // Cria o Objeto de Resposta
            CockpitResponseDto objResponse = new CockpitResponseDto
            {
                logsEmarProcessamento = new List<CockpitResponseDto.LogsEmarProcessamento>()
            };

            // Consulta os Logs de Processamento do Robô
            objResponse = CockpitADO.CONSULTA_EMAR_LOGS_PROCESSAMENTO(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), objRequest, objResponse);

            // Dados Meta
            object meta = new
            {
                draw = objRequest.Draw,
                pages = objResponse.RecordsTotal / objRequest.Length,
                recordsFiltered = objResponse.RecordsFiltered,
                recordsTotal = objResponse.RecordsTotal,
            };

            // Retorna a consulta
            return APIResponseHelper.EstruturaResponseDataTable(
                "Sucesso",
                "Logs de Processamento do Robô Consultados com Sucesso",
                "success",
                objResponse.logsEmarProcessamento,
                meta,
                200,
                Url.Action("logs_emar_processamentos", "Cockpit", null, Request.Scheme));
        }

        [HttpPost]
        [Route("dados_servidor_websocket")]
        public ObjectResult Servidor_WebSocket()
        {
            // Monta o Retorno
            object result = new
            {
                _infoWebSocket = _webSocketService.GetWebSocketInfo()
            };

            // Retorna a consulta
            return APIResponseHelper.EstruturaResponse(
                "Sucesso",
                "Dados do Servidor WebSocket Carregados com Sucesso",
                "success",
                result,
                200,
                Url.Action("dados_servidor_websocket", "Cockpit", null, Request.Scheme));
        }

        [HttpPost]
        [Route("dados_ambiente")]
        public ObjectResult Dados_Ambiente()
        {
            string host = Request.Host.Host;

            X509Certificate2 certSSL = SSLHelper.ConsultaHashCertificado(host);

            // Gera o retorno
            object result = new
            {
                _infoAmbiente = new
                {
                    Host = host,
                    SSL = new
                    {
                        Hash = BitConverter.ToString(certSSL.GetCertHash()).Replace("-", ""),
                        InicioValidade = certSSL.NotBefore,
                        FimValidade = certSSL.NotAfter

                    }
                }
            };

            // Retorna a consulta
            return APIResponseHelper.EstruturaResponse(
                "Sucesso",
                "Dados do Ambiente Carregados com Sucesso",
                "success",
                result,
                200,
                Url.Action("dados_ambiente", "Cockpit", null, Request.Scheme));
        }

        //[HttpPost]
        //[Route("consulta_certificado")]
        //public ObjectResult Consulta([FromForm] string hostname1, [FromForm] string hostname2)
        //{
        //    X509Certificate2 cert1 = SSLHelper.ConsultaHashCertificado(hostname1);
        //    X509Certificate2 cert2 = SSLHelper.ConsultaHashCertificado(hostname1);

        //    if (cert1 != null && cert2 != null)
        //    {
        //        // Exibir os hashes dos certificados
        //        Console.WriteLine($"Hash do certificado de {hostname1}: {BitConverter.ToString(cert1.GetCertHash()).Replace("-", "")}");
        //        Console.WriteLine($"Hash do certificado de {hostname2}: {BitConverter.ToString(cert2.GetCertHash()).Replace("-", "")}");

        //        // Comparar os hashes
        //        bool hashesAreEqual = SSLHelper.ComparaHashSSL(cert1.GetCertHash(), cert2.GetCertHash());
        //        Console.WriteLine($"Os hashes são iguais? {hashesAreEqual}");

        //        // Exibir as datas de validade dos certificados
        //        Console.WriteLine($"\nValidade do certificado de {hostname1}:");
        //        Console.WriteLine($"Início da validade: {cert1.NotBefore}");
        //        Console.WriteLine($"Fim da validade: {cert1.NotAfter}");

        //        Console.WriteLine($"\nValidade do certificado de {hostname2}:");
        //        Console.WriteLine($"Início da validade: {cert2.NotBefore}");
        //        Console.WriteLine($"Fim da validade: {cert2.NotAfter}");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Não foi possível obter os certificados de ambos os hostnames.");
        //    }

        //    // Gera o retorno
        //    object Retorno = new
        //    {
        //        Token = "[]"
        //    };

        //    // Retorna a consulta
        //    return APIResponseHelper.EstruturaResponse(
        //        "Sucesso",
        //        "Token Gerado com Sucesso",
        //        "success",
        //        Retorno,
        //        200,
        //        Url.Action("gerar_hash", "Emar", null, Request.Scheme));
        //}
    }
}
