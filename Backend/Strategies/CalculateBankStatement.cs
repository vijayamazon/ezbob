namespace EzBob.Backend.Strategies {
	using System;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models.Marketplaces;
	using EzBob.Models.Marketplaces.Builders;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;

	public class CalculateBankStatement : AStrategy {
		#region public

		#region constructor

		public CalculateBankStatement(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "CalculateBankStatement"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Result = null;

			try {
				Customer customer = Helper.GetCustomerInfo(m_nCustomerID);

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

				var builder = GetBuilder(oYodleeMp);

				var model = builder.Create(oYodleeMp, null);

				model.PaymentAccountBasic = builder.GetPaymentAccountModel(oYodleeMp, model, null);

				Result = model;
			}
			catch (Exception e) {
				Log.Warn(e, "Something went wrong while building a bank model for customer with id {0}.", m_nCustomerID);
			} // try
		} // Execute

		#endregion method Execute

		public MarketPlaceModel Result { get; private set; }

		#endregion public

		#region private

		private static IMarketplaceModelBuilder GetBuilder(MP_CustomerMarketPlace mp) {
			var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
			return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
		} // GetBuilder

		#region property Helper

		private DatabaseDataHelper Helper {
			get { return ObjectFactory.GetInstance<DatabaseDataHelper>(); }
		} // Helper

		#endregion property Helper

		private readonly int m_nCustomerID;
		private static readonly Guid ms_oYodleeID = new Guid("107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF");

		#endregion private
	} // class CalculateBankStatement
} // namespace
