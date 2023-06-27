using AutoMapper;
using FormFillerCore.Common.Models;
using FormFillerCore.Repository.Interfaces;
using FormFillerCore.Repository.RepModels;
using FormFillerCore.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public async Task<List<FormModel>> AllForms()
        {
            var aFrms = await _formRepository.AllForms();

            return _mapper.Map<List<FormModel>>(aFrms);
        }

        public async Task<FormModel> FormByName(string name)
        {
            var fMod = await _formRepository.FormByName(name);

            return _mapper.Map<FormModel>(fMod);
        }

        public async Task<FormModel> FormByID(int fid)
        {
            var frmModel = await _formRepository.FormByID(fid);

            return _mapper.Map<FormModel>(frmModel);
        }

        public async Task FormAdd(FullFormModel formitem)
        {
            int form;

            using (var br = new BinaryReader(formitem.FormModel.TempFile.OpenReadStream()))
            {
                formitem.FormModel.Form = br.ReadBytes((int)formitem.FormModel.TempFile.Length);
            }

            form = await _formRepository.FormAdd(_mapper.Map<Form>(formitem.FormModel));

            formitem.DataType.FormID = form;

            await _dataTypeRepository.AddDataType(_mapper.Map<FormDataType>(formitem.DataType));
        }

        public async Task FormUpdate(FormModel formitem)
        {

            using (var br = new BinaryReader(formitem.TempFile.OpenReadStream()))
            {
                formitem.Form = br.ReadBytes((int)formitem.TempFile.Length);
            }

            await _formRepository.FormUpdate(_mapper.Map<Form>(formitem));
        }

        public async Task FormDelete(int fid)
        {
            await _dataTypeRepository.DeleteDataTypeByForm(fid);
            await _formRepository.FormDelete(fid);
        }
        public async Task<string> DataTypeByID(int did)
        {
            return await _dataTypeRepository.DataTypeByID(did);
        }

        public async Task<byte[]> GetFile(int fid)
        {
            return await _formRepository.GetFile(fid);
        }
        public async Task<string> FileTypeByID(int fid)
        {
            return await _formRepository.FileTypeByID(fid);
        }
        public async Task<FullFormModel> FullFormInfo(int fid)
        {
            FullFormModel fullform = new FullFormModel();
            FormModel form = new FormModel();
            DataTypeModel dtype = new DataTypeModel();
            List<DataMapItemModel> mitems = new List<DataMapItemModel>();


            var frm = await _formRepository.FormByID(fid);
            var dataT = await _dataTypeRepository.DataTypesByForm(fid);


            form = _mapper.Map<FormModel>(frm);
            dtype = _mapper.Map<DataTypeModel>(dataT.First());

            int did = Convert.ToInt32(dtype.FormDataTypeID);

            var Items = await _dataMapRepository.DataMapItemsbyIDAsync(did);

            mitems = _mapper.Map<List<DataMapItemModel>>(Items);

            fullform.FormModel = form;
            fullform.DataType = dtype;
            fullform.DataMap = mitems;

            return fullform;
        }
        public async Task<int> GetFormIDbyDataType(int did)
        {
            return await _formRepository.FormIDByDataType(did);
        }
    }
}
