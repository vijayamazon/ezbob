namespace SalesForceLib.Models {
	public class ApiResponse {
		public ApiResponse(bool success, string error) {
			Success = success;
			Error = error;
		}
		public ApiResponse(bool success) : this(success, null) { }

		public bool Success { get; private set; }
		public string Error { get; private set; }
	}
}
