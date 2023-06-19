using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Common.Models
{
    public class ChildMapItemModel
    {
        public int? ChildObjectID { get; set; }
        public int? ParentObject { get; set; }
        public string FormObject { get; set; }
        public string DataObject { get; set; }
        public string CheckValue { get; set; }
        public bool Calculated { get; set; }
        public string Expression { get; set; }
        public string DataFormat { get; set; }
    }
}
