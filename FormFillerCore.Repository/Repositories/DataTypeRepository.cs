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

        public async Task AddDataType(FormDataType dtype)
        {
            await _context.FormDataTypes.AddAsync(dtype);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDataType(FormDataType dtype)
        {
            var entity = await _context.FormDataTypes.Where(x => x.FormDataTypeId == dtype.FormDataTypeId).FirstAsync();
            _context.Entry(entity).CurrentValues.SetValues(dtype);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteDataTypebyID(int did)
        {
            FormDataType dtype = await _context.FormDataTypes.Where(x => x.FormDataTypeId == did).FirstAsync();
            _context.FormDataTypes.Remove(dtype);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDataTypeByForm(int fid)
        {
            List<FormDataType> dtypes = await _context.FormDataTypes.Where(x => x.FormId == fid).ToListAsync();

            foreach (FormDataType dtype in dtypes)
            {
                _context.FormDataTypes.Remove(dtype);
            }
            await _context.SaveChangesAsync();
        }


        public async Task<string> DataTypeByID(int did)
        {
            string dtype = await _context.FormDataTypes.Where(d => d.FormDataTypeId == did).Select(dt => dt.DataType).FirstAsync();

            return dtype;
        }
    }
}
