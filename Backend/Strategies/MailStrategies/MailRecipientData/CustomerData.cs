namespace EzBob.Backend.Strategies.MailStrategies {
	using Exceptions;
	using Ezbob.Database;

	public class CustomerData {
		#region constructor

		public CustomerData(AStrategy oStrategy, int nCustomerID, AConnection oDB) {
			Strategy = oStrategy;
			RequestedID = nCustomerID;
			DB = oDB;
		} // constructor

		#endregion constructor

		#region method Load

		public virtual void Load() {
			LoadFromSp("GetBasicCustomerData");
		} // Load

		#endregion method Load

		#region method LoadCustomerOrBroker

		public virtual void LoadCustomerOrBroker() {
			LoadFromSp("GetBasicCustomerOrBrokerData");
		} // LoadCustomerOrBroker

		#endregion method LoadCustomerOrBroker

		#region method ToString

		public override string ToString() {
			return string.Format(
				"{0}: {1} {2} ({5}, {3}) {4} loan #: {6}, mobile: {7}, land line: {8}, test: {9} {10}, broker: {11}, filled by broker: {12}",
				Id,
				FirstName,
				Surname,
				FullName,
				Mail,
				IsOffline ? "offline" : "online",
				NumOfLoans,
				MobilePhone,
				DaytimePhone,
				IsTest ? "yes" : "no",
				IsWhiteLabel ? ", white label" : "",
				IsCampaign ? ", campaign" : ""
				BrokerID > 0 ? BrokerID.ToString() : "none",
				IsFilledByBroker ? "yes" : "no"
			);
		} // ToString

		#endregion method ToString

		#region properties

		public virtual int Id { get; protected set; }
		public virtual string FirstName { get; protected set; }
		public virtual string Surname { get; protected set; }
		public virtual string FullName { get; protected set; }
		public virtual string Mail { get; protected set; }
		public virtual bool IsOffline { get; protected set; }
		public virtual int NumOfLoans { get; protected set; }
		public virtual string RefNum { get; protected set; }
		public virtual string MobilePhone { get; protected set; }
		public virtual string DaytimePhone { get; protected set; }
		public virtual bool IsTest { get; protected set; }
		public virtual string Postcode { get; protected set; }
		public virtual string City { get; protected set; }
		public virtual int UserID { get; protected set; }
		public virtual bool IsWhiteLabel { get; protected set; }
		public virtual bool IsCampaign { get; protected set; }
		public virtual int BrokerID { get; protected set; }
		public virtual bool IsFilledByBroker { get; protected set; }

		#endregion properties

		protected AStrategy Strategy { get; private set; }
		protected AConnection DB { get; private set; }
		protected int RequestedID { get; private set; }

		#region private

		private void LoadFromSp(string sSpName) {
			SafeReader sr = DB.GetFirst(
				sSpName,
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", RequestedID)
			);

			Id = sr["Id"];

			if (Id != RequestedID)
				throw new StrategyWarning(Strategy, "Failed to find a customer by id " + RequestedID);

			FirstName = sr["FirstName"];
			Surname = sr["Surname"];
			FullName = sr["FullName"];
			Mail = sr["Mail"];
			IsOffline = sr["IsOffline"];
			NumOfLoans = sr["NumOfLoans"];
			RefNum = sr["RefNum"];
			MobilePhone = sr["MobilePhone"];
			DaytimePhone = sr["DaytimePhone"];
			IsTest = sr["IsTest"];
			Postcode = sr["Postcode"];
			City = sr["City"];
			UserID = sr["UserID"];
			IsWhiteLabel = sr["IsWhiteLabel"];
			IsCampaign = sr["IsCampaign"];
			BrokerID = sr["BrokerID"];
			IsFilledByBroker = sr["IsFilledByBroker"];
		} // Load

		#endregion private
	} // class CustomerData
} // namespace
