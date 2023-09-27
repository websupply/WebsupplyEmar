using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Security.Policy;

namespace WebsupplyEmar.API.Helpers
{
    public class APIResponseHelper
    {
        public static ObjectResult EstruturaResponse(string MensagemTitulo, string MensagemTexto, string MensagemTipo, object? Retorno, int StatusRequisicao, string Link_Referencia)
        {
            dynamic ResponseAPI = new ExpandoObject();

            ResponseAPI.Mensagem = new
            {
                Titulo = MensagemTitulo,
                Texto = MensagemTexto,
                Tipo = MensagemTipo
            };

            if (Retorno != null)
            {
                ResponseAPI.Requisicao = new
                {
                    Retorno = Retorno,
                    Status = StatusRequisicao,
                    Link_Referencia = Link_Referencia
                };
            }
            else
            {
                ResponseAPI.Requisicao = new
                {
                    Status = StatusRequisicao,
                    Link_Referencia = Link_Referencia
                };
            }

            return new ObjectResult(ResponseAPI)
            {
                StatusCode = StatusRequisicao
            };
        }
    }
}
