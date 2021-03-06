﻿namespace EzBob.Models.Marketplaces
{
	using System.Collections.Generic;
	using Yodlee;
	using Web.Areas.Underwriter.Models;
	using System;


	public class MarketPlaceModel
	{
		public int Id { get; set; }
		public string Type { get; set; }
		public string Name { get; set; }
		public bool IsNew { get; set; }
		public string LastChecked { get; set; }
		public string UpdatingStatus { get; set; }
		public string UpdateError { get; set; }
		public decimal RaitingPercent { get; set; }
		public string SellerInfoStoreURL { get; set; }
		public IEnumerable<string> Categories { get; set; }
		
		public DateTime? LastTransactionDate { get; set; }

		//Amazon
		public double AmazonSelerRating { get; set; }
		public string AskvilleStatus { get; set; }
		public string AskvilleGuid { get; set; }

		//eBay
		public EBayModel EBay { get; set; }

		public PayPalAccountModel PayPal { get; set; }

		//Aggregates
		public Dictionary<string, string> AnalysisDataInfo { get; set; }

		public string AccountAge { get; set; }
		public decimal MonthSales { get; set; }
		public decimal MonthSalesAnnualized { get { return MonthSales * 12; } }
		public decimal AnnualSales { get; set; }

		//Payment Aggregates
		public decimal MonthInPayments { get; set; }
		public decimal MonthInPaymentsAnnualized {get {return MonthInPayments * 12;}}
		public string Status { get; set; }
		public decimal TotalNetInPayments { get; set; }
		public decimal TotalNetOutPayments { get; set; }
		public int TransactionsNumber { get; set; }

		//Feedbacks
		public int? PositiveFeedbacks { get; set; }
		public int? NegativeFeedbacks { get; set; }
		public int? NeutralFeedbacks { get; set; }

		//Yodlee
		public YodleeModel Yodlee { get; set; }

		public HmrcData HmrcData { get; set; }

		public bool IsPaymentAccount { get; set; }

		public FreeAgentModel FreeAgent { get; set; }

		public SageModel Sage { get; set; }

		//Company Files
		public CompanyFilesModel CompanyFiles { get; set; }

		public int UWPriority { get; set; }

		public bool Disabled { get; set; }

		public bool IsHistory { get; set; }

		public DateTime? History { get; set; }
	}
}