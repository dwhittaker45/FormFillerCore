using FormFillerCore.Repository.Interfaces;
using FormFillerCore.Repository.RepModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Repository.Repositories
{
    public class DataMapItemRepository : IDataMapItemRepository
    {
        private readonly PdfformFillerContext _context;

        public DataMapItemRepository(PdfformFillerContext context)
        {
            _context = context;
        }

        public List<FormDataMap> DataMapItemsbyID(int did)
        {
             return  _context.FormDataMaps.Where(x => x.FormDataTypeId == did).ToList();
        }
        
        public async Task<List<FormDataMap>> DataMapItemsbyIDAsync(int did)
        {
            Task<List<FormDataMap>> dmap = new Task<List<FormDataMap>>(() => DataMapItemsbyID(did));

            dmap.Start();

            return await dmap;
        }
        public List<FormDataMap> DataMapItemsByName(string form, string datatype)
        {
            int fid = (from forms in _context.Forms
                        where forms.FormName == form
                        select forms.Fid).First();

            int did = (from dtype in _context.FormDataTypes
                        where dtype.DataType == datatype && dtype.FormId == fid
                        select dtype.FormDataTypeId).First();

            return _context.FormDataMaps.Where(x => x.FormDataTypeId == did).ToList();
        }
        public async Task<List<FormDataMap>> DataMapItemsByNameAsync(string form, string datatype)
        {
            Task<List<FormDataMap>> dmap = new Task<List<FormDataMap>>(() => DataMapItemsByName(form, datatype));

            dmap.Start();

            return await dmap;
        }
        public async Task AddDataMapItems(FormDataMap dmitem)
        {
            await _context.FormDataMaps.AddAsync(dmitem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDataMapItem(int did)
        {
            FormDataMap entity = await _context.FormDataMaps.Where(x => x.DataMapId == did).FirstAsync();
            _context.FormDataMaps.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDataMapItem(FormDataMap dmitem)
        {
            FormDataMap entity = await _context.FormDataMaps.Where(x => x.DataMapId == dmitem.DataMapId).FirstAsync();
            _context.Entry(entity).CurrentValues.SetValues(dmitem);
            await _context.SaveChangesAsync();
            
        }
        public async Task<FormDataMap> DataMapItemByID(int id)
        {
            return await _context.FormDataMaps.Where(x => x.DataMapId == id).FirstAsync();
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
        
        public async Task<int> FormIDfromDataIDAsync(int id)
        {
            Task<int> fid = new Task<int>(() => FormIDfromDataID(id));

            fid.Start();

            return await fid;
        }

        public async Task<List<string>> FormDataObjectsbyID(int did)
        {
            List<string> results = new List<string>();

            results = await _context.FormDataMaps.Where(y => y.FormDataTypeId == did).Select(x => x.DataObject).Distinct().ToListAsync();

            return results;
        }

        public async Task<List<string>> FormDataObjectsbyName(string form, string datatype, bool repeatable)
        {

            int fid = await (from forms in _context.Forms
                        where forms.FormName == form
                        select forms.Fid).FirstAsync();

            int did = await (from dtype in _context.FormDataTypes
                        where dtype.DataType == datatype && dtype.FormId == fid
                        select dtype.FormDataTypeId).FirstAsync();

            List<string> results = new List<string>();

            if (repeatable == false)
            {
                results = await _context.FormDataMaps.Where(x => x.FormDataTypeId == did && x.Repeatable == false).Select(x => x.DataObject).Distinct().ToListAsync();
            }
            else
            {
                results = await _context.FormDataMaps.Where(x => x.FormDataTypeId == did && x.Repeatable == true).Select(x => x.DataObject).Distinct().ToListAsync();
            }
            return results;
        }

        public async Task<List<DataMapChildObject>> ChildObjectsByParent(int pid)
        {
            return await _context.DataMapChildObjects.Where(x => x.ParentObject == pid).ToListAsync();
        }

        public async Task AddChildItem(DataMapChildObject citem)
        {
            await _context.DataMapChildObjects.AddAsync(citem);
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> ChildDataObjectsByParentName(string datao, string fname, string dtype)
        {
            int did = (from datat in _context.FormDataTypes
                        join formt in _context.Forms on datat.FormId equals formt.Fid
                        where formt.FormName == fname && datat.DataType == dtype
                        select datat.FormDataTypeId).First();

            int pid = (from objects in _context.FormDataMaps
                        where objects.DataObject == datao && objects.FormDataTypeId == did
                        select objects.DataMapId).First();

            List<string> results = new List<string>();

            results = await _context.DataMapChildObjects.Where(x => x.ParentObject == pid).Select(y => y.DataObject).Distinct().ToListAsync();

            return results;
        }


        public async Task<int> GetItemCountbyID(int did)
        {
            var iCount = await _context.FormDataMaps.Where(x => x.DataMapId == did).Select(y => y.ItemCount).FirstAsync();

            var ret = iCount == null ? default(int) : (int)iCount;

            return ret;
        }

        public async Task<int> GetItemCountbyName(string dname)
        {
            var iCount = await _context.FormDataMaps.Where(x => x.DataObject == dname).Select(y => y.ItemCount).FirstAsync();

            var ret = iCount == null ? default(int) : (int)iCount;

            return ret;
        }


        public async Task<DataMapChildObject> ChildObjectbynames(string cname, string pname, int dtype)
        {
            int pid = (from parents in _context.FormDataMaps
                        where parents.DataObject == pname && parents.FormDataTypeId == dtype
                        select parents.DataMapId).First();

            return await _context.DataMapChildObjects.Where(x => x.ParentObject == pid && x.DataObject == cname).FirstAsync();
        }


        public async Task<FormDataMap> GetDataMapItem(string dname, string foname, int dtype)
        {
            return await _context.FormDataMaps.Where(x => x.DataObject == dname && x.FormObject == foname && x.FormDataTypeId == dtype).FirstAsync();
        }


        public async Task<string> GetDataItemFormatByName(string dname, int dtype)
        {
            return await _context.FormDataMaps.Where(x => x.FormObject == dname && x.FormDataTypeId == dtype).Select(y => y.DataFormat).FirstAsync();
        }
    }
}
