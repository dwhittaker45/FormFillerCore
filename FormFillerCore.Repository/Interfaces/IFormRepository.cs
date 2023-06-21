using FormFillerCore.Repository.RepModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Repository.Interfaces
{
    public interface IFormRepository
    {
        List<Form> AllForms();
        Form FormByName(string name);
        Form FormByID(int fid);
        byte[] GetFile(int fid);
        string FileTypeByID(int fid);
        int FormAdd(Form formitem);
        void FormUpdate(Form formitem);
        void FormDelete(int fid);
        int FormIDByDataType(int did);
    }
}
