using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Common.Models
{
    public class FullFormModel
    {
        public DataTypeModel DataType { get; set; }
        public FormModel FormModel { get; set; }
        public List<DataMapItemModel>? DataMap { get; set; }
    }
}
