using ApplicationMng.Model;
using Newtonsoft.Json;

namespace EzBob.Web.Code.PostCode
{
    public class PostCodeResponseFullAddressModel: CustomerAddress, IPostCodeResponse
    {
        public virtual int Found { get; set; }
        [JsonIgnore]
        public virtual string Accountadminpage { get; set; }
        public virtual string Errormessage { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        [JsonIgnore]
        public virtual string Credits_display_text { get; set; }
    }
}