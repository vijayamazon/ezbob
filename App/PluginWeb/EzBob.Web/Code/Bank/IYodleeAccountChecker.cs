namespace EzBob.Web.Code.Bank
{
	using EZBob.DatabaseLib.Model.Database;

	public interface IYodleeAccountChecker
	{
		void Check(Customer customer, string accountNumber, string sortcode, string bankAccountType);
	}
}