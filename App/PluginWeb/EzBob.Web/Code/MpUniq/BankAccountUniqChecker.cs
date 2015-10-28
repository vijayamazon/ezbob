using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Code.MpUniq
{
    public class BankAccountUniqChecker
    {
	    private readonly ICardInfoRepository cardInfoRepository;
	    protected readonly IBankAccountWhiteListRepository whiteList;

		public BankAccountUniqChecker(ICardInfoRepository cardInfoRepository, IBankAccountWhiteListRepository whiteList)
        {
			this.cardInfoRepository = cardInfoRepository;
			this.whiteList = whiteList;
        }

        public virtual void Check(int customerID, CardInfo cardInfo)
        {
            if (this.whiteList.IsBankAccountInWhiteList(cardInfo))
            {
                return;
            }

			if (this.cardInfoRepository.Exists(cardInfo, customerID))
            {
                throw new BankAccountIsAlreadyAddedException();
            }
        }
    }
}