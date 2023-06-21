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
        List<FormDataMap> DataMapItemsByName(string form, string datatype);
        FormDataMap GetDataMapItem(string dname, string foname, int dtype);
        List<DataMapChildObject> ChildObjectsByParent(int pid);
        DataMapChildObject ChildObjectbynames(string cname, string pname, int dtype);
        List<string> FormDataObjectsbyID(int did);
        List<string> FormDataObjectsbyName(string form, string datatype, bool repeatable);
        List<string> ChildDataObjectsByParentName(string datao, string form, string datatype);
        void AddChildItem(DataMapChildObject citem);
        FormDataMap DataMapItemByID(int id);
        int FormIDfromDataID(int id);
        void AddDataMapItems(FormDataMap dmitem);
        void DeleteDataMapItem(int did);
        void UpdateDataMapItem(FormDataMap dmitem);
        int GetItemCountbyID(int did);
        int GetItemCountbyName(string dname);
        string GetDataItemFormatByName(string dname, int dtype);
    }
}
