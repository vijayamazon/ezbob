namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EchoSignLib;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using EzBob.Models;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class EsignaturesController : Controller {
	    private readonly DirectorRepository directorRepository;
		public EsignaturesController(IEzbobWorkplaceContext oContext, 
            DirectorRepository directorRepository) {
			this.context = oContext;
		    this.directorRepository = directorRepository;
		    this.serviceClient = new ServiceClient();
		} // constructor

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet] 
		public JsonResult Load(int? nCustomerID, bool bPollStatus) {
			Log.Debug("Loading e-signatures for customer {0} {1} polling status...", nCustomerID, bPollStatus ? "with" : "without");

			Esignature[] oSignatures;
			Esigner[] oPotentialSigners;

			try {
				EsignatureListActionResult elar = this.serviceClient.Instance.LoadEsignatures(this.context.UserId, nCustomerID, bPollStatus);
				oSignatures = elar.Data;
				oPotentialSigners = elar.PotentialSigners;
			}
			catch (Exception e) {
				Log.Warn(e, "Failed to load e-signatures for customer {0} {1} polling status.", nCustomerID, bPollStatus ? "with" : "without");
				oSignatures = new Esignature[0];
				oPotentialSigners = new Esigner[0];
			} // try

			Log.Debug("Loading e-signatures for customer {0} {1} polling status complete.", nCustomerID, bPollStatus ? "with" : "without");

			return Json(new { signatures = oSignatures, signers = oPotentialSigners, }, JsonRequestBehavior.AllowGet);
		} // Load
       
        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        public JsonResult LoadDirector(int directorId) {
            var director = this.directorRepository.Get(directorId);
            if (director == null) {
                throw new Exception(string.Format("director not found id {0}", directorId));
            }

            DirectorModel directorModel = DirectorModel.FromDirector(director, director.Company.Directors.ToList());
            return Json(directorModel, JsonRequestBehavior.AllowGet);
        } // LoadDirector

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpPost]
		[Permission(Name = "SendDocuments")]
		public JsonResult Send(string sPackage) {
			EchoSignEnvelope[] oPackage = JsonConvert.DeserializeObject<EchoSignEnvelope[]>(sPackage);

			if (oPackage == null) {
				Log.Debug("Could not extract e-sign package from {0}.", sPackage);
				return Json(new { success = false, error = "Could not extract e-sign package from input.", });
			} // if

			if (oPackage.Length == 0) {
				Log.Debug("Empty e-sign package received: {0}.", sPackage);
				return Json(new { success = false, error = "Empty e-sign package received.", });
			} // if

			EchoSignEnvelope[] oPackageToSend = oPackage.Where(x => x.IsValid).ToArray();

			if (oPackage.Length == 0) {
				Log.Debug("No envelopes are ready to be sent in: {0}.", string.Join("\n", (object[])oPackage));
				return Json(new { success = false, error = "No envelopes are ready to be sent.", });
			} // if

			Log.Debug("Send for signature request:\n{0}", string.Join("\n", (object[])oPackageToSend));

			string sResult;

			try {
				StringActionResult sar = this.serviceClient.Instance.EsignSend(this.context.UserId, oPackageToSend);
				sResult = sar.Value;
			}
			catch (Exception e) {
				Log.Warn(e, "Failed to send a package for e-signing.");
				return Json(new { success = false, error = "Failed to send a package for e-signing.", });
			} // try

			return Json(new { success = string.IsNullOrWhiteSpace(sResult), error = sResult, });
		} // Send

		[HttpGet]
		public FileResult Download(long nEsignatureID) {
			Log.Debug("Loading e-signature file for id {0}...", nEsignatureID);

			try {
				EsignatureFileActionResult efar = this.serviceClient.Instance.LoadEsignatureFile(this.context.UserId, nEsignatureID);

				Log.Debug("Loading e-signature file for id {0} complete.", nEsignatureID);

				return new FileContentResult(efar.Contents, efar.MimeType) {
					FileDownloadName = efar.FileName,
				};
			}
			catch (Exception e) {
				Log.Warn(e, "Loading e-signature file for id {0} failed.", nEsignatureID);
				throw new Exception("Failed to download requested file.");
			} // try
		} // Download

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "AddEditDirector")]
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
			Log.Debug("Saving Experian director (E-signatures controller): {0}: {1} {2}, {3} {4} {5} {6} {7} {8}",
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
				this.serviceClient.Instance.UpdateExperianDirectorDetails(null, this.context.UserId, m);
			}
			catch (Exception e) {
				Log.Warn(e, "Failed to save experian director details.");
				return Json(new { success = false, error = string.Empty, });
			} // try

			return Json(new { success = true, error = string.Empty, });
		} // SaveExperianDirector

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "AddEditDirector")]
		public JsonResult DeleteExperianDirector(int nDirectorID) {
			Log.Debug("Deleting Experian director (E-signatures controller): {0}", nDirectorID);

			if (nDirectorID <= 0)
				return Json(new { success = false, error = "Invalid director ID.", });

			try {
				this.serviceClient.Instance.DeleteExperianDirector(nDirectorID, this.context.UserId);
			}
			catch (Exception e) {
				Log.Warn(e, "Failed to delete experian director.");
				return Json(new { success = false, error = string.Empty, });
			} // try

			return Json(new { success = true, error = string.Empty, });
		} // DeleteExperianDirector

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "AddEditDirector")]
		public JsonResult DeleteDirector(int nDirectorID) {
			Log.Debug("Deleting director (E-signatures controller): {0}", nDirectorID);

			if (nDirectorID <= 0)
				return Json(new { success = false, error = "Invalid director ID.", });

			try {
				var director = this.directorRepository.Get(nDirectorID);
				if (director != null) {
					director.IsDeleted = true;
					this.directorRepository.SaveOrUpdate(director);
				}

			} catch (Exception e) {
				Log.Warn(e, "Failed to delete director {0}.", nDirectorID);
				return Json(new { success = false, error = string.Empty, });
			} // try

			return Json(new { success = true, error = string.Empty, });
		} // DeleteDirector

		private readonly ServiceClient serviceClient;
		private readonly IEzbobWorkplaceContext context;

		protected static readonly ASafeLog Log = new SafeILog(typeof (EsignaturesController));
	} // class EsignaturesController
} // namespace
