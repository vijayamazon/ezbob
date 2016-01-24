namespace SalesForceLib.Models {
	public class GetAccountByIDResponse {
		public GetAccountByIDResopnseAttributes attributes { get; set; }
		public string Id { get; set; }
	}//GetAccountByIDResponse

	public class GetAccountByIDResopnseAttributes {
		public string url { get; set; }
		public string type { get; set; }
	}//GetAccountByIDResopnseAttributes
}//ns
