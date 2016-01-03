using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels
{
    public class LoginInfo {
        public string Email { get; set; }
        public Password Password { get; set; }
        public int PasswordQuestionId { get; set; }
        public string PasswordAnswer { get; set; }
        public string RemoteIp { get; set; }
    }
}
