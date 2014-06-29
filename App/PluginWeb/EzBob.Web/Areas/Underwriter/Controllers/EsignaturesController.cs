namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EchoSignLib;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class EsignaturesController : Controller {
		#region constructor

		public EsignaturesController() {
			m_oServiceClient = new ServiceClient();
		} // constructor

		#endregion constructor

		#region action Load

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult Load(int? nCustomerID, bool bPollStatus) {
			ms_oLog.Debug("Loading e-signatures for customer {0} {1} polling status...", nCustomerID, bPollStatus ? "with" : "without");

			Esignature[] oSignatures;
			Esigner[] oPotentialSigners;

			try {
				EsignatureListActionResult elar = m_oServiceClient.Instance.LoadEsignatures(nCustomerID, bPollStatus);
				oSignatures = elar.Data;
				oPotentialSigners = elar.PotentialSigners;
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to load e-signatures for customer {0} {1} polling status.", nCustomerID, bPollStatus ? "with" : "without");
				oSignatures = new Esignature[0];
				oPotentialSigners = new Esigner[0];
			} // try

			ms_oLog.Debug("Loading e-signatures for customer {0} {1} polling status complete.", nCustomerID, bPollStatus ? "with" : "without");

			return Json(new { signatures = oSignatures, signers = oPotentialSigners, }, JsonRequestBehavior.AllowGet);
		} // Load

		#endregion action Load

		#region action Send

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpPost]
		public JsonResult Send(string sPackage) {
			EchoSignEnvelope[] oPackage = JsonConvert.DeserializeObject<EchoSignEnvelope[]>(sPackage);

			if (oPackage == null) {
				ms_oLog.Debug("Could not extract e-sign package from {0}.", sPackage);
				return Json(new { success = false, error = "Could not extract e-sign package from input.", });
			} // if

			if (oPackage.Length == 0) {
				ms_oLog.Debug("Empty e-sign package received: {0}.", sPackage);
				return Json(new { success = false, error = "Empty e-sign package received.", });
			} // if

			EchoSignEnvelope[] oPackageToSend = oPackage.Where(x => x.IsValid).ToArray();

			if (oPackage.Length == 0) {
				ms_oLog.Debug("No envelopes are ready to be sent in: {0}.", string.Join("\n", (object[])oPackage));
				return Json(new { success = false, error = "No envelopes are ready to be sent.", });
			} // if

			ms_oLog.Debug("Send for signature request:\n{0}", string.Join("\n", (object[])oPackageToSend));

			string sResult;

			try {
				StringActionResult sar = m_oServiceClient.Instance.EsignSend(oPackageToSend);
				sResult = sar.Value;
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to send a package for e-signing.");
				return Json(new { success = false, error = "Failed to send a package for e-signing.", });
			} // try

			return Json(new { success = string.IsNullOrWhiteSpace(sResult), error = sResult, });
		} // Send

		#endregion action Send

		#region action Download

		[HttpGet]
		public FileResult Download(long nEsignatureID) {
			ms_oLog.Debug("Loading e-signature file for id {0}...", nEsignatureID);

			try {
				EsignatureFileActionResult efar = m_oServiceClient.Instance.LoadEsignatureFile(nEsignatureID);

				ms_oLog.Debug("Loading e-signature file for id {0} complete.", nEsignatureID);

				return new FileContentResult(efar.Contents, efar.MimeType) {
					FileDownloadName = efar.FileName,
				};
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Loading e-signature file for id {0} failed.", nEsignatureID);
				throw new Exception("Failed to download requested file.");
			} // try
		} // Download

		#endregion action Download

		private readonly ServiceClient m_oServiceClient;

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof (EsignaturesController));
	} // class EsignaturesController
} // namespace
