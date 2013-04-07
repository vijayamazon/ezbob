using System;
using EZBob.DatabaseLib.Model.Database;

namespace PostcodeAnywhere
{
    public class FakeSortCodeChecker : ISortCodeChecker
    {
        public CardInfo Check(Customer customer, string accountNumnber, string sortcode, string bankAccountType)
        {
            return null;
        }

        public void Check(CardInfo card)
        {
        }
    }
}