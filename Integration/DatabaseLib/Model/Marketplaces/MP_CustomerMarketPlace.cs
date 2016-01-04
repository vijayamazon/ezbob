namespace EZBob.DatabaseLib.Model.Database {
	using System.Linq;
	using Common;
	using DatabaseWrapper;
	using Marketplaces.Amazon;
	using Marketplaces.FreeAgent;
	using Marketplaces.Sage;
	using Marketplaces.Yodlee;
	using System;
	using Ezbob.Database;
	using Iesi.Collections.Generic;
	using StructureMap;

	public static class MP_CustomerMarketPlaceExt {
		public static IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(this MP_CustomerMarketPlace mp) {
			return ObjectFactory
				.GetNamedInstance<IMarketplaceType>(mp.Marketplace.Name)
				.GetRetrieveDataHelper(ObjectFactory.GetInstance<DatabaseDataHelper>());
		}

		public static string Stringify(this MP_CustomerMarketPlace mp) {
			if (mp == null)
				return "-- null --";

			return string.Format("{1} ({0} of type {2})", mp.Id, mp.DisplayName, mp.Marketplace == null ? "unknown" : mp.Marketplace.Name);
		} // Stringify

		// GetRetrieveDataHelper
	}

	public class MP_CustomerMarketPlace : IDatabaseCustomerMarketPlace {
		public virtual ISet<MP_AmazonFeedback> AmazonFeedback { get; set; }

		public virtual MP_AmazonMarketplaceType AmazonMarketPlace { get; set; }

		public virtual ISet<MP_AmazonOrder> AmazonOrders { get; set; }

		public virtual ISet<MP_ChannelGrabberOrder> ChannelGrabberOrders { get; set; }

		public virtual DateTime? Created { get; set; }

		public virtual Customer Customer { get; set; }

		/// <summary>
		/// True if marketplace is disabled. Do not show totals, seniority and other
		/// stuff for such marketplaces.
		/// </summary>
		public virtual bool Disabled { get; set; }

		public virtual string DisplayName { get; set; }

		public virtual ISet<MP_EbayFeedback> EbayFeedback { get; set; }

		public virtual ISet<MP_EbayOrder> EbayOrders { get; set; }

		public virtual ISet<MP_EbayUserAccountData> EbayUserAccountData { get; set; }

		public virtual ISet<MP_EbayUserData> EbayUserData { get; set; }

		public virtual ISet<MP_EkmOrder> EkmOrders { get; set; }

		public virtual ISet<MP_FreeAgentRequest> FreeAgentRequests { get; set; }

		public virtual int Id { get; set; }

		IMarketplaceType IDatabaseCustomerMarketPlace.Marketplace {
			get {
				return _mpType;
			}
		}

		public virtual bool IsNew {
			get {
				return Library.Instance.DB.ExecuteScalar<bool>(
					"IsMarketplaceNew",
					CommandSpecies.StoredProcedure,
					new QueryParameter("MpID", Id)
				);
			}
		}

		public virtual DateTime? LastTransactionDate { get; set; }

		public virtual MP_MarketplaceType Marketplace { get; set; }

		/// <summary>
		/// Date of the first order/transaction for marketplace
		/// </summary>
		public virtual DateTime? OriginationDate { get; set; }

		public virtual ISet<MP_PayPalTransaction> PayPalTransactions { get; set; }

		public virtual ISet<MP_PayPointOrder> PayPointOrders { get; set; }

		public virtual MP_PayPalPersonalInfo PersonalInfo { get; set; }

		public virtual ISet<MP_RtiTaxMonthRecord> RtiTaxMonthRecords { get; set; }

		public virtual ISet<MP_SageRequest> SageRequests { get; set; }

		public virtual byte[] SecurityData { get; set; }

		public virtual ISet<MP_TeraPeakOrder> TeraPeakOrders { get; set; }

		public virtual DateTime? Updated { get; set; }

		public virtual string UpdateError { get; set; }

		public virtual DateTime? UpdatingEnd { get; set; }

		public virtual ISet<MP_CustomerMarketplaceUpdatingHistory> UpdatingHistory { get; set; }

		public virtual DateTime? UpdatingStart { get; set; }

		public virtual ISet<MP_VatReturnRecord> VatReturnRecords { get; set; }

		public virtual ISet<MP_YodleeOrder> YodleeOrders { get; set; }

		public MP_CustomerMarketPlace() {
			PayPalTransactions = new HashedSet<MP_PayPalTransaction>();
			EbayOrders = new HashedSet<MP_EbayOrder>();
			AmazonOrders = new HashedSet<MP_AmazonOrder>();
			UpdatingHistory = new HashedSet<MP_CustomerMarketplaceUpdatingHistory>();
			EbayUserData = new HashedSet<MP_EbayUserData>();
			EbayUserAccountData = new HashedSet<MP_EbayUserAccountData>();
			EbayFeedback = new HashedSet<MP_EbayFeedback>();
			AmazonFeedback = new HashedSet<MP_AmazonFeedback>();
			TeraPeakOrders = new HashedSet<MP_TeraPeakOrder>();
			EkmOrders = new HashedSet<MP_EkmOrder>();
			FreeAgentRequests = new HashedSet<MP_FreeAgentRequest>();
			SageRequests = new HashedSet<MP_SageRequest>();
			ChannelGrabberOrders = new HashedSet<MP_ChannelGrabberOrder>();
			VatReturnRecords = new HashedSet<MP_VatReturnRecord>();
			RtiTaxMonthRecords = new HashedSet<MP_RtiTaxMonthRecord>();
			PayPointOrders = new HashedSet<MP_PayPointOrder>();
			YodleeOrders = new HashedSet<MP_YodleeOrder>();
		}

		public virtual string GetUpdatingError(DateTime? history) {
			string error = null;
			if (history.HasValue) {
				var mpCustomerMarketplaceUpdatingHistory =
					UpdatingHistory.FirstOrDefault(h => h.UpdatingStart >= history.Value);
				if (mpCustomerMarketplaceUpdatingHistory != null &&
					!string.IsNullOrEmpty(mpCustomerMarketplaceUpdatingHistory.Error)) {
					error = mpCustomerMarketplaceUpdatingHistory.Error;
				}
			} else { error = UpdateError; }
			return error;
		}

		public virtual string GetUpdatingStatus(DateTime? history = null) {
			string status = "Done";
			if (history.HasValue) {
				var mpCustomerMarketplaceUpdatingHistory =
					UpdatingHistory.FirstOrDefault(h => h.UpdatingStart >= history.Value);
				if (mpCustomerMarketplaceUpdatingHistory != null &&
					!string.IsNullOrEmpty(mpCustomerMarketplaceUpdatingHistory.Error)) {
					status = "Error";
				}
			} else {
				if (!string.IsNullOrWhiteSpace(UpdateError))
					status = "Error";
				else if (UpdatingEnd != null)
					status = "Done";
				else if (UpdatingStart != null)
					status = "In progress";
				else
					status = "Never started";
			}
			return status;
		}

		public virtual void SetIMarketplaceType(IMarketplaceType marketplaceType) {
			_mpType = marketplaceType;
		}

		private IMarketplaceType _mpType;
	}
}
