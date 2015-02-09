namespace SalesForceLib.Models {
	using Newtonsoft.Json;

	public class ApiResponse {
		public ApiResponse() { }
		public ApiResponse(string success, string error) {
			Success = success;
			Error = error;
		}

		public string Success { get; set; }
		
		public string Error { get; set; }

		[JsonIgnore]
		public bool IsSuccess { get { return string.IsNullOrEmpty(Error); } }
	}
}
