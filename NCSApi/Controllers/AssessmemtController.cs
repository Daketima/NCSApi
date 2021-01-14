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
using Microsoft.Extensions.Configuration;
using NCSApi.Config;
using NCSApi.Contract;
using NCSApi.Core;
using NCSApi.Implementation;
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
        private readonly string appName;

        public AssessmemtController(CustomContext context, IMapper mapper,IConfiguration config)
        {
            _context = context; _mapper = mapper;
            appName = config.GetValue<string>("ApplicationName");
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

        // todo: called by the approval leg
        //[HttpGet]
        //[Route("find/{assessmentId}/{userId}")]
        
        [HttpPost]
        [Route("approval_find")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> thisAss(CheckDto dto )
        {
            try
            {
                //todo: implement check maker


                Log.Information($"Attemting to get assessment with an id {dto.AssessmentId}");
                Assessment getAss = await _context.Assessment
                                                     .Include(r => r.AssessmentType)
                                                             .FirstOrDefaultAsync(x => x.Id.Equals(dto.AssessmentId));
                var loggedinUser = await AuthService.GetLoggedinUserDetails(dto.LoggedInUser);
                var loggedInUsername = loggedinUser.Username;
                var loggedInbranchCode = loggedinUser.BranchCode;

                if (!string.IsNullOrEmpty(loggedInUsername)
                    && loggedinUser.Applications.Any(a => a.application.name.ToLower() == appName.ToLower()) && getAss.InitiatedByBranchCode == loggedInbranchCode)
                {
                    if (getAss != null)
                    {
                        Log.Information($"Assessment with id {dto.AssessmentId} found");

                        Log.Information($" Getting payment logged for Assessment with id {dto.AssessmentId}");
                        PaymentLog getPaymentLog = await _context.Payment.Where(x => x.AssessmentId == getAss.Id.ToString()).FirstOrDefaultAsync();

                        Log.Information($"No payment logged for Assessment with id {dto.AssessmentId}");
                        if (getPaymentLog == null) return NotFound(new { status = HttpStatusCode.NotFound, Message = $"No payment generated for assesment {dto.AssessmentId}" });

                        AssessmentResponse response = _mapper.Map<AssessmentResponse>(getAss);
                        response.Taxes = _mapper.Map<List<TaxResponse>>(
                            await _context.Tax.Where(x => x.AssessmentId.Equals(dto.AssessmentId)).ToListAsync()
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
                    Log.Information($"Request complete, no assessment found with an id {dto.AssessmentId}");
                    return NotFound(new { status = HttpStatusCode.NotFound, Message = "Request Successful", data = "Resource not found" });
                }
                else
                {
                    return BadRequest("Only supervisor role can perform this operation | this request was not initiated by your branch.");
                }
               
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", data = ex });
            }
        }


        // todo: initiator leg
        // POST api/<AssessmemtController>
        [HttpPost]
        [Route("find")]
        public async Task<IActionResult> Post([FromBody] AssessmentRequest model)
        {

            var loggedinUser = await AuthService.GetLoggedinUserDetails(model.LoggedInUser);
            var loggedInUsername = loggedinUser.Username;
            var loggedInbranchCode = loggedinUser.BranchCode;

            if (!string.IsNullOrEmpty(loggedInUsername)
                && loggedinUser.Applications.Any(a => a.application.name.ToLower() == appName.ToLower() && !a.isSupervisor))
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
            else
            {
                return BadRequest("you are not allowed to perform this action");
            }
          
        }
    }
}
