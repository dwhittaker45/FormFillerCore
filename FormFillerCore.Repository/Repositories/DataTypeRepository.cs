using FormFillerCore.Repository.Interfaces;
using FormFillerCore.Repository.RepModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Repository.Repositories
{
    public class DataTypeRepository : IDataTypeRepository
    {

        private readonly PdfformFillerContext _context;

        public DataTypeRepository(PdfformFillerContext context)
        {
            _context = context;
        }

        public async Task<List<FormDataType>> DataTypesByForm(int fid)
        {
            return await _context.FormDataTypes.Where(x => x.FormId == fid).ToListAsync();
        }

        public void AddDataType(FormDataType dtype)
        {
            using (var db = new PdfformFillerContext())
            {
                db.FormDataTypes.Add(dtype);
                db.SaveChanges();
            }
        }

        public void UpdateDataType(FormDataType dtype)
        {
            using (var db = new PdfformFillerContext())
            {
                var entity = db.FormDataTypes.Where(x => x.FormDataTypeId == dtype.FormDataTypeId).First();
                db.Entry(entity).CurrentValues.SetValues(dtype);
                db.SaveChanges();
            }
        }
        public void DeleteDataTypebyID(int did)
        {
            using (var db = new PdfformFillerContext())
            {
                FormDataType dtype = db.FormDataTypes.Where(x => x.FormDataTypeId == did).First();
                db.FormDataTypes.Remove(dtype);
                db.SaveChanges();
            }
        }

        public void DeleteDataTypeByForm(int fid)
        {
            using (var db = new PdfformFillerContext())
            {
                List<FormDataType> dtypes = db.FormDataTypes.Where(x => x.FormId == fid).ToList();

                foreach (FormDataType dtype in dtypes)
                {
                    db.FormDataTypes.Remove(dtype);
                }
                db.SaveChanges();
            }
        }


        public string DataTypeByID(int did)
        {
            using (var db = new PdfformFillerContext())
            {
                string dtype = db.FormDataTypes.Where(d => d.FormDataTypeId == did).First().DataType;

                return dtype;
            }
        }
    }
}
