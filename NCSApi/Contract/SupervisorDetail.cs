using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{

    public class SupervisorDetail
    {
        public User[] users { get; set; }
    }

    public class User
    {
        public string userId { get; set; }
        public User1 user { get; set; }
        public string applicationId { get; set; }
        public object application { get; set; }
        public bool isSupervisor { get; set; }
    }

    public class User1
    {
        public string userId { get; set; }
        public string username { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string branchCode { get; set; }
        public DateTime created { get; set; }
        public DateTime dateModified { get; set; }
        public object credentials { get; set; }
        public object[] userApplications { get; set; }
    }

}
