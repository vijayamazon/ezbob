namespace EzService.EzServiceImplementation {
	using System.Collections.Generic;
	using EchoSignLib;
	using Ezbob.Backend.Strategies.Esign;
	using Ezbob.Backend.Models;

	partial class EzServiceImplementation {
		public ActionMetaData EsignProcessPending(int? nCustomerID) {
			return Execute<EsignProcessPending>(null, null, nCustomerID);
		} // EsignProcessPending

		public EsignatureListActionResult LoadEsignatures(int userId, int? nCustomerID, bool bPollStatus) {
			LoadEsignatures oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nCustomerID, userId, nCustomerID, bPollStatus);

			List<Esignature> eSignData = new List<Esignature>();

			oInstance.Result.ForEach((ignored, longIgnored, oSignature) => eSignData.Add(oSignature));

			return new EsignatureListActionResult {
				MetaData = oMetaData,
				Data = eSignData,
				PotentialSigners = oInstance.PotentialEsigners,
			};
		} // LoadEsignatures

		public EsignatureFileActionResult LoadEsignatureFile(int userId, long nEsignatureID) {
			LoadEsignatureFile oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, userId, nEsignatureID);

			return new EsignatureFileActionResult {
				MetaData = oMetaData,
				FileName = oInstance.FileName,
				MimeType = oInstance.MimeType,
				Contents = oInstance.Contents,
			};
		} // LoadEsignatureFile

		public StringActionResult EsignSend(int userId, EchoSignEnvelope[] oPackage) {
			EsignSend oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, new ExecuteArguments {
				UserID = userId,
				OnInit = (s, amd) => ((EsignSend)s).Package = oPackage,
			});

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstance.Result,
			};
		} // EsignSend
	} // class EzServiceImplementation
} // namespace
