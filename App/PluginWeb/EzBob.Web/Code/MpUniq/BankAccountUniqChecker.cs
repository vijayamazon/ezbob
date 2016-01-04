namespace EzBob.Web.Code.MpUniq {
	using EZBob.DatabaseLib.Model.Database;

	public class BankAccountUniqChecker {
		public BankAccountUniqChecker(ICardInfoRepository cardInfoRepository, IBankAccountWhiteListRepository whiteList) {
			this.cardInfoRepository = cardInfoRepository;
			this.whiteList = whiteList;
		} // constructor

		public virtual void Check(int customerID, CardInfo cardInfo) {
			if (this.whiteList.IsBankAccountInWhiteList(cardInfo))
				return;

			if (this.cardInfoRepository.Exists(cardInfo, customerID))
				throw new BankAccountIsAlreadyAddedException();
		} // Check

		private readonly IBankAccountWhiteListRepository whiteList;
		private readonly ICardInfoRepository cardInfoRepository;
	} // class BankAccountUniqChecker
} // namespace