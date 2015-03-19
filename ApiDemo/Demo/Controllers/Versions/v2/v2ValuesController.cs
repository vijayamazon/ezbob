namespace Demo.Controllers.Versions.v1 {
	using System.Collections.Generic;
	using System.Web.Http;
	using Demo.Models;
	using Demo.Filters;

	/// <summary>
	/// Provides an example of an anonymous API.
	/// </summary>
	[ValidateAppKey]
	[HandleActionExecuted(2)]
	[RoutePrefix("api/v2/values")]
	public class V2ValuesController : ValuesController {

		// GET api/values
		/// <summary>
		/// Gets list of all currently existing values.
		/// <para>appkey header must be provided. HTTP status code 403 is returned when this header is missing.
		/// HTTP status code 401 is returned when this header contains an unexpected value.
		/// </para>
		/// </summary>
		/// <returns>List of all currently existing values.</returns>
		[Route("")]
		public IEnumerable<ValueModel> Get() {
			return base.Get();
		} // Get

		// GET api/values/5
		/// <summary>
		/// Gets specified value by its id.
		/// <para>appkey header must be provided. HTTP status code 403 is returned when this header is missing.
		/// HTTP status code 401 is returned when this header contains an unexpected value.
		/// </para>
		/// </summary>
		/// <param name="nID">Value id to look for.</param>
		/// <returns>Corresponding value when requested id exists, or empty output and HTTP status code of 404 otherwise.</returns>
		[Route("{nID:int}")]
		public ValueModel Get(int nID) {
			return base.Get(nID);
		} // Get

		// POST api/values
		/// <summary>
		/// Creates a new value.
		/// <para>appkey header must be provided. HTTP status code 403 is returned when this header is missing.
		/// HTTP status code 401 is returned when this header contains an unexpected value.
		/// </para>
		/// </summary>
		/// <param name="oValue">New value details.</param>
		/// <returns>A new value that has been created.</returns>
		[Route("")]
		public ValueModel Post([FromBody]ValueModel oValue) {
			return base.Post(oValue);
		} // Post

		// PUT api/values/5
		/// <summary>
		/// Updates existing value by its id. HTTP status code 404 is returned when no value found by requested id.
		/// <para>appkey header must be provided. HTTP status code 403 is returned when this header is missing.
		/// HTTP status code 401 is returned when this header contains an unexpected value.
		/// </para>
		/// </summary>
		/// <param name="nID">Id of value to update.</param>
		/// <param name="oValue">New value details.</param>
		[Route("{nID:int}")]
		public void Put(int nID, [FromBody]ValueModel oValue) {
			base.Put(nID, oValue);
		} // Put

		// DELETE api/values/5
		/// <summary>
		/// Deletes existing value by its id. HTTP status code 404 is returned when no value found by requested id.
		/// <para>appkey header must be provided. HTTP status code 403 is returned when this header is missing.
		/// HTTP status code 401 is returned when this header contains an unexpected value.
		/// </para>
		/// </summary>
		/// <param name="nID">Id of value to update.</param>
		[Route("{nID:int}")]
		public void Delete(int nID) {
			base.Delete(nID);
		} // Delete

	} // class ValuesController
} // namespace
