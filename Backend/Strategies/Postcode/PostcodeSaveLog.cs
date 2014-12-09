namespace EzBob.Backend.Strategies.Postcode {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class PostcodeSaveLog : AStrategy {

		public PostcodeSaveLog(
			string sRequestType,
			string sUrl,
			string sStatus,
			string sResponseData,
			string sErrorMessage,
			int nUserID,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_oSp = new SpPostcodeSaveLog(DB, Log) {
				RequestType = sRequestType,
				Url = sUrl,
				Status = sStatus,
				ResponseData = sResponseData,
				ErrorMessage = sErrorMessage,
				UserID = nUserID,
			};
		} // constructor

		public override string Name {
			get { return "Postcode save log"; }
		} // Name

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
		} // Execute

		private readonly SpPostcodeSaveLog m_oSp;

		private class SpPostcodeSaveLog : AStoredProc {
			public SpPostcodeSaveLog(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				if (UserID < 0)
					return false;

				if (string.IsNullOrWhiteSpace(RequestType))
					return false;

				if (string.IsNullOrWhiteSpace(Url))
					return false;

				if (string.IsNullOrWhiteSpace(Status))
					return false;

				return true;
			} // HasValidParameters

			public string RequestType { get; set; }
			public string Url { get; set; } 
			public string Status { get; set; } 
			public string ResponseData { get; set; } 
			public string ErrorMessage { get; set; } 
			public int UserID { get; set; }
			public DateTime InsertDate //don't delete
			{
				get { return DateTime.UtcNow; } // get
				set { } // set, should exist
			} // InsertDate
		} // class SpPostcodeSaveLog

	} // class PostcodeSaveLog
} // namespace EzBob.Backend.Strategies.Postcode
