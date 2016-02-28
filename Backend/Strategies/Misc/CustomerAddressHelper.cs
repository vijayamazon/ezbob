namespace Ezbob.Backend.Strategies.Misc {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Backend.Models;

	public class CustomerAddressHelper : AStrategy {
		public CustomerAddressHelper(int customerId) {
			_customerId = customerId;
		} // constructor

		public override string Name {
			get { return "CustomerAddressHelper"; }
		} // Name

		public List<CustomerAddressModel> OwnedAddresses { get; set; }

		public override void Execute() {
			OwnedAddresses = GetAddresses(_customerId);
		} // Execute

		private List<CustomerAddressModel> GetAddresses(int customerId) {
			if (customerId == 0)
				return null;

			List<CustomerAddressModel> ownedAddresses = DB.Fill<CustomerAddressModel>(
				"GetCustomerOwnedAddresses",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			foreach (CustomerAddressModel address in ownedAddresses)
				address.FillDetails();

			return ownedAddresses;
		} // GetAddresses

		private readonly int _customerId;
	} // class CustomerAddressHelper
} // namespace
