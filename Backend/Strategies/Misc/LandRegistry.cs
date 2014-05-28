﻿namespace EzBob.Backend.Strategies.Misc {
	using EzBob.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Serialization;
	using LandRegistryLib;

	#region class LandRegistryEnquiry

	public class LandRegistryEnquiry : AStrategy {
		#region public

		#region constructor

		public LandRegistryEnquiry(
			int customerId,
			string buildingNumber,
			string streetName,
			string cityName,
			string postCode,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_nCustomerID = customerId;
			m_sBuildingNumber = buildingNumber;
			m_sStreetName = streetName;
			m_sCityName = cityName;
			m_sPostCode  = postCode;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Land Registry Enquiry"; } } // Name

		#endregion property Name

		#region property Result

		public string Result { get; set; }

		#endregion property Result

		#region method Execute

		public override void Execute() {
			var helper = new StrategyHelper();

			LandRegistryDataModel response = helper.GetLandRegistryEnquiryData(m_nCustomerID, m_sBuildingNumber, m_sStreetName, m_sCityName, m_sPostCode );

			Result = new Serialized(response);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;
		private readonly string m_sBuildingNumber;
		private readonly string m_sStreetName;
		private readonly string m_sCityName;
		private readonly string m_sPostCode;

		#endregion private
	} // LandRegistryEnquiry

	#endregion class LandRegistryEnquiry

	#region class LandRegistryRes

	public class LandRegistryRes : AStrategy {
		#region public

		#region constructor

		public LandRegistryRes(int customerId, string titleNumber, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = customerId;
			m_sTitleNumber = titleNumber;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Land Registry RES"; } } // Name

		#endregion property Name

		#region property Result

		public string Result { get; set; }

		#endregion property Result

		#region method Execute

		public override void Execute() {
			var helper = new StrategyHelper();

			LandRegistryDataModel response = helper.GetLandRegistryData(m_nCustomerID, m_sTitleNumber);

			Result = new Serialized(response);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;
		private readonly string m_sTitleNumber;

		#endregion private
	} // class LandRegistryRes

	#endregion class LandRegistryRes
} // namespace EzBob.Backend.Strategies.Broker
