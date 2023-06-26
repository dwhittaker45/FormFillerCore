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
        Task<List<Form>> AllForms();
        Task<Form> FormByName(string name);
        Task<Form> FormByID(int fid);
        Task<byte[]> GetFile(int fid);
        Task<string> FileTypeByID(int fid);
        Task<int> FormAdd(Form formitem);
        Task FormUpdate(Form formitem);
        Task FormDelete(int fid);
        Task<int> FormIDByDataType(int did);
    }
}
