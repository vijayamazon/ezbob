using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.Yodlee
{
    using EzBobCommon.NSB;

    public class YodleeLoginUser3dPartyCommandResponse : CommandResponseBase
    {
        public string UserToken { get; set; }

        //these fields are needed to book new yodlee user account, in case login failed 
        public int CustomerId { get; set; }
        public int SiteId { get; set; }

        public bool IsLoginFailed { get; set; }

    }
}
