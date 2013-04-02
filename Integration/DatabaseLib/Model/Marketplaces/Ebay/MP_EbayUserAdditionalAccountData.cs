namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayUserAdditionalAccountData
	{
		public virtual int Id { get; set; }
		public virtual MP_EbayUserAccountData EbayUserAccountData { get; set; }
		public virtual string Currency { get; set; }
		public virtual double? Balance { get; set; }
		public virtual string AccountCode { get; set; }
	}
}