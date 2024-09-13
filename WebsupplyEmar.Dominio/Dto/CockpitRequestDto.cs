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
        public List<ColumnDto>? Columns { get; set; }
        public List<OrderDto>? Order { get; set; }
        public SearchDto? Search { get; set; }
        public int? Start { get; set; }
        public int? Length { get; set; }
        public DateTime periodoInicio { get; set; }
        public DateTime periodoFim { get; set; }
    }
}
