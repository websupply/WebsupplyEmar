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
        public List<ColumnDto>? Columns { get; set; } = new List<ColumnDto>() { new ColumnDto() };
        public List<OrderDto>? Order { get; set; } = new List<OrderDto> { new OrderDto() };
        public SearchDto? Search { get; set; } = new SearchDto();
        public int? Start { get; set; }
        public int? Length { get; set; }
        public DateTime periodoInicio { get; set; }
        public DateTime periodoFim { get; set; }
    }
}
