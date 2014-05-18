namespace EzBob.Web.Areas.Customer.Models
{
	public class EkmAccountModel
	{
		public int id { get; set; }
		public string login { get; set; }
		public string password { get; set; }
		public string displayName { get { return login; } }

		public EkmAccountModel ToModel(int id, string login)
		{
			return new EkmAccountModel
			{
				id = id,
				login = login,
			};
		} // ToModel
	}
}