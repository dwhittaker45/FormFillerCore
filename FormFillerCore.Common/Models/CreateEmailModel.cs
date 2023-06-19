using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Common.Models
{
    public class CreateEmailModel
    {
        public Dictionary<string, object> JObject { get; set; }
        public Dictionary<string, object> FormInfo { get; set; }
        public Dictionary<string, object> EmailInfo { get; set; }
    }
}
