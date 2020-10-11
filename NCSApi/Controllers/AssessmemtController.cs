using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DataLayer.Data;
using DataLayer.DataContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NCSApi.Config;
using NCSApi.Contract;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NCSApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmemtController : ControllerBase
    {
        readonly CustomContext _context;
        readonly IMapper _mapper;
        public AssessmemtController(CustomContext context, IMapper mapper)
        {
            _context = context; _mapper = mapper;
        }

        [HttpGet]
        [Route("find/{assessmentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> thisAss(Guid assessmentId)
        {
            try
            {
                Assessment getAss = await _context.Assessment
                                                      .Include(r => r.AssessmentType)
                                                             .FirstOrDefaultAsync(x => x.Id.Equals(assessmentId));
                if (getAss != null)
                {
                    PaymentLog getPaymentLog = await _context.Payment.Where(x => x.AssessmentId == assessmentId.ToString()).FirstOrDefaultAsync();

                    if (getPaymentLog == null) return NotFound(new { status = HttpStatusCode.NotFound, Message = $"No payment generated for assesment {assessmentId}" });

                    AssessmentResponse response = _mapper.Map<AssessmentResponse>(getAss);

                    return Ok(new
                    {
                        status = HttpStatusCode.OK,
                        Message = "Request Successful",
                        data = new
                        {
                            Details = response,
                            PaymentReference = getPaymentLog.PaymentReference,
                            ProcessStatus = Enum.GetName(typeof(TransactionStatus), getPaymentLog.StatusId),
                            TransactionStatus = Enum.GetName(typeof(TransactionStatus), getPaymentLog.TransactionStatusId)
                        }
                    });
                }
                return NotFound(new { status = HttpStatusCode.NotFound, Message = "Request Successful", data = "Resource not found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", data = ex });
            }
        }


        // POST api/<AssessmemtController>
        [HttpPost]
        [Route("find")]
        public async Task<IActionResult> Post([FromBody] AssessmentRequest model)
        {
            var assessment = await _context.Assessment.Where(x => x.Year.Equals(model.SADAssessmentYear) && x.AssessmentSerial.Equals(model.SADAssessmentSerial) && x.AssessmentNumber.Equals(model.SADAssessmentNumber)).FirstOrDefaultAsync();

            if (assessment != null)
            {
                AssessmentResponse response = _mapper.Map<AssessmentResponse>(assessment);
                response.Taxes = await _context.Tax.Where(x => x.AssessmentId.Equals(assessment.Id)).ToListAsync();

                return Ok(new { status = HttpStatusCode.OK, Message = "Request completed", Data = response });
            }

            return NotFound(new { status = HttpStatusCode.NotFound, Message = "Resource not found" });
        }


    }
}
