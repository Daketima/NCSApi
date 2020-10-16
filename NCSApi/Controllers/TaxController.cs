using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataLayer.Data;
using DataLayer.DataContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NCSApi.Config;
using NCSApi.Core;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NCSApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxController : ControllerBase
    {
        readonly ICustomDutyClient _client;
        readonly DutyConfig _dutyConfig;
        readonly CustomContext _context;
        Random _random = new Random();

        public TaxController(ICustomDutyClient client, DutyConfig dutyConfig, CustomContext context)
        {
            _client = client;
            _dutyConfig = dutyConfig;
            _context = context;
        }
        // GET: api/<TaxController>
        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/<TaxController>/5
        [HttpGet]
        [Route("Find/byAssessment/{AssessmentId}")]
        public async Task<IActionResult> FindTaxes(Guid AssessmentId)
        {
            try {List<Tax> FindTaxes = await _context.Tax.Where(x => x.AssessmentId.Equals(AssessmentId)).ToListAsync();
            if (FindTaxes.Any())
            {

                return Ok(new { status = HttpStatusCode.OK, Message = "Request Successful", data = FindTaxes });
            }
            return NotFound(new { status = HttpStatusCode.NotFound, Message = "Request completed", data = new { message = "No tax(es) found for the payment reference" } }); }
            catch (Exception ex)
            {
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", data = ex });
            }
        }

        [HttpGet]
        [Route("Find/byPayment/{PaymentReference}")]
        public async Task<IActionResult> FindByPaymentre(string PaymentReference)
        {
            try
            {
                PaymentLog getPaymentLog = _context.Payment.Where(x => x.PaymentReference.Equals(PaymentReference)).FirstOrDefault();

                if (getPaymentLog == null) return BadRequest(new { status = HttpStatusCode.BadRequest, Message = "Request Unsuccessful", data = new { message = "Wrong Payment reference" } });


                Guid.TryParse(getPaymentLog.AssessmentId, out Guid assId);

                List<Tax> FindTaxes = await _context.Tax.Where(x => x.AssessmentId.Equals(assId)).ToListAsync();

                if (FindTaxes.Any())
                {
                    return Ok(new { status = HttpStatusCode.OK, Message = "Request Successful", data = FindTaxes });
                }
                return NotFound(new { status = HttpStatusCode.NotFound, Message = "Request completed", data = new { message = "No tax(es) found for the payment reference" } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", data = ex });
            }
        }
    }

    //// POST api/<TaxController>
    //[HttpPost]
    //public void Post([FromBody] string value)
    //{
    //}

    //// PUT api/<TaxController>/5
    //[HttpPut("{id}")]
    //public void Put(int id, [FromBody] string value)
    //{
    //}

    //// DELETE api/<TaxController>/5
    //[HttpDelete("{id}")]
    //public void Delete(int id)
    //{
    //}
}

