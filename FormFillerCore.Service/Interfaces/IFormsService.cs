using FormFillerCore.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Service.Interfaces
{
    public interface IFormsService
    {
        List<FormModel> AllForms();
        Task<FormModel> FormByName(string name);
        FormModel FormByID(int fid);
        FullFormModel FullFormInfo(int fid);
        byte[] GetFile(int fid);
        string FileTypeByID(int fid);
        void FormAdd(FullFormModel formitem);
        void FormUpdate(FormModel formitem);
        void FormDelete(int fid);
        int GetFormIDbyDataType(int did);
    }
}
