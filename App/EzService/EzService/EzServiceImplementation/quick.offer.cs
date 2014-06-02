﻿namespace EzService.EzServiceImplementation {
	using System;
	using System.ServiceModel;
	using Exceptions;
	using EzBob.Backend.Strategies.Exceptions;
	using EzBob.Backend.Strategies.Experian;
	using EzBob.Backend.Strategies.Misc;
	using EzBob.Backend.Strategies.QuickOffer;
	using EzServiceConfiguration;
	using Ezbob.Backend.Models;

	partial class EzServiceImplementation {
		#region public

		#region IEzService exposed methods

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
				var oCfg = EzBob.Backend.Strategies.QuickOffer.QuickOffer.LoadConfiguration(DB, Log);

				if (ReferenceEquals(oCfg, null))
					throw new ServiceAlert("Failed to load quick offer configuration.");

				if (oCfg.Enabled == QuickOfferEnabledStatus.Disabled) {
					return new QuickOfferActionResult {
						HasValue = false,
						Value = null,
						MetaData = ExperianCompanyCheck(customerId, false)
					};
				} // if

				Log.Debug("QuickOfferWithPrerequisites: performing company check for customer {0}...", customerId);
				new ExperianCompanyCheck(customerId, false, DB, Log).Execute();

				Log.Debug("QuickOfferWithPrerequisites: performing fraud check for customer {0}...", customerId);
				new FraudChecker(customerId, FraudMode.CompanyDetailsCheck, DB, Log).Execute();

				Log.Debug("QuickOfferWithPrerequisites: performing quick offer calculation for customer {0}...", customerId);
				return QuickOfferProcedure(customerId, saveOfferToDB, true, oCfg);
			}
			catch (Exception e) {
				if (!(e is AStrategyException))
					Log.Alert(e, "Exception during executing QuickOfferWithPrerequisites strategy.");

				throw new FaultException(e.Message);
			} // try
		} // QuickOfferWithPrerequisites

		#endregion IEzService exposed methods

		#endregion public

		#region private

		private QuickOfferActionResult QuickOfferProcedure(int nCustomerID, bool bSaveOfferToDB, bool bHackForTest, QuickOfferConfigurationData oCfg) {
			QuickOffer oStrategy;

			var oResult = ExecuteSync(out oStrategy, nCustomerID, nCustomerID, nCustomerID, bSaveOfferToDB, bHackForTest, oCfg);

			return new QuickOfferActionResult {
				HasValue = !ReferenceEquals(oStrategy.Offer, null),
				Value = oStrategy.Offer,
				MetaData = oResult,
			};
		} // QuickOfferProcedure

		#endregion private
	} // class EzServiceImplementation
} // namespace EzService
