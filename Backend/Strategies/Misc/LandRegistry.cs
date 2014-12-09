namespace EzBob.Backend.Strategies.Misc {
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Serialization;
	using LandRegistryLib;

	public class LandRegistryEnquiry : AStrategy {

		public LandRegistryEnquiry(
			int customerId,
			string buildingNumber,
			string buildingName,
			string streetName,
			string cityName,
			string postCode,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_nCustomerID = customerId;
			m_sBuildingNumber = buildingNumber;
			m_sBuildingName = buildingName;
			m_sStreetName = streetName;
			m_sCityName = cityName;
			m_sPostCode  = postCode;
		} // constructor

		public override string Name { get { return "Land Registry Enquiry"; } } // Name

		public string Result { get; set; }

		public override void Execute() {
			var helper = new StrategyHelper();

			LandRegistryDataModel response = helper.GetLandRegistryEnquiryData(m_nCustomerID, m_sBuildingNumber, m_sBuildingName, m_sStreetName, m_sCityName, m_sPostCode );

			Result = new Serialized(response);
		} // Execute

		private readonly int m_nCustomerID;
		private readonly string m_sBuildingNumber;
		private readonly string m_sBuildingName;
		private readonly string m_sStreetName;
		private readonly string m_sCityName;
		private readonly string m_sPostCode;

	} // LandRegistryEnquiry

	public class LandRegistryRes : AStrategy {

		public LandRegistryRes(int customerId, string titleNumber, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = customerId;
			m_sTitleNumber = titleNumber;
		} // constructor

		public override string Name { get { return "Land Registry RES"; } } // Name

		public string Result { get; set; }

		public override void Execute() {
			var helper = new StrategyHelper();
			LandRegistry landRegistry;
			LandRegistryDataModel response = helper.GetLandRegistryData(m_nCustomerID, m_sTitleNumber, out landRegistry);
			helper.LinkLandRegistryAndAddress(m_nCustomerID, landRegistry.Response, m_sTitleNumber, landRegistry.Id);

			Result = new Serialized(response);
		} // Execute

		private readonly int m_nCustomerID;
		private readonly string m_sTitleNumber;

	} // class LandRegistryRes

} // namespace EzBob.Backend.Strategies.Broker
