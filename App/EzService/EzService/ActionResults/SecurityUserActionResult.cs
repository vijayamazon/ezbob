namespace EzService.ActionResults {
    using System.Runtime.Serialization;
    using Ezbob.Backend.ModelsWithDB.Authentication;

    [DataContract]
    public class SecurityUserActionResult : ActionResult {
		[DataMember]
        public User User { get; set; }
	}
}
