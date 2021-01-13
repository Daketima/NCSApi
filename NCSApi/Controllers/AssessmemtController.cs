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
using Serilog;

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
        [Route("All/{BranchCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(string BranchCode)
        {
           // string BranchCode = Request?.Headers["BranchCode"];
            try
            {
                List<Assessment> getAsses = await _context.Assessment.Where(x => x.BankBranchCode == BranchCode).ToListAsync();

                if (getAsses != null)
                {
                    var response = _mapper.Map<List<AssessmentResponse>>(getAsses);

                    //Log.Information($"Processstatus id: {getPaymentLog.StatusId} and transactio reference {getPaymentLog.PaymentReference}");
                    return Ok(new { status = HttpStatusCode.OK, Message = "Request Successful", data = response });
                }
                return NotFound(new { status = HttpStatusCode.NotFound, Message = "Request Successful", data = "Resource not found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", data = ex });
            }
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
                Log.Information($"Attemting to get assessment with an id {assessmentId}");
                Assessment getAss = await _context.Assessment
                                                     .Include(r => r.AssessmentType)
                                                             .FirstOrDefaultAsync(x => x.Id.Equals(assessmentId));
                if (getAss != null)
                {
                    Log.Information($"Assessment with id {assessmentId} found");
                    
                    Log.Information($" Getting payment logged for Assessment with id {assessmentId}");
                    PaymentLog getPaymentLog = await _context.Payment.Where(x => x.AssessmentId == getAss.Id.ToString()).FirstOrDefaultAsync();

                    Log.Information($"No payment logged for Assessment with id {assessmentId}");
                    if (getPaymentLog == null) return NotFound(new { status = HttpStatusCode.NotFound, Message = $"No payment generated for assesment {assessmentId}" });

                    AssessmentResponse response = _mapper.Map<AssessmentResponse>(getAss);
                    response.Taxes = _mapper.Map<List<TaxResponse>>(
                        await _context.Tax.Where(x => x.AssessmentId.Equals(assessmentId)).ToListAsync()
                        );

                    Log.Information($"Processstatus id: {getPaymentLog.StatusId} and transactio reference {getPaymentLog.PaymentReference}");
                    return Ok(new
                    {
                        status = HttpStatusCode.OK,
                        Message = "Request Successful",
                        data = new
                        {
                            Details = response,
                            PaymentReference = getPaymentLog.PaymentReference,
                            ProcessStatus = Enum.GetName(typeof(TransactionStatus), getPaymentLog.StatusId),
                            TransactionStatus = Enum.GetName(typeof(TransactionStatus), getPaymentLog.TransactionStatusId),
                            Comment = getPaymentLog.Comment

                        }
                    });
                }
                Log.Information($"Request complete, no assessment found with an id {assessmentId}");
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
            var assessment = await _context.Assessment.Where(x => x.Year.Equals(model.SADAssessmentYear) && x.AssessmentSerial.Equals(model.SADAssessmentSerial) && x.AssessmentNumber.Equals(model.SADAssessmentNumber)).ToListAsync();

            var currentVerson = assessment.LastOrDefault();

            if (currentVerson != null)
            {
                AssessmentResponse response = _mapper.Map<AssessmentResponse>(currentVerson);
                response.Taxes = _mapper.Map<List<TaxResponse>>(
                    await _context.Tax.Where(x => x.AssessmentId.Equals(currentVerson.Id)).ToListAsync()
                    );

                return Ok(new { status = HttpStatusCode.OK, Message = "Request completed", Data = response });
            }
            return NotFound(new { status = HttpStatusCode.NotFound, Message = "Resource not found" });
        }
    }
}
