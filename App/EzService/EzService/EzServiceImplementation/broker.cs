namespace EzService {
	using System.Linq;
	using EzBob.Backend.Strategies.Broker;

	partial class EzServiceImplementation {
		#region method IsBroker

		public BoolActionResult IsBroker(string sContactEmail) {
			BrokerIsBroker oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null, sContactEmail);

			return new BoolActionResult {
				MetaData = oResult,
				Value = oInstance.IsBroker,
			};
		} // IsBroker

		#endregion method IsBroker

		#region method BrokerSignup

		public ActionMetaData BrokerSignup(
			string FirmName,
			string FirmRegNum,
			string ContactName,
			string ContactEmail,
			string ContactMobile,
			string MobileCode,
			string ContactOtherPhone,
			decimal EstimatedMonthlyClientAmount,
			string Password,
			string Password2
		) {
			return ExecuteSync<BrokerSignup>(null, null,
				FirmName,
				FirmRegNum,
				ContactName,
				ContactEmail,
				ContactMobile,
				MobileCode,
				ContactOtherPhone,
				EstimatedMonthlyClientAmount,
				Password,
				Password2
			);
		} // BrokerSignup

		#endregion method BrokerSignup

		#region method BrokerLogin

		public ActionMetaData BrokerLogin(string Email, string Password) {
			return ExecuteSync<BrokerLogin>(null, null, Email, Password);
		} // BrokerLogin

		#endregion method BrokerLogin

		#region method BrokerRestorePassword

		public ActionMetaData BrokerRestorePassword(string sMobile, string sCode) {
			return ExecuteSync<BrokerRestorePassword>(null, null, sMobile, sCode);
		} // BrokerRestorePassword

		#endregion method BrokerRestorePassword

		#region method BrokerLoadCustomerList

		public BrokerCustomersActionResult BrokerLoadCustomerList(string sContactEmail) {
			BrokerLoadCustomerList oIntstance;

			ActionMetaData oResult = ExecuteSync(out oIntstance, null, null, sContactEmail);

			return new BrokerCustomersActionResult {
				MetaData = oResult,
				Records = oIntstance.Result.Values.ToList(),
			};
		} // BrokerRestorePassword

		#endregion method BrokerLoadCustomerList

		#region method BrokerLoadCustomerDetails

		public BrokerCustomerDetailsActionResult BrokerLoadCustomerDetails(int nCustomerID, string sContactEmail) {
			BrokerLoadCustomerDetails oIntstance;

			ActionMetaData oResult = ExecuteSync(out oIntstance, nCustomerID, null, nCustomerID, sContactEmail);

			return new BrokerCustomerDetailsActionResult {
				MetaData = oResult,
				Data = oIntstance.Result,
			};
		} // BrokerLoadCustomerDetails

		#endregion method BrokerLoadCustomerDetails

		#region method BrokerSaveCrmEntry

		public StringActionResult BrokerSaveCrmEntry(bool bIsIncoming, int nActionID, int nStatusID, string sComment, int nCustomerID, string sContactEmail) {
			BrokerSaveCrmEntry oInstance;

			ActionMetaData oResult = ExecuteSync(
				out oInstance,
				nCustomerID,
				null,
				bIsIncoming,
				nActionID,
				nStatusID,
				sComment,
				nCustomerID,
				sContactEmail
			);

			return new StringActionResult {
				MetaData = oResult,
				Value = oInstance.ErrorMsg,
			};
		} // BrokerSaveCrmEntry

		#endregion method BrokerSaveCrmEntry

		#region method BrokerLoadCustomerFiles

		public BrokerCustomerFilesActionResult BrokerLoadCustomerFiles(int nCustomerID, string sContactEmail) {
			BrokerLoadCustomerFiles oInstance;

			ActionMetaData oResult = ExecuteSync(
				out oInstance,
				nCustomerID,
				null,
				nCustomerID,
				sContactEmail
			);

			return new BrokerCustomerFilesActionResult {
				MetaData = oResult,
				Files = oInstance.Files,
			};
		} // BrokerLoadCustomerFiles

		#endregion method BrokerLoadCustomerFiles

		#region method BrokerDownloadCustomerFile

		public BrokerCustomerFileContentsActionResult BrokerDownloadCustomerFile(int nCustomerID, string sContactEmail, int nFileID) {
			BrokerDownloadCustomerFile oInstance;

			ActionMetaData oResult = ExecuteSync(
				out oInstance,
				nCustomerID,
				null,
				nCustomerID,
				sContactEmail,
				nFileID
			);

			return new BrokerCustomerFileContentsActionResult {
				MetaData = oResult,
				Name = oInstance.FileName,
				Contents = oInstance.Contents,
			};
		} // BrokerLoadCustomerFiles

		#endregion method BrokerDownloadCustomerFile

		#region method BrokerSaveUploadedCustomerFile

		public ActionMetaData BrokerSaveUploadedCustomerFile(int nCustomerID, string sContactEmail, byte[] oFileContents, string sFileName) {
			return ExecuteSync<BrokerSaveUploadedCustomerFile>(nCustomerID, null, nCustomerID, sContactEmail, oFileContents, sFileName);
		} // BrokerSaveUploadedCustomerFile

		#endregion method BrokerSaveUploadedCustomerFile

		#region method BrokerDeleteCustomerFiles

		public ActionMetaData BrokerDeleteCustomerFiles(int nCustomerID, string sContactEmail, int[] aryFileIDs) {
			return ExecuteSync<BrokerDeleteCustomerFiles>(nCustomerID, null, nCustomerID, sContactEmail, aryFileIDs);
		} // BrokerDeleteCustomerFiles

		#endregion method BrokerDeleteCustomerFiles
	} // class EzServiceImplementation
} // namespace EzService
