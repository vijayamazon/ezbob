namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EchoSignLib;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class EsignaturesController : Controller {
		#region constructor

		public EsignaturesController(IEzbobWorkplaceContext oContext) {
			m_oContext = oContext;
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

		#region action SaveExperianDirector

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SaveExperianDirector(
			int directorID,
			string email,
			string mobilePhone,
			string line1,
			string line2,
			string line3,
			string town,
			string county,
			string postcode
		) {
			ms_oLog.Debug("User id: {0} {1}", m_oContext.UserId, m_oContext.User.Name);

			ms_oLog.Debug("Saving Experian director (E-signatures controller): {0}: {1} {2}, {3} {4} {5} {6} {7} {8}",
				directorID,
				email,
				mobilePhone,
				line1,
				line2,
				line3,
				town,
				county,
				postcode
			);

			var m = new Esigner {
				DirectorID = directorID,
				Email = (email ?? string.Empty).Trim(),
				MobilePhone = (mobilePhone ?? string.Empty).Trim(),
				Line1 = (line1 ?? string.Empty).Trim(),
				Line2 = (line2 ?? string.Empty).Trim(),
				Line3 = (line3 ?? string.Empty).Trim(),
				Town = (town ?? string.Empty).Trim(),
				County = (county ?? string.Empty).Trim(),
				Postcode = (postcode ?? string.Empty).Trim(),
			};

			string sValidation = m.ValidateExperianDirectorDetails();

			if (!string.IsNullOrWhiteSpace(sValidation))
				return Json(new { success = false, error = sValidation, });

			try {
				m_oServiceClient.Instance.UpdateExperianDirectorDetails(null, m_oContext.UserId, m);
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to save experian director details.");
				return Json(new { success = false, error = string.Empty, });
			} // try

			return Json(new { success = true, error = string.Empty, });
		} // SaveExperianDirector

		#endregion action SaveExperianDirector

		private readonly ServiceClient m_oServiceClient;
		private readonly IEzbobWorkplaceContext m_oContext;

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof (EsignaturesController));
	} // class EsignaturesController
} // namespace
