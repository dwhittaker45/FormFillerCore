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
    public class FormRepository : IFormRepository
    {

        private readonly PdfformFillerContext _context;

        public FormRepository(PdfformFillerContext context)
        {
            _context = context;
        }

        public async Task<List<Form>> AllForms()
        {
            return await _context.Forms.Where(x => x.Active == true).ToListAsync();
        }

        public async Task<Form> FormByName(string name)
        {
            var c = from Forms in _context.Forms
                    where Forms.FormName == name
                    select Forms;

            return await c.SingleOrDefaultAsync();
        }

        public async Task<Form> FormByID(int fid)
        {
            var c = from Forms in _context.Forms
                    where Forms.Fid == fid
                    select Forms;


            return await c.SingleOrDefaultAsync();
        }

        public async Task<int> FormAdd(Form formitem)
        {
            await _context.Forms.AddAsync(formitem);
            _context.SaveChanges();
            return formitem.Fid;
        }

        public async Task FormUpdate(Form formitem)
        {
            var entity = await _context.Forms.Where(f => f.Fid == formitem.Fid).FirstAsync();
            _context.Entry(entity).CurrentValues.SetValues(formitem);
            await _context.SaveChangesAsync();
        }

        public async Task FormDelete(int fid)
        {
                var entity = await _context.Forms.Where(f => f.Fid == fid).FirstAsync();

                entity.Active = false;
                //db.Entry(entity).CurrentValues.SetValues(;
                await _context.SaveChangesAsync();
        }


        public async Task<byte[]> GetFile(int fid)
        {
            byte[] frm = await _context.Forms.Where(f => f.Fid == fid).Select(fr => fr.Form1).FirstAsync();

            return frm;
        }


        public async Task<string> FileTypeByID(int fid)
        {
            string ftype = await _context.Forms.Where(f => f.Fid == fid).Select(ft => ft.FileType).FirstAsync();

            return ftype;
        }


        public async Task<int> FormIDByDataType(int did)
        {
            int frm = await _context.FormDataTypes.Where(f => f.FormDataTypeId == did).Select(fr => fr.FormId).FirstAsync();

            return frm;
        }
    }
}
