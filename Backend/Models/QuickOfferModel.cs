﻿namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class QuickOfferModel {
		[DataMember]
		public int ID { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[DataMember]
		public DateTime CreationDate { get; set; }

		[DataMember]
		public DateTime ExpirationDate { get; set; }

		[DataMember]
		public int Aml { get; set; }

		[DataMember]
		public int BusinessScore { get; set; }

		[DataMember]
		public DateTime IncorporationDate { get; set; }

		[DataMember]
		public decimal TangibleEquity { get; set; }

		[DataMember]
		public decimal TotalCurrentAssets { get; set; }

		[DataMember]
		public int ImmediateTerm { get; set; }

		[DataMember]
		public decimal ImmediateInterestRate { get; set; }

		[DataMember]
		public decimal ImmediateSetupFee { get; set; }

		[DataMember]
		public decimal PotentialAmount { get; set; }

		[DataMember]
		public int PotentialTerm { get; set; }

		[DataMember]
		public decimal PotentialInterestRate { get; set; }

		[DataMember]
		public decimal PotentialSetupFee { get; set; }
	} // class QuickOfferModel
} // namespace EzBob.Backend.Models
