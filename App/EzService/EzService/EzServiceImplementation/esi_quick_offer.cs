namespace EzService.EzServiceImplementation {
	using System;
	using System.ServiceModel;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.QuickOffer;
	using EzServiceConfiguration;
	using Ezbob.Backend.Models;
	using Ezbob.Utils.Exceptions;

	partial class EzServiceImplementation {

		public QuickOfferActionResult QuickOffer(int customerId, bool saveOfferToDB) {
			try {
				return QuickOfferProcedure(customerId, saveOfferToDB, false, null);
			}
			catch (Exception e) {
				Log.Alert(e, "Exception during executing Quick offer strategy.");
				throw new FaultException(e.Message);
			} // try
		} // QuickOffer

		public QuickOfferActionResult QuickOfferWithPrerequisites(int customerId, bool saveOfferToDB) {
			try {
				var oCfg = Ezbob.Backend.Strategies.QuickOffer.QuickOffer.LoadConfiguration();

				if (ReferenceEquals(oCfg, null))
					throw new Alert(Log, "Failed to load quick offer configuration.");

				if (oCfg.Enabled == QuickOfferEnabledStatus.Disabled) {
					return new QuickOfferActionResult {
						HasValue = false,
						Value = null,
						MetaData = ExperianCompanyCheck(1, customerId, false)
					};
				} // if

				Log.Debug("QuickOfferWithPrerequisites: performing company check for customer {0}...", customerId);
				new ExperianCompanyCheck(customerId, false).Execute();

				Log.Debug("QuickOfferWithPrerequisites: performing fraud check for customer {0}...", customerId);
				new FraudChecker(customerId, FraudMode.CompanyDetailsCheck).Execute();

				Log.Debug("QuickOfferWithPrerequisites: performing quick offer calculation for customer {0}...", customerId);
				return QuickOfferProcedure(customerId, saveOfferToDB, true, oCfg);
			}
			catch (Exception e) {
				if (!(e is AException))
					Log.Alert(e, "Exception during executing QuickOfferWithPrerequisites strategy.");

				throw new FaultException(e.Message);
			} // try
		} // QuickOfferWithPrerequisites

		private QuickOfferActionResult QuickOfferProcedure(int nCustomerID, bool bSaveOfferToDB, bool bHackForTest, QuickOfferConfigurationData oCfg) {
			QuickOffer oStrategy;

			var oResult = ExecuteSync(out oStrategy, nCustomerID, nCustomerID, nCustomerID, bSaveOfferToDB, bHackForTest, oCfg);

			return new QuickOfferActionResult {
				HasValue = !ReferenceEquals(oStrategy.Offer, null),
				Value = oStrategy.Offer,
				MetaData = oResult,
			};
		} // QuickOfferProcedure

	} // class EzServiceImplementation
} // namespace EzService
