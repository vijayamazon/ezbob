namespace Integration.ChannelGrabberConfig {
	using System;

	public class VendorInfo {
		public Behaviour Behaviour { get; set; }

		public ClientSide ClientSide { get; set; }

		public string Description { get; set; }

		public string DisplayName { get; set; }

		public bool HasExpenses { get; set; }

		public string InternalID { get; set; }

		public bool IsPaymentAccount { get; set; }

		public string Name { get; set; }

		public SecurityData SecurityData { get; set; }

		public VendorInfo() {
			Behaviour = Behaviour.Default;
			Name = "";
			DisplayName = "";
			Description = "";
			InternalID = (new Guid()).ToString();
			HasExpenses = false;
			SecurityData = new SecurityData();
			ClientSide = new ClientSide();
			m_oGuid = new Guid();
		} // constructor

		public Guid Guid() {
			return m_oGuid;
		} // Guid

		public void Parse() {
			Name = (Name ?? "").Trim();

			if (Name == string.Empty)
				throw new ConfigException("Vendor name not specified.");

			DisplayName = (DisplayName ?? "").Trim();
			if (DisplayName == string.Empty)
				DisplayName = Name;

			Description = (Description ?? "").Trim();
			if (Description == string.Empty)
				Description = Name;

			try {
				m_oGuid = new Guid(InternalID);
			} catch (Exception e) {
				throw new ConfigException("Failed to parse vendor internal id: " + e.Message);
			} // catch

			SecurityData.Validate();

			ClientSide.Parse();
		} // Parse

		public override string ToString() {
			return string.Format("Unique name: {0}\nBehaviour: {6}\nDisplay name: {1}\nDescription: {2}\nHas expenses: {5}\nInternal ID: {3}\n{4}",
				Name, DisplayName, Description, Guid(), SecurityData,
				HasExpenses ? "yes" : "no",
				Behaviour
			);
		} // ToString

		public const string TopSecret = "topsecret";

		protected void SetGuid(Guid v) {
			m_oGuid = v;
		} // SetGuid

		private Guid m_oGuid;
	} // class VendorInfo
} // namespace Integration.ChannelGrabberConfig
