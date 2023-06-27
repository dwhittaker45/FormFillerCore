using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Service.Interfaces
{
    public interface IFormAPIService
    {
        Task<Dictionary<string, object>> GetDataSchema(string fname, string dtype);
        Task<byte[]> FillForm(string fname, Dictionary<string, object> values);
        Task<byte[]> FillForm(string fname, Dictionary<string, object> values, string OptReplace);
        byte[] BuildForm(Dictionary<string, object> values, string title);
        Task<byte[]> BuildFormAsync(Dictionary<string, object> values, string title);
        string XmlConvert(byte[] xdoc);
        Task<string> XmlConvertAsync(byte[] xdoc);
    }
}
