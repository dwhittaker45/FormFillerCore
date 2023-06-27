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
        Task<List<FormModel>> AllForms();
        Task<FormModel> FormByName(string name);
        Task<FormModel> FormByID(int fid);
        Task<FullFormModel> FullFormInfo(int fid);
        Task<byte[]> GetFile(int fid);
        Task<string> FileTypeByID(int fid);
        Task FormAdd(FullFormModel formitem);
        Task FormUpdate(FormModel formitem);
        Task FormDelete(int fid);
        Task<int> GetFormIDbyDataType(int did);
        Task<string> DataTypeByID(int did);
    }
}
