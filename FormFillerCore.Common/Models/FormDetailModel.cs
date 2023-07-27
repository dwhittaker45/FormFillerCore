using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Common.Models
{
    public class FormDetailModel
    {
        public FormDetailModel() 
        {
            NewMapItem = new DataMapItemModel();
            DataMap = new List<DataMapItemModel>();
            FormFields = new List<string>();
        }

        public DataMapItemModel NewMapItem { get; set; }   

        public List<DataMapItemModel> DataMap { get; set; } 

        public List<string> FormFields { get; set; }
    }
}
