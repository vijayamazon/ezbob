namespace Ezbob.Backend.Strategies.Lottery {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Backend.Strategies;

	public class EnlistLottery : AStrategy {
		public EnlistLottery(int customerID, AConnection db, ASafeLog log)
			: base(db, log) {
			this.customerID = customerID;
			this.isFilled = false;
			this.brokerID = 0;
			this.lotteries = new List<LotteryDataForEnlisting>();
			this.loans = new List<LoanData>();
		} // constructor

		public override string Name {
			get { return "EnlistLottery"; }
		} // Name

		public override void Execute() {
			DB.ForEachRowSafe(
				sr => {
					RowType rt;

					if (!Enum.TryParse(sr["RowType"], true, out rt))
						return;

					switch (rt) {
					case RowType.MetaData:
						this.isFilled = true;
						this.brokerID = sr["BrokerID"];
						break;

					case RowType.Lottery:
						this.lotteries.Add(sr.Fill<LotteryDataForEnlisting>());
						break;

					case RowType.Loan:
						this.loans.Add(sr.Fill<LoanData>());
						break;

					default:
						throw new ArgumentOutOfRangeException();
					} // switch
				},
				"FindRelevantLotteries",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			Log.Info(
				"Customer id: {0}, broker id: {1}; lotteries count: {2}; total loan count: {3}.",
				this.customerID,
				this.brokerID,
				this.lotteries.Count,
				this.loans.Count
			);

			if (!this.isFilled || (this.lotteries.Count < 1)) {
				Log.Debug("Exiting (either no lotteries or no meta data).");
				return;
			} // if

			foreach (LotteryDataForEnlisting ld in this.lotteries)
				if (Enlist(ld))
					break;
		} // Execute

		private bool Enlist(LotteryDataForEnlisting ld) {
			if (!ld.Fits(this.loans))
				return false;

			try {
				var eel = new EmailEnlistedLottery(UserID, DB, Log);
				eel.Execute();
				return eel.Enlisted;
			} catch (Exception e) {
				Log.Warn(e, "Failed to enlist user {0} to lottery {1}.", UserID, ld.LotteryID);
				return false;
			} // try
		} // Enlist

		private bool IsBroker {
			get { return this.brokerID > 0; }
		} // IsBroker

		private int UserID {
			get { return IsBroker ? this.brokerID : this.customerID; }
		} // UserID

		private enum RowType {
			MetaData,
			Lottery,
			Loan,
		} // enum RowType

		private readonly int customerID;
		private readonly List<LoanData> loans; 

		private bool isFilled;
		private int brokerID;
		private readonly List<LotteryDataForEnlisting> lotteries;
	} // class EnlistLottery
} // namespace

