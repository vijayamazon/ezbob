namespace Ezbob.Integration.LogicalGlue.Models {
	using System;
	using System.Collections.Generic;

	public class Request {
		public long ID { get; set; }
		public int CustomerID { get; set; }
		public long ServiceLogID { get; set; }
		public RequestType RequestType { get; set; }
		public DateTime SendingTime { get; set; }

		public string Content { get; set; }

		public List<RequestItem> Items { get; set; }

		public Response Response { get; set; }
	} // class Request
} // namespace
