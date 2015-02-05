namespace Ezbob.Backend.Strategies.Lottery {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Database;

	public class EnlistLottery : AStrategy {
		public EnlistLottery(int customerID) {
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

					string rowType = sr["RowType"];

					if (!Enum.TryParse(rowType, true, out rt)) {
						Log.Warn("Failed to parse row type {0} received from FindRelevantLotteries.", rowType);
						return;
					} // if

					switch (rt) {
					case RowType.MetaData:
						Log.Debug("FindRelevantLotteries: meta data has been loaded.");
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

			Log.Debug("Traversing available lotteries...");

			foreach (LotteryDataForEnlisting ld in this.lotteries) {
				Log.Debug("Checking lottery {0}...", ld.LotteryID);

				if (Enlist(ld)) {
					Log.Debug("Enlisted to lottery {0}.", ld.LotteryID);
					break;
				} // if

				Log.Debug("Not enlisted to lottery {0}.", ld.LotteryID);
			} // for each

			Log.Debug("Traversing available lotteries complete.");
		} // Execute

		private bool Enlist(LotteryDataForEnlisting ld) {
			Log.Debug("Enlist(lottery {0}) started...", ld.LotteryID);

			if (!ld.Fits(this.loans)) {
				Log.Debug("Enlist(lottery {0}) complete: ain't no fits.", ld.LotteryID);
				return false;
			} // if

			try {
				var eel = new EmailEnlistedLottery(UserID, Guid.NewGuid(), ld.LotteryID, IsBroker);
				eel.Execute();

				Log.Debug("Enlist(lottery {0}) complete: {1}fits.", ld.LotteryID, eel.Enlisted ? string.Empty : "ain't no ");

				return eel.Enlisted;
			} catch (Exception e) {
				Log.Warn(e, "Failed to enlist user {0} to lottery {1}.", UserID, ld.LotteryID);
				Log.Debug("Enlist(lottery {0}) complete: ain't no fits.", ld.LotteryID);
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

