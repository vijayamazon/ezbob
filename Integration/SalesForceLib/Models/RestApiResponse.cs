namespace SalesForceLib.Models {
	using System.Collections.Generic;

	public class RestApiResponse {
		public bool success { get; set; }
		public string message { get; set; }
		public string errorCode { get; set; }
	}

	public class GetActivityRestApiResonse : RestApiResponse {
		public IEnumerable<ActivityResultModel> listObj { get; set; } 
	}
}
