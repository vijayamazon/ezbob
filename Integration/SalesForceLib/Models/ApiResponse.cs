namespace SalesForceLib.Models {
	public class ApiResponse {
		public ApiResponse() { }
		public ApiResponse(string success, string error) {
			Success = success;
			Error = error;
		}

		public string Success { get; private set; }
		public bool IsSuccess { get { return string.IsNullOrEmpty(Error);} }
		public string Error { get; private set; }
	}
}
