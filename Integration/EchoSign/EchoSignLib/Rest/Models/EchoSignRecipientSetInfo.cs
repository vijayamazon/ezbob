using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoSignLib.Rest.Models
{
    using EchoSignLib.EchoSignService;

    class EchoSignRecipientSetInfo
    {
        public EchoSignRecipientInfo[] recipientSetMemberInfos { get; set; }
        public string recipientSetRole { get; set; }
    }
}
