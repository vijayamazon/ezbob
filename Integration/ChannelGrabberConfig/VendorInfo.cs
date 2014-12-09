using System;
using System.Collections.Generic;

namespace Integration.ChannelGrabberConfig {

	public class VendorInfo {

		public const string TopSecret = "topsecret";

		public VendorInfo() {
			Behaviour = Behaviour.Default;
			Name = "";
			DisplayName = "";
			Description = "";
			InternalID = (new Guid()).ToString();
			HasExpenses = false;
			SecurityData = new SecurityData();
			Aggregators = new List<AggregatorInfo>();
			ClientSide = new ClientSide();
			m_oGuid = new Guid();
		} // constructor

		public string Name { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public string InternalID { get; set; }
		public bool HasExpenses { get; set; }
		public bool IsPaymentAccount { get; set; }
		public Behaviour Behaviour { get; set; }

		public SecurityData SecurityData { get; set; }

		public List<AggregatorInfo> Aggregators { get; set; }

		public ClientSide ClientSide { get; set; }

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
			}
			catch (Exception e) {
				throw new ConfigException("Failed to parse vendor internal id: " + e.Message);
			} // catch

			SecurityData.Validate();

			if (Aggregators.Count == 0)
				throw new ConfigException("Aggregators not specified.");

			Aggregators.ForEach(a => a.Parse());

			ClientSide.Parse();
		} // Parse

		public Guid Guid() {
			return m_oGuid; 
		} // Guid

		public override string ToString() {
			return string.Format("Unique name: {0}\nBehaviour: {7}\nDisplay name: {1}\nDescription: {2}\nHas expenses: {6}\nInternal ID: {3}\n{4}\nAggregators:\n\t{5}",
				Name, DisplayName, Description, Guid(), SecurityData,
				string.Join("\n\t", (object[])Aggregators.ToArray()),
				HasExpenses ? "yes" : "no",
				Behaviour
			);
		} // ToString

		protected void SetGuid(Guid v) {
			m_oGuid = v;
		} // SetGuid

		private Guid m_oGuid;

	} // class VendorInfo

} // namespace Integration.ChannelGrabberConfig
