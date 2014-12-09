namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models.Marketplaces;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetBankModel : AStrategy {

		public GetBankModel(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
		} // constructor

		public override string Name {
			get { return "Get bank model"; }
		} // Name

		public override void Execute() {
			Log.Debug("Getting a bank model for customer {0}...", m_nCustomerID);
			Result = null;

			try {
				Customer customer = DbHelper.GetCustomerInfo(m_nCustomerID);

				if (customer == null) {
					Log.Warn("Could not find a customer by id {0}.", m_nCustomerID);
					return;
				} // if

				MP_CustomerMarketPlace oYodleeMp = customer.CustomerMarketPlaces
					.FirstOrDefault(mp => mp.Marketplace.InternalId == ms_oYodleeID);

				if (oYodleeMp == null) {
					Log.Debug("Customer {0} has no bank accounts.", customer.Stringify());
					return;
				} // if

				var builder = GetMpModelBuilder(oYodleeMp);

				var model = builder.Create(oYodleeMp, null);

				model.PaymentAccountBasic = builder.GetPaymentAccountModel(oYodleeMp, model, null);

				Result = model;

				Log.Debug("Getting a bank model for customer {0} complete.", customer.Stringify());
			}
			catch (Exception e) {
				Log.Warn(e, "Something went wrong while building a bank model for customer with id {0}.", m_nCustomerID);
			} // try
		} // Execute

		public MarketPlaceModel Result { get; private set; }

		private readonly int m_nCustomerID;
		private static readonly Guid ms_oYodleeID = new Guid("107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF");

	} // class GetBankModel
} // namespace
