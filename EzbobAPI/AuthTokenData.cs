using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzbobAPI {
	public class AuthTokenData {
		public string AuthToken { get; set; }
		public DateTime Expires { get; set; }
	}
}