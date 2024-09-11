using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsupplyEmar.Dominio.Dto
{
    public class CockpitResponseDto
    {
        // Parametros de Acesso
        public List<Card> card { get; set; }
        public string TotalMensagensRecebidas { get; set; }
        public string TotalConexoesUltimaHora { get; set; }
        public List<LogsEmar> logsEmar { get; set; }
        public List<LogsEmarProcessamento> logsEmarProcessamento { get; set; }
        public List<LogsWebsocket> logsWebsocket { get; set; }

        // Classes de Retorno
        public class Card
        {
            public string Tipo { get; set; }
            public string Valor { get; set; }
        }

        public class LogsEmar
        {
            public string Log { get; set; }
            public string DataHorario { get; set; }
        }

        public class LogsEmarProcessamento
        {
            public string Log { get; set; }
            public string DataHorario { get; set; }
        }

        public class LogsWebsocket
        {
            public string Log { get; set; }
            public string DataHorario { get; set; }
        }
    }
}
