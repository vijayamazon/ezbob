namespace Demo.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Http;
	using System.Web.Http;
	using Models;
	using Filters;

	/// <summary>
	/// Provides an example of an anonymous API.
	/// </summary>
	[CopyStatusToHeader]
	[ValidateAppKey]
	public class ValuesController : ApiController {
		#region public

		#region load all values action

		// GET api/values
		/// <summary>
		/// Gets list of all currently existing values.
		/// <para>appkey header must be provided. HTTP status code 403 is returned when this header is missing.
		/// HTTP status code 401 is returned when this header contains an unexpected value.
		/// </para>
		/// </summary>
		/// <returns>List of all currently existing values.</returns>
		public IEnumerable<ValueModel> Get() {
			try {
				if (ms_oValues.Count < 1) {
					throw new HttpResponseException(new HttpResponseMessage {
						StatusCode = HttpStatusCode.NoContent,
						ReasonPhrase = "There are no items in the list.",
					});
				} // if

				return ms_oValues.Values;
			}
			catch (HttpResponseException) {
				throw;
			}
			catch (Exception e) {
				throw new HttpResponseException(new HttpResponseMessage {
					StatusCode = HttpStatusCode.InternalServerError,
					ReasonPhrase = string.Format("Failed to retrieve item list: {0}", e.Message),
				});
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
		public ValueModel Get(int nID) {
			try {
				if (!ms_oValues.ContainsKey(nID)) {
					throw new HttpResponseException(new HttpResponseMessage {
						StatusCode = HttpStatusCode.NotFound,
						ReasonPhrase = string.Format("No item found with the requested id ({0}).", nID),
					});
				} // if

				return ms_oValues[nID];
			}
			catch (HttpResponseException) {
				throw;
			}
			catch (Exception e) {
				throw new HttpResponseException(new HttpResponseMessage {
					StatusCode = HttpStatusCode.InternalServerError,
					ReasonPhrase = string.Format("Failed to retrieve item with the requested id ({0}): {1}", nID, e.Message),
				});
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
		public ValueModel Post([FromBody]ValueModel oValue) {
			try {
				if (string.IsNullOrWhiteSpace(oValue.Title))
					throw new NullReferenceException("Value title cannot be an empty string.");

				oValue.ID = ms_nIDGen++;

				ms_oValues[oValue.ID] = oValue;

				return oValue;
			}
			catch (Exception e) {
				throw new HttpResponseException(new HttpResponseMessage {
					StatusCode = HttpStatusCode.InternalServerError,
					ReasonPhrase = string.Format("Failed to create a new item: {0}", e.Message),
				});
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
		public void Put(int nID, [FromBody]ValueModel oValue) {
			try {
				if (!ms_oValues.ContainsKey(nID)) {
					throw new HttpResponseException(new HttpResponseMessage {
						StatusCode = HttpStatusCode.NotFound,
						ReasonPhrase = string.Format("No item found with the requested id ({0}).", nID),
					});
				} // if

				var oOld = ms_oValues[oValue.ID];
				oOld.Title = oValue.Title;
				oOld.Content = oValue.Content;

				throw new HttpResponseException(new HttpResponseMessage {
					StatusCode = HttpStatusCode.OK,
					ReasonPhrase = string.Format("Item with the requested id ({0}) has been updated.", nID),
				});
			}
			catch (HttpResponseException) {
				throw;
			}
			catch (Exception e) {
				throw new HttpResponseException(new HttpResponseMessage {
					StatusCode = HttpStatusCode.InternalServerError,
					ReasonPhrase = string.Format("Failed to update item with the requested id ({0}): {1}", nID, e.Message),
				});
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
		public void Delete(int nID) {
			try {
				if (!ms_oValues.ContainsKey(nID)) {
					throw new HttpResponseException(new HttpResponseMessage {
						StatusCode = HttpStatusCode.NotFound,
						ReasonPhrase = string.Format("No item found with the requested id ({0}).", nID),
					});
				} // if

				throw new HttpResponseException(new HttpResponseMessage {
					StatusCode = HttpStatusCode.OK,
					ReasonPhrase = string.Format("Item with the requested id ({0}) has been removed.", nID),
				});
			}
			catch (HttpResponseException) {
				throw;
			}
			catch (Exception e) {
				throw new HttpResponseException(new HttpResponseMessage {
					StatusCode = HttpStatusCode.InternalServerError,
					ReasonPhrase = string.Format("Failed to remove item with the requested id ({0}): {1}", nID, e.Message),
				});
			} // try
		} // Delete

		#endregion delete value action

		#endregion public

		#region private

		private static readonly SortedDictionary<int, ValueModel> ms_oValues = new SortedDictionary<int, ValueModel>();
		private static int ms_nIDGen = 1;

		#endregion private
	} // class ValuesController
} // namespace
