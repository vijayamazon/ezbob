using EZBob.DatabaseLib.Model.Database;

namespace PostcodeAnywhere
{
    public interface ISortCodeChecker
    {
        CardInfo Check(Customer customer, string accountNumber, string sortcode, string bankAccountType);
        void Check(CardInfo card);
    }
}