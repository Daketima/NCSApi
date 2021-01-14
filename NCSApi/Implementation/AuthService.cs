using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCSApi.Implementation
{
    public class AuthService
    {
        public static async Task<UserInfo> GetLoggedinUserDetails(string username)
        {
            var client = new HttpClient() { BaseAddress = new Uri("http://172.16.200.172/TAJCentralAuth/") };
            var getResponse = await client.GetAsync($"api/account/getuser?username={username}");
            var userDetails = await getResponse.Content.ReadAsAsync<UserInfo>();

            return userDetails;
        }
        public static async Task<Root_> GetBranchSupervisors(string branchcode)
        {

            var client = new HttpClient() { BaseAddress = new Uri("http://172.16.200.172/TAJCentralAuth/") };
            var getResponse = await client.GetAsync($"api/account/getsupervisorsbybranchcode?branchCode={branchcode}&appName=customs");

            var supervisors = await getResponse.Content.ReadAsAsync<Root_>();
            return supervisors;
        }
    }
    public class User_
    {
        public string userId { get; set; }
        public string username { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string branchCode { get; set; }
    }

    public class _User
    {
        public string userId { get; set; }
        public User_ user { get; set; }
        public string applicationId { get; set; }
        public object application { get; set; }
        public bool isSupervisor { get; set; }
    }

    public class Root_
    {
        public List<_User> users { get; set; }
    }



    public class UserInfo
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string lastName { get; set; }
        public string Email { get; set; }
        public string BranchCode { get; set; }
        public List<UserApp> Applications { get; set; }
    }
    public class UserApp
    {
        public string UserId { get; set; }
        public string applicationId { get; set; }
        public App application { get; set; }
        public bool isSupervisor { get; set; }
    }
    public class App
    {
        public string name { get; set; }
    }


}
