using AutoMapper;
using DataLayer.Data;
using Microsoft.AspNetCore.Routing.Constraints;
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
                    .ForMember(destination => destination.AssessmentType, source => source.MapFrom(src => src.AssessmentType.Name))
                    .ForMember(destinationMember: dest => dest.AttachmentUrl, source => source.MapFrom(src => src.AttachmentPath ))
                     .ForMember(destinationMember: dest => dest.FormMNumber, source => source.MapFrom(src => src.FormMNumber));
            }
        }

        public class TaxProfile : Profile
        {
            public TaxProfile()
            {
                CreateMap<Tax, TaxResponse>()
                    .ForSourceMember(x => x.Assessment, y => y.DoNotValidate());
                    
            }
        }

        public class PaymentProfile : Profile
        {
            public PaymentProfile()
            {
                CreateMap<PaymentLog, PaymentResponse>()
                    .ForMember(destinationMember: dest => dest.StatusId, src => src.MapFrom(surs => surs.StatusId));

            }
        }

        public class ReportProfile : Profile
        {
            public ReportProfile()
            {
                CreateMap<PaymentStatus, ReportResponse>()
                    .ForMember(destinationMember: dest => dest.PaymentDetail, src => src.MapFrom(surs => surs.PaymentLog));
            }
        }
    }
}
