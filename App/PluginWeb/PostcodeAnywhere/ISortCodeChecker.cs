using EZBob.DatabaseLib.Model.Database;

namespace PostcodeAnywhere
{
    public interface ISortCodeChecker
    {
        CardInfo Check(Customer customer, string accountNumnber, string sortcode, string BankAccountType);
        void Check(CardInfo card);
    }
}