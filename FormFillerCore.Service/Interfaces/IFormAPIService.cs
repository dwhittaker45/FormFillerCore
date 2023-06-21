using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Service.Interfaces
{
    public interface IFormAPIService
    {
        Dictionary<string, object> GetDataSchema(string fname, string dtype);
        byte[] FillForm(string fname, Dictionary<string, object> values);
        byte[] FillForm(string fname, Dictionary<string, object> values, string OptReplace);
        byte[] BuildForm(Dictionary<string, object> values, string title);
        string XmlConvert(byte[] xdoc);
    }
}
