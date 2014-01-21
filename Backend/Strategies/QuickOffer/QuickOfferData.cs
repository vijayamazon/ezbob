namespace EzBob.Backend.Strategies.QuickOffer {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class QuickOfferData

	class QuickOfferData {
		#region public

		#region constructor

		public QuickOfferData(ASafeLog oLog) {
			m_oLog = new SafeLog(oLog);

		} // constructor

		#endregion constructor

		#region method Load

		public void Load(SafeReader oReader) {
			CustomerID = oReader["CustomerID"];
			IsOffline = oReader["IsOffline"];
			CompanyRefNum = oReader["CompanyRefNum"];
			DefaultCount = oReader["DefaultCount"];
			AmlID = oReader["AmlID"];
			AmlData = oReader["AmlData"];
			PersonalID = oReader["PersonalID"];
			PersonalScore = oReader["PersonalScore"];
			CompanyID = oReader["CompanyID"];
			CompanyData = oReader["CompanyData"];
		} // Load

		#endregion method Load

		#region method IsValid

		public bool IsValid() {
			return CustomerID > 0;
		} // IsValid

		#endregion method IsValid

		#region method Calculate

		public decimal? Calculate() {
			// TODO
			return null;
		} // Calculate

		#endregion method Calculate

		#endregion public

		#region private

		private int CustomerID;
		private bool IsOffline;
		private string CompanyRefNum;
		private int DefaultCount;
		private long AmlID;
		private string AmlData;
		private long PersonalID;
		private int PersonalScore;
		private long CompanyID;
		private string CompanyData;

		private SafeLog m_oLog;

		#endregion private
	} // class QuickOfferData

	#endregion class QuickOfferData
} // namespace EzBob.Backend.Strategies.QuickOffer
