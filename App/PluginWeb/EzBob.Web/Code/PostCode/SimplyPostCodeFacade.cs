namespace EzBob.Web.Code.PostCode {
	using System;
	using System.IO;
	using System.Net;
	using EzBob.Web.Infrastructure;
	using Ezbob.Logger;
	using Newtonsoft.Json;
	using log4net;

	public class SimplyPostCodeFacade : IPostCodeFacade {
		public SimplyPostCodeFacade(IEzBobConfiguration config) {
			m_oCfg = config;
		} // constructor

		public IPostCodeResponse GetAddressFromPostCode(string postCode, int nUserID) {
			return PostToPostcodeService(typeof(PostCodeResponseSearchListModel), postCode, nUserID);
		} // GetAddressFromPostCode

		public IPostCodeResponse GetFullAddressFromPostCode(string id, int nUserID) {
			return PostToPostcodeService(typeof(PostCodeResponseFullAddressModel), id, nUserID);
		} // GetFullAddressFromPostCode

		private IPostCodeResponse PostToPostcodeService(Type oPostCodeResponseType, string sSearchKey, int nUserID) {
			var oLog = new SafeILog(LogManager.GetLogger(typeof(SimplyPostCodeFacade)));

			string sRequestType;
			string sRequestAction;
			string sKeyName;

			oLog.Debug("Postcode service request for user {0}: {1} of type {2}...", nUserID, sSearchKey, oPostCodeResponseType);

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
				oLog.Warn("Unsupported response type requested: " + oPostCodeResponseType);
				return new PostCodeResponseSearchListModel {Success = false, Message = "Internal error"};
			} // if

			string sUrl = string.Format(
				"http://www.simplylookupadmin.co.uk/JSONservice/{0}.aspx?datakey={1}&{2}={3}",
				sRequestAction, m_oCfg.PostcodeConnectionKey, sKeyName, sSearchKey
			);

			oLog.Debug("Postcode service request for user {0}: {1} of type {2} - URL is {3}", nUserID, sSearchKey, oPostCodeResponseType, sUrl);

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
						oLog.Debug("Postcode service request for user {0}: {1} of type {2} - cannot read Postcode response stream.", nUserID, sSearchKey, oPostCodeResponseType);
					else {
						using (var reader = new StreamReader(stream)) {
							var responseData = reader.ReadToEnd();

							model = (IPostCodeResponse)JsonConvert.DeserializeObject(responseData, oPostCodeResponseType);

							if (!string.IsNullOrEmpty(model.Errormessage))
								oLog.Alert("Postcode service return error: {0}", model.Errormessage);

							oLog.Debug("Postcode service credit text: {0}", model.Credits_display_text);

							response.Close();

							sStatus = "Successful";
							sResponseData = responseData;

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
				oLog.Alert(e, "Postcode service request for user {0}: {1} of type {2} - something went terribly not so well.", nUserID, sSearchKey, oPostCodeResponseType);
				sStatus = "Failed";
				sErrorMessage = e.ToString();
			} // try

			oLog.Debug("Postcode service request for user {0}: {1} of type {2} done.", nUserID, sSearchKey, oPostCodeResponseType);

			try {
				new ServiceClient().Instance.PostcodeSaveLog(sRequestType, sUrl, sStatus, sResponseData, sErrorMessage, nUserID);
			}
			catch (Exception e) {
				oLog.Alert(e, "Postcode service request for user {0}: {1} of type {2} - failed to save log to DB.", nUserID, sSearchKey, oPostCodeResponseType);
			} // try

			oLog.Debug("Postcode service request for user {0}: {1} of type {2} complete.", nUserID, sSearchKey, oPostCodeResponseType);

			return model ?? new PostCodeResponseSearchListModel { Success = false, Message = "Not found" };
		} // PostToPostcodeService

		private readonly IEzBobConfiguration m_oCfg;
	} // class SimplyPostCodeFacade
} // namespace
