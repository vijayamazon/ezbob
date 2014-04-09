namespace EzBob.Web.Code.PostCode {
	using System.Web.Script.Serialization;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public interface IPostCodeResponse {
		string Credits_display_text { get; set; }
		int Found { get; set; }
		string Errormessage { get; set; }
		bool Success { get; set; }
		string Message { get; set; }
	} // interface IPostCodeResponse

	public class PostCodeReccord {
		public string L { get; set; }
		public string Id { get; set; }
	} // class PostCodeReccord

	public class PostCodeResponseSearchListModel : IPostCodeResponse {
		public int Found { get; set; }

		[JsonIgnore, ScriptIgnore]
		public string Credits_display_text { get; set; }

		[JsonIgnore, ScriptIgnore]
		public string Accountadminpage { get; set; }

		public string Errormessage { get; set; }

		public bool Success { get; set; }

		public string Message { get; set; }

		public int Maxresults { get; set; }

		public int Recordcount { get; set; }

		public List<PostCodeReccord> Records { get; set; }
	} // class PostCodeResponseSearchListModel
} // namespace
