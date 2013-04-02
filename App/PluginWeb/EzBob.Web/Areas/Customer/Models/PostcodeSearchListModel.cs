using System.Collections.Generic;
using Newtonsoft.Json;

namespace EzBob.Web.Areas.Customer.Models
{
    interface IPostcode
    {
        string Credits_display_text { get; set; }
        int Found { get; set; }
        string Errormessage { get; set; }
    }
    public class PostCodeReccord
    {
        public string L { get; set; }
        public string Id { get; set; }
    }
    //----------------------------------------------
    public class PostcodeSearchListModel : IPostcode
    {
        public int                   Found { get; set; }
// ReSharper disable InconsistentNaming
        [JsonIgnore]
        public string                Credits_display_text { get; set; }
// ReSharper restore InconsistentNaming
        [JsonIgnore]
        public string                Accountadminpage { get; set; }
        public string                Errormessage { get; set; }
        public int Maxresults { get; set; }
        public int                   Recordcount { get; set; }        
        public List<PostCodeReccord> Records { get; set; }
    }
}