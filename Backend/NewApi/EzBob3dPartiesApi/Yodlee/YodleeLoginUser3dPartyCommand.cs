using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.Yodlee
{
    using EzBobCommon.NSB;

    public class YodleeLoginUser3dPartyCommand : CommandBase
    {
        public string CobrandToken { get; set; }
        public string UserName { get; set; }
        public string PasswordEncrypted { get; set; }

        //these fields are needed to book new yodlee user account, in case login failed 
        //these fields are returned back by LoginYodleeUser3dPartyCommandResponse
        public int CustomerId { get; set; }
        public int SiteId { get; set; }
    }
}
