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

namespace WebsupplyEmar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CockpitController : Controller
    {
        private readonly IConfiguration _configuration;

        public CockpitController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("inicio")]
        public ObjectResult Inicio(CockpitRequestDto objRequest)
        {
            // Cria o Objeto de Resposta
            CockpitResponseDto objResponse = new CockpitResponseDto();

            // Consulta os Cards
            objResponse = CockpitADO.CONSULTA_CARDS(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), objRequest, objResponse);

            // Consulta os Logs do Websocket
            objResponse = CockpitADO.CONSULTA_WEBSOCKET_LOGS(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), objRequest, objResponse);

            // Consulta os Logs do Robô
            objResponse = CockpitADO.CONSULTA_EMAR_LOGS(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), objRequest, objResponse);

            // Consulta os Logs de Processamento do Robô
            objResponse = CockpitADO.CONSULTA_EMAR_LOGS_PROCESSAMENTO(_configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), objRequest, objResponse);

            // Retorna a consulta
            return APIResponseHelper.EstruturaResponse(
                "Sucesso",
                "Dados do Cockpit Gerados com Sucesso",
                "success",
                objResponse,
                200,
                Url.Action("inicio", "Cockpit", null, Request.Scheme));
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
