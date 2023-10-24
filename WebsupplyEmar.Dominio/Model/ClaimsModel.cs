using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsupplyEmar.Dominio.Model
{
    public class ClaimsModel
    {
        public string CGCMatriz { get; set; }
        public string CGC { get; set; }
        public string CCUSTO { get; set; }
        public string REQUISIT { get; set; }
        public string TABELA { get; set; }
        public string? CGCF { get; set; }
        public string? CDGPED { get; set; }
        public string? CODPROD { get; set; }
        public string? CODITEM { get; set; }
        public string? CL_CDG { get; set; }
        public string? DISPONIVEL_FORNEC { get; set; }
        public string? TIPO { get; set; }
        public DateTime? DT_CRIACAO { get; set; }
        public DateTime? DT_EXPIRACAO { get; set; }
    }
}
