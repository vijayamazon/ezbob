namespace Demo.Controllers {
	using System.Collections.Generic;
	using System.Web.Http;
	using Filters;

	[CopyStatusToHeader]
	public class SecureValuesController : ApiController {
		// GET api/securevalues
		public IEnumerable<string> Get() {
			return new string[] { "value1", "value2" };
		}

		// GET api/securevalues/5
		public string Get(int id) {
			return "value";
		}

		// POST api/securevalues
		public void Post([FromBody]string value) {
		}

		// PUT api/securevalues/5
		public void Put(int id, [FromBody]string value) {
		}

		// DELETE api/securevalues/5
		public void Delete(int id) {
		}
	}
}
