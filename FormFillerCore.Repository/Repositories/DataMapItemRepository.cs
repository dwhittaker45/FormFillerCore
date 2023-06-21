using FormFillerCore.Repository.Interfaces;
using FormFillerCore.Repository.RepModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Repository.Repositories
{
    public class DataMapItemRepository : IDataMapItemRepository
    {
        public List<FormDataMap> DataMapItemsbyID(int did)
        {
            using (var db = new PdfformFillerContext())
            {
                return db.FormDataMaps.Where(x => x.FormDataTypeId == did).ToList();
            }
        }
        public List<FormDataMap> DataMapItemsByName(string form, string datatype)
        {
            using (var db = new PdfformFillerContext())
            {
                int fid = (from forms in db.Forms
                           where forms.FormName == form
                           select forms.Fid).First();

                int did = (from dtype in db.FormDataTypes
                           where dtype.DataType == datatype && dtype.FormId == fid
                           select dtype.FormDataTypeId).First();

                return db.FormDataMaps.Where(x => x.FormDataTypeId == did).ToList();

            }
        }
        public void AddDataMapItems(FormDataMap dmitem)
        {
            using (var db = new PdfformFillerContext())
            {
                db.FormDataMaps.Add(dmitem);
                db.SaveChanges();
            }
        }

        public void DeleteDataMapItem(int did)
        {
            using (var db = new PdfformFillerContext())
            {
                var entity = db.FormDataMaps.Where(x => x.DataMapId == did).First();
                db.FormDataMaps.Remove(entity);
                db.SaveChanges();
            }
        }

        public void UpdateDataMapItem(FormDataMap dmitem)
        {
            using (var db = new PdfformFillerContext())
            {
                var entity = db.FormDataMaps.Where(x => x.DataMapId == dmitem.DataMapId).First();
                db.Entry(entity).CurrentValues.SetValues(dmitem);
                db.SaveChanges();
            }
        }
        public FormDataMap DataMapItemByID(int id)
        {
            using (var db = new PdfformFillerContext())
            {
                return db.FormDataMaps.Where(x => x.DataMapId == id).First();
            }
        }


        public int FormIDfromDataID(int id)
        {
            using (var db = new PdfformFillerContext())
            {
                int did = db.FormDataMaps.Where(x => x.DataMapId == id).Select(y => y.FormDataTypeId).First();
                int fid = db.FormDataTypes.Where(x => x.FormDataTypeId == did).Select(y => y.FormId).First();

                return fid;
            }
        }


        public List<string> FormDataObjectsbyID(int did)
        {
            using (var db = new PdfformFillerContext())
            {
                List<string> results = new List<string>();

                results = db.FormDataMaps.Where(y => y.FormDataTypeId == did).Select(x => x.DataObject).Distinct().ToList();

                return results;
            }
        }

        public List<string> FormDataObjectsbyName(string form, string datatype, bool repeatable)
        {
            using (var db = new PdfformFillerContext())
            {
                int fid = (from forms in db.Forms
                           where forms.FormName == form
                           select forms.Fid).First();

                int did = (from dtype in db.FormDataTypes
                           where dtype.DataType == datatype && dtype.FormId == fid
                           select dtype.FormDataTypeId).First();

                List<string> results = new List<string>();

                if (repeatable == false)
                {
                    results = db.FormDataMaps.Where(x => x.FormDataTypeId == did && x.Repeatable == false).Select(x => x.DataObject).Distinct().ToList();
                }
                else
                {
                    results = db.FormDataMaps.Where(x => x.FormDataTypeId == did && x.Repeatable == true).Select(x => x.DataObject).Distinct().ToList();
                }
                return results;

            }
        }

        public List<DataMapChildObject> ChildObjectsByParent(int pid)
        {
            using (var db = new PdfformFillerContext())
            {
                return db.DataMapChildObjects.Where(x => x.ParentObject == pid).ToList();
            }
        }


        public void AddChildItem(DataMapChildObject citem)
        {
            using (var db = new PdfformFillerContext())
            {
                db.DataMapChildObjects.Add(citem);
                db.SaveChanges();
            }
        }


        public List<string> ChildDataObjectsByParentName(string datao, string fname, string dtype)
        {
            using (var db = new PdfformFillerContext())
            {
                int did = (from datat in db.FormDataTypes
                           join formt in db.Forms on datat.FormId equals formt.Fid
                           where formt.FormName == fname && datat.DataType == dtype
                           select datat.FormDataTypeId).First();

                int pid = (from objects in db.FormDataMaps
                           where objects.DataObject == datao && objects.FormDataTypeId == did
                           select objects.DataMapId).First();

                List<string> results = new List<string>();

                results = db.DataMapChildObjects.Where(x => x.ParentObject == pid).Select(y => y.DataObject).Distinct().ToList();

                return results;
            }
        }


        public int GetItemCountbyID(int did)
        {
            using (var db = new PdfformFillerContext())
            {
                return (int)db.FormDataMaps.Where(x => x.DataMapId == did).Select(y => y.ItemCount).First();
            }
        }

        public int GetItemCountbyName(string dname)
        {
            using (var db = new PdfformFillerContext())
            {
                return (int)db.FormDataMaps.Where(x => x.DataObject == dname).Select(y => y.ItemCount).First();
            }
        }


        public DataMapChildObject ChildObjectbynames(string cname, string pname, int dtype)
        {
            using (var db = new PdfformFillerContext())
            {
                int pid = (from parents in db.FormDataMaps
                           where parents.DataObject == pname && parents.FormDataTypeId == dtype
                           select parents.DataMapId).First();

                return db.DataMapChildObjects.Where(x => x.ParentObject == pid && x.DataObject == cname).First();
            }
        }


        public FormDataMap GetDataMapItem(string dname, string foname, int dtype)
        {
            using (var db = new PdfformFillerContext())
            {
                return db.FormDataMaps.Where(x => x.DataObject == dname && x.FormObject == foname && x.FormDataTypeId == dtype).First();
            }
        }


        public string GetDataItemFormatByName(string dname, int dtype)
        {
            using (var db = new PdfformFillerContext())
            {
                return db.FormDataMaps.Where(x => x.FormObject == dname && x.FormDataTypeId == dtype).Select(y => y.DataFormat).First();
            }
        }
    }
}
