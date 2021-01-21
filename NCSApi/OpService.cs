using NCSApi.Config;
using NCSApi.Contract;
using NCSApi.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NCSApi
{
    public class OpService
    {
        readonly DutyConfig _dutyConfig;
        readonly ICustomDutyClient _client;

        public OpService(DutyConfig DutyConfig, ICustomDutyClient client) { _dutyConfig = DutyConfig; _client = client; }

        public void SendMailToSupervisor(string SupervisorMail, string AuthorizerName, string AssessmentId, out string MailSentMessage)
        {
            MailSentMessage = null;

            string mailTemplate = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\EmailTemplates\\AuthorizerEmail.txt");
            mailTemplate = mailTemplate.Replace("{Authoriser}", AuthorizerName).Replace("{AssessmentId}", AssessmentId);

            SendMailRequest model = new SendMailRequest { ToAddress = SupervisorMail, Subject = "New Custom Duty Payment", MessageBody = mailTemplate };

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_dutyConfig.EmailEndpoint}"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
            };

            var response = _client.SendKaoshiRequest(request).Result;

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                MailSentMessage = responseString;

            }
        }
        public void SendMailToSupervisor_(List<NameMailPair> nameMailPairs, string AssessmentId)
        {
            try
            {
                //MailSentMessage = null;
                foreach (var item in nameMailPairs)
                {
                    string mailTemplate = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\EmailTemplates\\AuthorizerEmail.txt");
                    mailTemplate = mailTemplate.Replace("{Authoriser}", item.FullName).Replace("{AssessmentId}", AssessmentId);

                    SendMailRequest model = new SendMailRequest { ToAddress = item.Email, Subject = "New Custom Duty Payment", MessageBody = mailTemplate };

                    HttpRequestMessage request = new HttpRequestMessage
                    {
                        RequestUri = new Uri($"{_dutyConfig.EmailEndpoint}"),
                        Method = HttpMethod.Post,
                        Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
                    };

                    var response = _client.SendKaoshiRequest(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = response.Content.ReadAsStringAsync().Result;
                       // MailSentMessage = responseString;

                    }
                }

            }
            catch (Exception e)
            {

                //throw;
            }
          
         
        }

        public void GetStaffDetail(string Branchcode,  string ApplicationName, out string SupervisorMail, out string SupervisorName)
        {
            SupervisorMail = null;
            SupervisorName = null;

             HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_dutyConfig.StaffDetailEndpoint}?branchCode={Branchcode}&appName={ApplicationName}"),
                Method = HttpMethod.Get,
               // Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
            };

            var response = _client.SendKaoshiRequest(request).Result;

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                var allStaffDetail = JsonConvert.DeserializeObject<SupervisorDetail>(responseString);

                var supervisorDetail = allStaffDetail.users.FirstOrDefault();
                SupervisorMail = supervisorDetail.user.email;
                SupervisorName = $"{supervisorDetail.user.firstName} {supervisorDetail.user.lastName}";
            }

        }
        
        public void GetStaffDetail_(string Branchcode,  string ApplicationName, out List<NameMailPair> nameMailPairs)
        {
            //SupervisorMail = null;
            //SupervisorName = null;
            nameMailPairs = null;

             HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_dutyConfig.StaffDetailEndpoint}?branchCode={Branchcode}&appName={ApplicationName}"),
                Method = HttpMethod.Get,
               // Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
            };

            var response = _client.SendKaoshiRequest(request).Result;

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                var allStaffDetail = JsonConvert.DeserializeObject<SupervisorDetail>(responseString);
                
                var objs = new List<NameMailPair>();
                var emails = new List<string>();
                var fullNames = new List<string>();
                var supervisorDetail = allStaffDetail.users;
                supervisorDetail.ToList().ForEach(a => objs.Add(new NameMailPair {FullName = $"{a.user.firstName} {a.user.lastName}",Email = a.user.email }));
                nameMailPairs = objs;

                //SupervisorName = $"{supervisorDetail.user.firstName} {supervisorDetail.user.lastName}";
                //supervisorDetail.ToList().ForEach(a => emails.Add(a.user.email));

            }

        }


    }
    public class NameMailPair
    {
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}

