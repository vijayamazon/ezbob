namespace EzBob.Web.Controllers {
	using System;
	using System.IO;
	using System.Net;
	using System.Web.Mvc;
	using Code.PostCode;
	using ConfigManager;
	using Infrastructure;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using log4net;

	[Authorize]
	public class PostcodeController : Controller {

		public PostcodeController(IEzbobWorkplaceContext context) {
			this.m_oContext = context;
		} // constructor

		[OutputCache(VaryByParam = "postCode", Duration = 3600 * 24 * 7)]
		public JsonResult GetAddressListFromPostCode(string postCode) {
			return Json(
				PostToPostcodeService(typeof(PostCodeResponseSearchListModel), postCode, this.m_oContext.User.Id),
				JsonRequestBehavior.AllowGet
			);
		} // GetAddressListFromPostCode

		[OutputCache(VaryByParam = "id", Duration = 3600 * 24 * 7)]
		public JsonResult GetFullAddressFromId(string id) {
			return Json(
				PostToPostcodeService(typeof(PostCodeResponseFullAddressModel), id, this.m_oContext.User.Id),
				JsonRequestBehavior.AllowGet
			);
		} // GetFullAddressFromId

		private IPostCodeResponse PostToPostcodeService(Type oPostCodeResponseType, string sSearchKey, int nUserID) {
			string sRequestType;
			string sRequestAction;
			string sKeyName;

			ms_oLog.DebugFormat("Postcode service request for user {0}: {1} of type {2}...", nUserID, sSearchKey, oPostCodeResponseType);

			if (string.IsNullOrEmpty(sSearchKey)) {
				ms_oLog.ErrorFormat("Empty postcode was provided {0} {1} {2}", nUserID, sSearchKey, oPostCodeResponseType);
				return new PostCodeResponseSearchListModel { Success = false, Message = "Please, try again" };
			}


			if (oPostCodeResponseType == typeof (PostCodeResponseSearchListModel)) {
				sRequestType = "Get list of address by postcode";
				sRequestAction = "JSONSearchForAddress";
				sKeyName = "postcode";
			}
			else if (oPostCodeResponseType == typeof (PostCodeResponseFullAddressModel)) {
				sRequestType = "Get address details by postcode id";
				sRequestAction = "JSONGetAddressRecord";
				sKeyName = "id";
			}
			else {
				ms_oLog.Warn("Unsupported response type requested: " + oPostCodeResponseType);
				return new PostCodeResponseSearchListModel {Success = false, Message = "Internal error"};
			} // if

			string sUrl = string.Format(
				"http://www.simplylookupadmin.co.uk/JSONservice/{0}.aspx?datakey={1}&{2}={3}",
				sRequestAction, CurrentValues.Instance.PostcodeConnectionKey.Value, sKeyName, sSearchKey
			);

			ms_oLog.DebugFormat("Postcode service request for user {0}: {1} of type {2} - URL is {3}", nUserID, sSearchKey, oPostCodeResponseType, sUrl);

			string sStatus = string.Empty;
			string sResponseData = string.Empty;
			string sErrorMessage = string.Empty;

			IPostCodeResponse model = null;

			try {
				var request = (HttpWebRequest)WebRequest.Create(sUrl);

				request.Method = "GET";
				request.Accept = "application/json";

				using (var response = (HttpWebResponse)request.GetResponse()) {
					var stream = response.GetResponseStream();

					if (stream == null)
						ms_oLog.DebugFormat("Postcode service request for user {0}: {1} of type {2} - cannot read Postcode response stream.", nUserID, sSearchKey, oPostCodeResponseType);
					else {
						using (var reader = new StreamReader(stream)) {
							sResponseData = reader.ReadToEnd();

							model = (IPostCodeResponse)JsonConvert.DeserializeObject(sResponseData, oPostCodeResponseType);

							if (!string.IsNullOrEmpty(model.Errormessage))
								ms_oLog.ErrorFormat("Postcode service return error: {0}", model.Errormessage);

							ms_oLog.DebugFormat("Postcode service credit text: {0}", model.Credits_display_text);

							response.Close();

							sStatus = "Successful";

							if (model.Found == 0) {
								model.Success = false;
								model.Message = "Not found";
							}
							else {
								model.Success = true;
								model.Message = "";
							} // if
						} // using reader
					} // if stream is not null
				} // using response
			}
			catch (Exception e) {
				ms_oLog.Error(string.Format("Postcode service request for user {0}: {1} of type {2} - something went terribly not so well.", nUserID, sSearchKey, oPostCodeResponseType), e);
				sStatus = "Failed";
				sErrorMessage = e.ToString();
			} // try

			ms_oLog.DebugFormat("Postcode service request for user {0}: {1} of type {2} done.", nUserID, sSearchKey, oPostCodeResponseType);

			try {
				new ServiceClient().Instance.PostcodeSaveLog(sRequestType, sUrl, sStatus, sResponseData, sErrorMessage, nUserID);
			}
			catch (Exception e) {
				ms_oLog.Error(string.Format("Postcode service request for user {0}: {1} of type {2} - failed to save log to DB.", nUserID, sSearchKey, oPostCodeResponseType), e);
			} // try

			ms_oLog.DebugFormat("Postcode service request for user {0}: {1} of type {2} complete.", nUserID, sSearchKey, oPostCodeResponseType);

			return model ?? new PostCodeResponseSearchListModel { Success = false, Message = "Not found" };
		} // PostToPostcodeService

		private readonly IEzbobWorkplaceContext m_oContext;

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof (PostcodeController));

	} // class PostcodeController
} // namespace
