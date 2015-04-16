namespace EzService
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Ezbob.Backend.Models;

    [DataContract]
    public class ExperianTargetingActionResult : ActionResult
	{
		[DataMember]
        public List<CompanyInfo> CompanyInfos { get; set; }
	} // class WizardConfigsActionResult

} // namespace EzService
