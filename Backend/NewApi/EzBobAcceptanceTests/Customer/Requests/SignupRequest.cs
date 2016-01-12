using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobAcceptanceTests.Customer.Requests
{
    /// <summary>
    /// Represents body of customer sign-up REST request
    /// </summary>
    public class SignupRequest
    {
        public SignupRequest() {
            
        }

        public SignupRequest(string emailAddress) {
            this.Account = new Account {
                EmailAddress =  emailAddress
            };
        }

        public Account Account { get; set; }
    }
}
