namespace EzBob.Backend.Strategies {
	using CommonLib;
	using EzBob.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LandRegistryEnquiry : AStrategy
	{
		private int _customerId { get; set; }
		private string _buildingNumber { get; set; }
		private string _streetName { get; set; }
		private string _cityName { get; set; }
		private string _postCode{ get; set; }

		public LandRegistryEnquiry(int customerId, string buildingNumber, string streetName, string cityName, string postCode, AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog)
		{
			_customerId = customerId;
			_buildingNumber = buildingNumber;
			_streetName = streetName;
			_postCode = postCode;
		} // constructor

		
		public override string Name { get { return "Land Registry Enquiry"; } } // Name
		public string Result { get; set; }
		public override void Execute()
		{
			var helper = new StrategyHelper();
			var response = helper.GetLandRegistryEnquiryData(_customerId, _buildingNumber, _streetName, _cityName, _postCode);
			Result = SerializeDataHelper.SerializeToString(response);
		} // Execute
	}

	public class LandRegistryRes : AStrategy
	{
		private int _customerId { get; set; }
		private string _titleNumber { get; set; }

		public LandRegistryRes(int customerId, string titleNumber, AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog)
		{
			_customerId = customerId;
			_titleNumber = titleNumber;
		} // constructor


		public override string Name { get { return "Land Registry RES"; } } // Name
		public string Result { get; set; }
		public override void Execute()
		{
			var helper = new StrategyHelper();
			var response = helper.GetLandRegistryData(_customerId, _titleNumber);
			Result = SerializeDataHelper.SerializeToString(response);
		} // Execute
	}
} // namespace EzBob.Backend.Strategies.Broker
