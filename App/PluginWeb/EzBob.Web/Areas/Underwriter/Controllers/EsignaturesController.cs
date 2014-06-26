namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
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
		public JsonResult Load(int? nCustomerID) {
			ms_oLog.Debug("Loading e-signatures for customer {0}...", nCustomerID);

			Esignature[] data;

			try {
				EsignatureListActionResult elar = m_oServiceClient.Instance.LoadEsignatures(nCustomerID);
				data = elar.Data;
			}
			catch (Exception e) {
				ms_oLog.Warn(e, "Failed to load e-signatures for customer {0}.", nCustomerID);
				data = new Esignature[0];
			} // try

			ms_oLog.Debug("Loading e-signatures for customer {0} complete.", nCustomerID);

			return Json(new { aaData = data, }, JsonRequestBehavior.AllowGet);
		} // Load

		#endregion action Load

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
