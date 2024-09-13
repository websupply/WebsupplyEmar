using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
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

        public static ObjectResult EstruturaResponseDataTable(string MensagemTitulo, string MensagemTexto, string MensagemTipo, object? Retorno, dynamic? Meta, int StatusRequisicao, string Link_Referencia)
        {
            dynamic ResponseAPI = new ExpandoObject();

            ResponseAPI.Mensagem = new
            {
                Titulo = MensagemTitulo,
                Texto = MensagemTexto,
                Tipo = MensagemTipo
            };

            ResponseAPI.Requisicao = new
            {
                Status = StatusRequisicao,
                Link_Referencia = Link_Referencia
            };

            // Adiciona o Retorno
            ResponseAPI.data = Retorno;

            // Adiciona os Dados Meta
            ResponseAPI.draw = Meta.draw;
            ResponseAPI.pages = Meta.pages;
            ResponseAPI.recordsFiltered = Meta.recordsFiltered;
            ResponseAPI.recordsTotal = Meta.recordsTotal;

            return new ObjectResult(ResponseAPI)
            {
                StatusCode = StatusRequisicao
            };
        }
    }
}
