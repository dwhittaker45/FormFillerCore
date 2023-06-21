using AutoMapper;
using FormFillerCore.Common.Models;
using FormFillerCore.Repository.RepModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormFillerCore.Service.Configs
{
    public class NormalProfile : Profile
    {
        public NormalProfile()
        {
            CreateMap<Form, FormModel>();
            CreateMap<Form, FormModel>().ForMember(dest => dest.Form, opt => opt.MapFrom(src => src.Form1));
            CreateMap<FormModel, Form>();
            CreateMap<FormModel, Form>().ForMember(dest => dest.Form1, opt => opt.MapFrom(src => src.Form));
            CreateMap<FormDataType, DataTypeModel>();
            CreateMap<DataTypeModel, FormDataType>();
            CreateMap<DataMapItemModel, FormDataMap>();
            CreateMap<FormDataMap, DataMapItemModel>();
            CreateMap<ChildMapItemModel, DataMapChildObject>();
            CreateMap<DataMapChildObject, ChildMapItemModel>();
        }
    }
}
