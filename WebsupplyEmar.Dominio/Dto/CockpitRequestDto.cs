using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsupplyEmar.Dominio.Model;

namespace WebsupplyEmar.Dominio.Dto
{
    public class CockpitRequestDto : DataTableModel
    {
        public DateTime periodoInicio { get; set; }
        public DateTime periodoFim { get; set; }
    }
}
