using System;
using System.Collections.Generic;

namespace Integration.ChannelGrabberConfig {
	#region enum VendorSpecies

	public enum VendorSpecies {
		Shop       = 0,
		Accounting = 1
	} // VendorSpecies

	#endregion enum VendorSpecies

	#region class VendorInfo

	public class VendorInfo {
		#region public

		#region constructor

		public VendorInfo() {
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

		#endregion constructor

		#region properties

		public string Name { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public VendorSpecies Species { get; set; }
		public string InternalID { get; set; }
		public bool HasExpenses { get; set; }

		public SecurityData SecurityData { get; set; }

		public List<AggregatorInfo> Aggregators { get; set; }

		public ClientSide ClientSide { get; set; }

		#endregion properties

		#region method Parse

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
		} // Parse

		#endregion method Parse

		#region method Guid

		public Guid Guid() {
			return m_oGuid; 
		} // Guid

		#endregion method Guid

		#region method ToString

		public override string ToString() {
			return string.Format("Unique name: {0}\nDisplay name: {1}\nDescription: {2}\nSpecies: {6}\nInternal ID: {3}\n{4}\nAggregators:\n\t{5}",
				Name, DisplayName, Description, Guid(), SecurityData,
				string.Join("\n\t", (object[])Aggregators.ToArray()),
				Species
			);
		} // ToString

		#endregion method ToString

		#endregion public

		#region protected

		protected void SetGuid(Guid v) {
			m_oGuid = v;
		} // SetGuid

		#endregion protected

		#region private

		private Guid m_oGuid;

		#endregion private
	} // class VendorInfo

	#endregion class VendorInfo
} // namespace Integration.ChannelGrabberConfig
