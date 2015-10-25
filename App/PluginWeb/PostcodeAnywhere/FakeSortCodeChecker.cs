using EZBob.DatabaseLib.Model.Database;

namespace PostcodeAnywhere
{
	using System;

	public class FakeSortCodeChecker : ISortCodeChecker
    {

		public FakeSortCodeChecker(int maxBankAccountValidationAttempts)
		{
			
		}

        public CardInfo Check(Customer customer, string accountNumber, string sortcode, string bankAccountType)
        {
            return new CardInfo { 
				BankAccount = accountNumber, 
				SortCode = sortcode, 
				Customer = customer,
				Type = (BankAccountType)Enum.Parse(typeof(BankAccountType), bankAccountType) };
        }

        public void Check(CardInfo card)
        {
        }
    }
}