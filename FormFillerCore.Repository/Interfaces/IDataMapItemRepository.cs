using FormFillerCore.Repository.RepModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Repository.Interfaces
{
    public interface IDataMapItemRepository
    {
        List<FormDataMap> DataMapItemsbyID(int did);
        Task<List<FormDataMap>> DataMapItemsbyIDAsync(int did);
        List<FormDataMap> DataMapItemsByName(string form, string datatype);
        Task<List<FormDataMap>> DataMapItemsByNameAsync(string form, string dataType);
        Task<FormDataMap> GetDataMapItem(string dname, string foname, int dtype);
        Task<List<DataMapChildObject>> ChildObjectsByParent(int pid);
        Task<DataMapChildObject> ChildObjectbynames(string cname, string pname, int dtype);
        Task<List<string>> FormDataObjectsbyID(int did);
        Task<List<string>> FormDataObjectsbyName(string form, string datatype, bool repeatable);
        Task<List<string>> ChildDataObjectsByParentName(string datao, string form, string datatype);
        Task AddChildItem(DataMapChildObject citem);
        Task<FormDataMap> DataMapItemByID(int id);
        int FormIDfromDataID(int id);
        Task<int> FormIDfromDataIDAsync(int id);
        Task AddDataMapItems(FormDataMap dmitem);
        Task DeleteDataMapItem(int did);
        Task UpdateDataMapItem(FormDataMap dmitem);
        Task<int> GetItemCountbyID(int did);
        Task<int> GetItemCountbyName(string dname);
        Task<string> GetDataItemFormatByName(string dname, int dtype);
    }
}
