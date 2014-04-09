namespace EzBob.Web.Code.PostCode {
	using System.Web.Script.Serialization;
	using EZBob.DatabaseLib.Model.Database;
	using Newtonsoft.Json;

	public class PostCodeResponseFullAddressModel : CustomerAddress, IPostCodeResponse {
		public virtual int Found { get; set; }

		[JsonIgnore, ScriptIgnore]
		public virtual string Accountadminpage { get; set; }

		public virtual string Errormessage { get; set; }

		public bool Success { get; set; }

		public string Message { get; set; }

		[JsonIgnore, ScriptIgnore]
		public virtual string Credits_display_text { get; set; }
	} // class PostCodeResponseFullAddressModel
} // namespace
