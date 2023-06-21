using FormFillerCore.Repository.Interfaces;
using FormFillerCore.Repository.RepModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Repository.Repositories
{
    public class FormRepository : IFormRepository
    {
        public List<Form> AllForms()
        {
            using (var db = new PdfformFillerContext())
            {
                return db.Forms.Where(x => x.Active == true).ToList();
            }
        }

        public Form FormByName(string name)
        {
            using (var db = new PdfformFillerContext())
            {
                var c = from Forms in db.Forms
                        where Forms.FormName == name
                        select Forms;

                return c.SingleOrDefault();
            }
        }

        public Form FormByID(int fid)
        {
            using (var db = new PdfformFillerContext())
            {
                var c = from Forms in db.Forms
                        where Forms.Fid == fid
                        select Forms;


                return c.SingleOrDefault();
            }
        }

        public int FormAdd(Form formitem)
        {
            using (var db = new PdfformFillerContext())
            {
                db.Forms.Add(formitem);
                db.SaveChanges();
                return formitem.Fid;
            }

        }

        public void FormUpdate(Form formitem)
        {
            using (var db = new PdfformFillerContext())
            {
                var entity = db.Forms.Where(f => f.Fid == formitem.Fid).First();
                db.Entry(entity).CurrentValues.SetValues(formitem);
                db.SaveChanges();
            }
        }

        public void FormDelete(int fid)
        {
            using (var db = new PdfformFillerContext())
            {
                var entity = db.Forms.Where(f => f.Fid == fid).First();

                entity.Active = false;
                //db.Entry(entity).CurrentValues.SetValues(;
                db.SaveChanges();
            }
        }


        public byte[] GetFile(int fid)
        {
            using (var db = new PdfformFillerContext())
            {
                byte[] frm = db.Forms.Where(f => f.Fid == fid).First().Form1;

                return frm;
            }
        }


        public string FileTypeByID(int fid)
        {
            using (var db = new PdfformFillerContext())
            {
                string ftype = db.Forms.Where(f => f.Fid == fid).First().FileType;

                return ftype;
            }
        }


        public int FormIDByDataType(int did)
        {
            using (var db = new PdfformFillerContext())
            {
                int frm = db.FormDataTypes.Where(f => f.FormDataTypeId == did).First().FormId;

                return frm;
            }
        }
    }
}
