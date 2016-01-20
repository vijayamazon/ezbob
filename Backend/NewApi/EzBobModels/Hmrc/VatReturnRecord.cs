using System;

namespace EzBobModels.Hmrc {
    public class VatReturnRecord {
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public DateTime Created { get; set; }
        public int? CustomerMarketPlaceUpdatingHistoryRecordId { get; set; }
        public string Period { get; set; } //mandatory
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime DateDue { get; set; }
        public int BusinessId { get; set; }
        public long RegistrationNo { get; set; }
        public bool? IsDeleted { get; set; }
        public int SourceId { get; set; }
        public Guid InternalID { get; set; }
    }
}
