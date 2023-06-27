using FormFillerCore.Repository.RepModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Repository.Interfaces
{
    public interface IDataTypeRepository
    {
        Task<List<FormDataType>> DataTypesByForm(int fid);
        Task<string> DataTypeByID(int did);
        Task AddDataType(FormDataType dtype);
        Task UpdateDataType(FormDataType dtype);
        Task DeleteDataTypebyID(int did);
        Task DeleteDataTypeByForm(int fid);
    }
}
