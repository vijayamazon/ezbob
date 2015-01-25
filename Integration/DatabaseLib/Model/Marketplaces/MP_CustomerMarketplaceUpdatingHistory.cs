﻿namespace EZBob.DatabaseLib.Model.Database {
	using Marketplaces.Amazon;
	using Marketplaces.FreeAgent;
	using Marketplaces.Sage;
	using Marketplaces.Yodlee;
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Repository.Turnover;
	using Iesi.Collections.Generic;

	public class MP_CustomerMarketplaceUpdatingHistory {
		public virtual Iesi.Collections.Generic.ISet<MP_CustomerMarketplaceUpdatingActionLog> ActionLog { get; set; }
		public virtual MP_AmazonFeedback AmazonFeedback { get; set; }
		public virtual MP_AmazonOrder AmazonOrder { get; set; }
		public virtual MP_ChannelGrabberOrder ChannelGrabberOrder { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual MP_EbayFeedback EbayFeedback { get; set; }
		public virtual MP_EbayOrder EbayOrder { get; set; }
		public virtual MP_EbayUserAccountData EbayUserAccountData { get; set; }
		public virtual MP_EbayUserData EbayUserData { get; set; }
		public virtual MP_EkmOrder EkmOrder { get; set; }
		public virtual string Error { get; set; }
		public virtual MP_FreeAgentRequest FreeAgentRequest { get; set; }
		public virtual int Id { get; set; }
		public virtual MP_PayPalTransaction PayPalTransaction { get; set; }
		public virtual MP_PayPointOrder PayPointOrder { get; set; }
		public virtual MP_SageRequest SageRequest { get; set; }
		public virtual MP_TeraPeakOrder TeraPeakOrder { get; set; }
		public virtual DateTime? UpdatingEnd { get; set; }
		public virtual DateTime? UpdatingStart { get; set; }
		public virtual MP_YodleeOrder YodleeOrder { get; set; }
		public virtual int? UpdatingTimePassInSeconds { get; set; }

		public virtual Iesi.Collections.Generic.ISet<HmrcAggregation> HmrcAggregations { get; set; }
		public virtual Iesi.Collections.Generic.ISet<AmazonAggregation> AmazonAggregations { get; set; }
		 public virtual Iesi.Collections.Generic.ISet<EbayAggregation> EbayAggregations { get; set; }
		public virtual Iesi.Collections.Generic.ISet<PayPalAggregation> PayPalAggregations { get; set; }
		public virtual Iesi.Collections.Generic.ISet<YodleeAggregation> YodleeAggregations { get; set; }

		public MP_CustomerMarketplaceUpdatingHistory() {
			ActionLog = new HashedSet<MP_CustomerMarketplaceUpdatingActionLog>();

			this.HmrcAggregations = new HashedSet<HmrcAggregation>();
			this.AmazonAggregations = new HashedSet<AmazonAggregation>();			
			this.YodleeAggregations = new HashedSet<YodleeAggregation>();
			this.EbayAggregations = new HashedSet<EbayAggregation>();
			this.PayPalAggregations = new HashedSet<PayPalAggregation>();
		}
	}
}
