using AutoMapper;
using FormFillerCore.Common.Models;
using FormFillerCore.Repository.Interfaces;
using FormFillerCore.Repository.RepModels;
using FormFillerCore.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Service.Services
{
    public class FormsService : IFormsService
    {

        private readonly IFormRepository _formRepository;
        private readonly IDataTypeRepository _dataTypeRepository;
        private readonly IDataMapItemRepository _dataMapRepository;
        private readonly IMapper _mapper;
        public FormsService(IFormRepository formRepository, IDataTypeRepository dataTypeRepository, IDataMapItemRepository dataMapRepository, IMapper mapper)
        {
            _formRepository = formRepository;
            _dataTypeRepository = dataTypeRepository;
            _dataMapRepository = dataMapRepository;
            _mapper = mapper;
        }
        public List<FormModel> AllForms()
        {
            return _mapper.Map<List<FormModel>>(_formRepository.AllForms());
        }

        public async Task<FormModel> FormByName(string name)
        {
            var fMod = await _formRepository.FormByName(name);

            return _mapper.Map<FormModel>(fMod);
        }

        public FormModel FormByID(int fid)
        {
            return _mapper.Map<FormModel>(_formRepository.FormByID(fid));
        }

        public void FormAdd(FullFormModel formitem)
        {
            int form;

            using (var br = new BinaryReader(formitem.FormModel.TempFile.OpenReadStream()))
            {
                formitem.FormModel.Form = br.ReadBytes((int)formitem.FormModel.TempFile.Length);
            }

            form = _formRepository.FormAdd(_mapper.Map<Form>(formitem.FormModel));

            formitem.DataType.FormID = form;

            _dataTypeRepository.AddDataType(_mapper.Map<FormDataType>(formitem.DataType));
        }

        public void FormUpdate(FormModel formitem)
        {

            using (var br = new BinaryReader(formitem.TempFile.OpenReadStream()))
            {
                formitem.Form = br.ReadBytes((int)formitem.TempFile.Length);
            }

            _formRepository.FormUpdate(_mapper.Map<Form>(formitem));
        }

        public void FormDelete(int fid)
        {
            _dataTypeRepository.DeleteDataTypeByForm(fid);
            _formRepository.FormDelete(fid);
        }
        public string DataTypeByID(int did)
        {
            return _dataTypeRepository.DataTypeByID(did);
        }

        public byte[] GetFile(int fid)
        {
            return _formRepository.GetFile(fid);
        }
        public string FileTypeByID(int fid)
        {
            return _formRepository.FileTypeByID(fid);
        }
        public FullFormModel FullFormInfo(int fid)
        {
            FullFormModel fullform = new FullFormModel();
            FormModel form = new FormModel();
            DataTypeModel dtype = new DataTypeModel();
            List<DataMapItemModel> mitems = new List<DataMapItemModel>();

            form = _mapper.Map<FormModel>(_formRepository.FormByID(fid));
            dtype = _mapper.Map<DataTypeModel>(_dataTypeRepository.DataTypesByForm(fid).First());

            int did = Convert.ToInt32(dtype.FormDataTypeID);
            mitems = _mapper.Map<List<DataMapItemModel>>(_dataMapRepository.DataMapItemsbyID(did));

            fullform.FormModel = form;
            fullform.DataType = dtype;
            fullform.DataMap = mitems;

            return fullform;
        }
        public int GetFormIDbyDataType(int did)
        {
            return _formRepository.FormIDByDataType(did);
        }
    }
}
