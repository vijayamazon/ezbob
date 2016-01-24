using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Modules.Marketplaces.Hmrc
{
    using Nancy;

    public class PdfUploadModel
    {
        public string CustomerId { get; set; }
        public IEnumerable<HttpFile> Files { get; set; } 
    }
}
