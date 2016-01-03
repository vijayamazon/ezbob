namespace EzBob3dParties.HMRC {
    using System;
    using System.Collections.Generic;
    using EzBobCommon.Currencies;

    public class VatReturnInfo {

        private readonly IDictionary<string, Money> returnDetails = new Dictionary<string, Money>(); 

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Period { get; set; }
        public long RegistrationNumber { get; set; }
        public string BusinessName { get; set; }
        public string[] BusinessAddress { get; set; }

        public IDictionary<string, Money> ReturnDetails
        {
            get { return this.returnDetails; }
        }

        public byte[] PdfFile { get; set; }
    }
}
