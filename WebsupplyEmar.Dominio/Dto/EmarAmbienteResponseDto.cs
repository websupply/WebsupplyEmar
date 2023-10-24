using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsupplyEmar.Dominio.Dto
{
    public class EmarAmbienteResponseDto
    {
        public int COD_AMBIENTE_ARQUIVOS { get; set; }
        public string CGCMatriz { get; set; }
        public string Ambiente { get; set; }
        public string DriverFisicoArquivos { get; set; }
        public DateTime DATAHORARIO_CADASTRO { get; set; }
        public DateTime? DATAHORARIO_ATUALIZACAO { get; set; }
        public string INDEXCLUI { get; set; }
    }
}
