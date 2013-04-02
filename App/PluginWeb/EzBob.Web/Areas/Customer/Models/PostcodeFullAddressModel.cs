using ApplicationMng.Model;
using Newtonsoft.Json;

namespace EzBob.Web.Areas.Customer.Models
{
    public class PostcodeFullAddressModel: CustomerAddress, IPostcode
    {
        public virtual int Found { get; set; }
        [JsonIgnore]
        public virtual string Accountadminpage { get; set; }
        public virtual string Errormessage { get; set; }
// ReSharper disable InconsistentNaming
        [JsonIgnore]
        public virtual string Credits_display_text { get; set; }
// ReSharper restore InconsistentNaming
    }
}