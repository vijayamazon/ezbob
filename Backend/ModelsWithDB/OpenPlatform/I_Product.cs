namespace Ezbob.Backend.ModelsWithDB.OpenPlatform {
	using System.ComponentModel;
	using System.Runtime.Serialization;
    using Ezbob.Utils.dbutils;

    [DataContract(IsReference = true)]
	public class I_Product {
		[PK]
		[DataMember]
		public int ProductID { get; set; }
		
		[Length(255)]
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public bool IsDefault { get; set; }

		[DataMember]
		public bool IsEnabled { get; set; }
	}//class I_Product

	public enum I_ProductEnum {
		Loans = 1,
		Alibaba = 2,
		[Description("Credit line")]
		CreditLine = 3,
		[Description("Invoice finance")]
		InvoiceFinance = 4,
	}//enum I_ProductEnum
}//ns
