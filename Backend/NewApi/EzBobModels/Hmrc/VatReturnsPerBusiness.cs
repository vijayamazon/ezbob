using System.Collections.Generic;

namespace EzBobModels.Hmrc
{
    public class VatReturnsPerBusiness
    {
        public VatReturnRecord VatReturnRecord { get; set; }
        public HmrcBusiness Business { get; set; }
        public IEnumerable<VatReturnEntry> Entries { get; set; } 
    }
}
