using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FormFillerCore.Common.Enumerators;
using System.ComponentModel.DataAnnotations;

namespace FormFillerCore.Common.Models
{
    public class DataTypeModel
    {
        public int? FormDataTypeID { get; set; }

        public int? FormID { get; set; }

        [Required]
        public DataFormat DataType { get; set; }
    }
}
