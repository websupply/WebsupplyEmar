using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebsupplyEmar.API.Funcoes;
using WebsupplyEmar.Dados.ADO;
using WebsupplyEmar.Dominio.Dto;

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
        [Route("receber-emails")]
        public ObjectResult RECEBER_EMAILS(EmarRequestDto objEmarRequest)
        {
            LocalhostMail localhostMail = new LocalhostMail(_configuration);
            if(objEmarRequest.Servidor.ToUpper() == "POP" || objEmarRequest.Servidor.ToUpper() == "POP3")
            {
                object objEmails = localhostMail.receberEmailPop(objEmarRequest.SSL);
                return new ObjectResult(objEmails);
            }
            else if(objEmarRequest.Servidor.ToUpper() == "IMAP")
            {
                object objEmails = localhostMail.receberEmailImap(objEmarRequest.SSL);
                return new ObjectResult(objEmails);
            }
            else
            {
                return new ObjectResult(new
                {
                    Mensagem = "Permitido somente servidor POP ou IMAP"
                });
            }
        }

        [HttpPost]
        [Route("gerar-hash")]
        public ObjectResult GERAR_HASH(ClaimsRequestDto objClaimsRequest)
        {
            GeradorClaimsJWT geradorClaimsJWT = new GeradorClaimsJWT(objClaimsRequest.APP_URL);

            return new ObjectResult(new
            {
                Token = geradorClaimsJWT.CriaClaims(
                    objClaimsRequest.CDGPED,
                    objClaimsRequest.CODPROD,
                    objClaimsRequest.CODITEM,
                    objClaimsRequest.CGCF)
            });
        }
    }
}
