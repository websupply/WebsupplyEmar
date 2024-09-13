using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsupplyEmar.Dominio.Model;

namespace WebsupplyEmar.Dominio.Dto
{
    public class CockpitResponseDto : DataTableModel
    {
        // Parametros de Acesso
        public List<Card> card { get; set; }
        public string TotalMensagensRecebidas { get; set; }
        public string TotalConexoesUltimaHora { get; set; }
        public List<LogsEmar> logsEmar { get; set; } = new List<LogsEmar>() { new LogsEmar() };
        public List<LogsEmarProcessamento> logsEmarProcessamento { get; set; } = new List<LogsEmarProcessamento>() { new LogsEmarProcessamento() };
        public List<LogsWebsocket> logsWebsocket { get; set; } = new List<LogsWebsocket>() { new LogsWebsocket() };

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
