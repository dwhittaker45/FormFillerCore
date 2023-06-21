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
        List<DataMapItemModel> GetFormDataMap(int did);
        List<DataMapItemModel> GetFormDataMapByName(string fname, string dtype);
        List<ChildMapItemModel> GetChildObjectsByParent(int pid);
        void AddChildObject(ChildMapItemModel citem);
        List<string> GetFormFields(int fid);
        void AddMapItem(DataMapItemModel dm);
        void UpdateMapItem(DataMapItemModel dm);
        void DeleteMapItem(int id);
        DataMapItemModel GetMapItem(int id);
        int GetFormIDFromMapItem(int id);
        void AutoMapItems(int did, int fid);
        Dictionary<string, object> FillMap(string fname, string dtype, Dictionary<string, object> values);
        int ItemCountByid(int did);
        int ItemCountByName(string dname);
        void FillCalculatedFields(Queue<KeyValuePair<string, object>> expressions, ref Dictionary<string, object> dmap, FormModel frm, int dtype, Dictionary<string, string> dformats);
        KeyValuePair<string, object> ReplaceFormField(string field, byte[] pdffrm, int dtype, Dictionary<string, object> dmap);
        string FormatValue(string val, string format);
    }
}
