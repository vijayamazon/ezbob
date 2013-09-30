namespace YodleeLib
{
	/// <summary>
	// A common super class for all sample applications that helps
	// initialize a client, and establishes a
	// CobrandContext.
	/// </summary>
	public class ApplicationSuper
	{
		protected CobrandContextSingleton CobCxtSing;

		public ApplicationSuper()
		{
			CobCxtSing = CobrandContextSingleton.Instance;
		}

		public CobrandContext GetCobrandContext()
		{
			return CobCxtSing.GetCobrandContext();
		}
	}
}
