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
        List<FormDataType> DataTypesByForm(int fid);
        string DataTypeByID(int did);
        void AddDataType(FormDataType dtype);
        void UpdateDataType(FormDataType dtype);
        void DeleteDataTypebyID(int did);
        void DeleteDataTypeByForm(int fid);
    }
}
