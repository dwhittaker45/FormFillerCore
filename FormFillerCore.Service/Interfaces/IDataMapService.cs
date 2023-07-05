using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FormFillerCore.Common.Models;

namespace FormFillerCore.Service.Interfaces
{
    public interface IDataMapService
    {
        Task<List<DataMapItemModel>> GetFormDataMap(int did);
        Task<List<DataMapItemModel>> GetFormDataMapByName(string fname, string dtype);
        Task<List<ChildMapItemModel>> GetChildObjectsByParent(int pid);
        Task AddChildObject(ChildMapItemModel citem);
        List<string> GetFormFields(int fid);
        Task<List<string>> GetFormFieldsAsync(int fid);
        Task AddMapItem(DataMapItemModel dm);
        Task UpdateMapItem(DataMapItemModel dm);
        Task DeleteMapItem(int id);
        Task<DataMapItemModel> GetMapItem(int id);
        Task<int> GetFormIDFromMapItem(int id);
        Task AutoMapItems(int did, int fid);
        Task<Dictionary<string, object>> FillMap(string fname, string dtype, Dictionary<string, object> values);
        int ItemCountByid(int did);
        int ItemCountByName(string dname);
        void FillCalculatedFields(Queue<KeyValuePair<string, object>> expressions, ref Dictionary<string, object> dmap, FormModel frm, int dtype, Dictionary<string, string> dformats);
        Task<Dictionary<string, object>> FillCalculatedFieldsAsync(Queue<KeyValuePair<string, object>> expressions, Dictionary<string, object> dmap, FormModel frm, int dtype, Dictionary<string, string> dformats);
        KeyValuePair<string, object> ReplaceFormField(string field, byte[] pdffrm, int dtype, Dictionary<string, object> dmap);
        Task<KeyValuePair<string, object>> ReplaceFormFieldAsync(string field, byte[] pdfform, int dtype, Dictionary<string, object> dmap);
        string FormatValue(string val, string format);
    }
}
