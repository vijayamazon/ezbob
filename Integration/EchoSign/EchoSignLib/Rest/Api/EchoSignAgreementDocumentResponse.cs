using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoSignLib.Rest.Api
{
    class EchoSignAgreementDocumentResponse {
        public byte[] Content { get; set; }
        public string MimeType { get; set; }
    }
}
