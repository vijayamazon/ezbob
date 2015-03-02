namespace Ezbob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[Serializable]
	[DataContract(IsReference = true)]
	public class MarketPlaceDataModel {
		[DataMember]
		public int Id { get; set; }
		[DataMember]
		public string Type { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public bool IsNew { get; set; }
		[DataMember]
		public string LastChecked { get; set; }
		[DataMember]
		public string UpdatingStatus { get; set; }
		[DataMember]
		public string UpdateError { get; set; }
		[DataMember]
		public decimal RaitingPercent { get; set; }
		[DataMember]
		public DateTime? LastTransactionDate { get; set; }
		[DataMember]
		public string AccountAge { get; set; }
		[DataMember]
		public DateTime? OriginationDate { get; set; }

		[DataMember]
		public decimal MonthSales { get; set; }
		
		public decimal MonthSalesAnnualized { get { return MonthSales * 12; } }
		[DataMember]
		public decimal AnnualSales { get; set; }
		[DataMember]
		public int UWPriority { get; set; }
		[DataMember]
		public bool Disabled { get; set; }
		[DataMember]
		public bool IsHistory { get; set; }
		[DataMember]
		public DateTime? History { get; set; }
		[DataMember]
		public string SellerInfoStoreURL { get; set; }
		[DataMember]
		public bool IsPaymentAccount { get; set; }

		//Payment account
		[DataMember]
		public decimal MonthInPayments { get; set; }
		public decimal MonthInPaymentsAnnualized { get { return MonthSales * 12; } }
		[DataMember]
		public decimal TotalNetInPayments { get; set; }
		[DataMember]
		public decimal TotalNetOutPayments { get; set; }
		[DataMember]
		public int TransactionsNumber { get; set; }

		//Trend
		[DataMember]
		public List<TurnoverTrend> TurnoverTrend { get; set; }
		
	}
}
