using AutoMapper;
using DataLayer.Data;
using NCSApi.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.MapperProfile
{
    public class AutoMapperProfile : Profile
    {
        public class AssessmentProfile : Profile
        {
            public AssessmentProfile()
            {
                CreateMap<Assessment, AssessmentResponse>()
                    .ForMember(destination => destination.AssessmentType, source => source.MapFrom(src => src.AssessmentType.Name));
            }
        }
    }
}
