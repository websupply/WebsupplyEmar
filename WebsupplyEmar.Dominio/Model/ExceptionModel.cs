using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsupplyEmar.Dominio.Model
{
    public class ExceptionModel
    {
        public int Linha { get; set; }
        public int Coluna { get; set; }
        public string Descricao { get; set; }
        public string Arquivo { get; set; }
        public string Funcao { get; set; }
        public string Mensagem { get; set; }
        public string MensagemCompleta { get; set; }

        public ExceptionModel(Exception excecao, bool dadosArquivo)
        {
            // Define o StackTrace
            StackTrace Rastreamento = new StackTrace(excecao, dadosArquivo);

            // Define o Total de Frames
            int Frames = Rastreamento.GetFrames().Count();
            int FrameDetalhado = (Frames == 0 ? 0 : Frames - 1);

            // Pega a Linha da Excepetion
            Linha = Rastreamento.GetFrame(FrameDetalhado).GetFileLineNumber();

            // Pega a Coluna da Excepetion
            Coluna = Rastreamento.GetFrame(FrameDetalhado).GetFileColumnNumber();

            // Pega o Nome do Arquivo da Excepetion
            Arquivo = Rastreamento.GetFrame(FrameDetalhado).GetFileName();

            // Pega a Função/Metodo que ocorreu a Excepetion
            Funcao = Rastreamento.GetFrame(FrameDetalhado).GetMethod().ToString();

            // Pega a Mensagem interna da Excepetion
            Descricao = excecao.Message;

            // Define a mensagem padrão da Excepetiond
            Mensagem = $"---> Ocorreu a seguinte excepetion: \n{Descricao} em {Funcao} no arquivo {Arquivo} ({Linha}, {Coluna}).";

            // Define a mensagem completa
            MensagemCompleta = $"---> Excepetion Completa: \n{excecao.StackTrace}";
        }
    }
}
