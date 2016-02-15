namespace Ezbob.Backend.Strategies.CompaniesHouse {
	using System.Collections.Generic;
	using Newtonsoft.Json;
	
	public class ErrorsResult {
		public IList<ErrorResult> errors { get; set; }
	}

	public class ErrorResult {
		[JsonConverter(typeof(ErrorConvertor))]
		public string error { get; set; }
		public IList<Dictionary<string, string>> error_values { get; set; }
		public string location { get; set; }
		public string type { get; set; }
	}
}