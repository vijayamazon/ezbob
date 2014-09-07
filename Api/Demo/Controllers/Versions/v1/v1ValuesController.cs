namespace Demo.Controllers.Versions.v1 {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Web.Http;
	using Demo.Models;
	using Demo.Filters;
	using Infrastructure;

	/// <summary>
	/// Provides an example of an anonymous API.
	/// </summary>
	[ValidateAppKey]
	[HandleActionExecuted(1)]
	[RoutePrefix("api/v1/values")]
	public class ValuesController : DemoApiControllerBase {
		#region public

		#region constructor

		/// <summary>
		/// 
		/// </summary>
		public ValuesController() {
			m_oValues = ValueStorage.Instance;
		} // constructor

		#endregion constructor

		#region load all values action

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
			try {
				if (m_oValues.IsEmpty)
					throw Return.Status(ApiVersion, HttpStatusCode.NoContent, "There are no items in the list.");

				return m_oValues.Values;
			}
			catch (HttpResponseException) {
				throw;
			}
			catch (Exception e) {
				throw Return.Error(ApiVersion, "Failed to retrieve item list: {0}", e.Message);
			} // try
		} // Get

		#endregion load all values action

		#region load one value action

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
			try {
				if (!m_oValues.Contains(nID))
					throw Return.NotFound(ApiVersion, "No item found with the requested id ({0}).", nID);

				return m_oValues[nID];
			}
			catch (HttpResponseException) {
				throw;
			}
			catch (Exception e) {
				throw Return.Error(ApiVersion, "Failed to retrieve item list: {0}", e.Message);
			} // try
		} // Get

		#endregion load one value action

		#region create value action

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
			try {
				if (string.IsNullOrWhiteSpace(oValue.Title))
					throw Return.Status(ApiVersion, HttpStatusCode.NotAcceptable, "Value title cannot be an empty string.");

				m_oValues += oValue;

				return oValue;
			}
			catch (HttpResponseException) {
				throw;
			}
			catch (Exception e) {
				throw Return.Error(ApiVersion, "Failed to create a new item: {0}", e.Message);
			} // try
		} // Post

		#endregion create value action

		#region update value action

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
			try {
				oValue.ID = nID;

				if (!m_oValues.Update(oValue))
					throw Return.NotFound(ApiVersion, "No item found with the requested id ({0}).", nID);

				throw Return.Success(ApiVersion, "Item with the requested id ({0}) has been updated.", nID);
			}
			catch (HttpResponseException) {
				throw;
			}
			catch (Exception e) {
				throw Return.Error(ApiVersion, "Failed to update item with the requested id ({0}): {1}", nID, e.Message);
			} // try
		} // Put

		#endregion update value action

		#region delete value action

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
			try {
				if (!m_oValues.Remove(nID))
					throw Return.NotFound(ApiVersion, "No item found with the requested id ({0}).", nID);

				throw Return.Success(ApiVersion, "Item with the requested id ({0}) has been removed.", nID);
			}
			catch (HttpResponseException) {
				throw;
			}
			catch (Exception e) {
				throw Return.Error(ApiVersion, "Failed to remove item with the requested id ({0}): {1}", nID, e.Message);
			} // try
		} // Delete

		#endregion delete value action

		#endregion public

		private ValueStorage m_oValues;
	} // class ValuesController
} // namespace
