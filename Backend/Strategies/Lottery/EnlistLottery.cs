namespace Ezbob.Backend.Strategies.Lottery {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Database;

	public class EnlistLottery : AStrategy {
		public EnlistLottery(int customerID) {
			this.customerID = customerID;
			this.metaData = new MetaData();
			this.lotteries = new List<LotteryDataForEnlisting>();
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
						sr.Fill(this.metaData);
						this.metaData.IsFilled = true;
						break;

					case RowType.Lottery:
						this.lotteries.Add(sr.Fill<LotteryDataForEnlisting>());
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
				"Customer {0} meta data: {1}; lotteries count: {2}.",
				this.customerID,
				this.metaData,
				this.lotteries.Count
			);

			if (!this.metaData.IsFilled || (this.lotteries.Count < 1)) {
				Log.Debug("Exiting (either no lotteries or no meta data).");
				return;
			} // if

			foreach (LotteryDataForEnlisting ld in this.lotteries)
				Enlist(ld);
		} // Execute

		private void Enlist(LotteryDataForEnlisting ld) {
			if (!ld.Fits(this.metaData.LoanCount, this.metaData.LoanAmount))
				return;

			try {
				new EmailEnlistedLottery(UserID, Guid.NewGuid(), ld.LotteryID, IsBroker).Execute();
			} catch (Exception e) {
				Log.Warn("Failed to enlist user {0} to lottery {1}.", UserID, ld.LotteryID);
			} // try
		} // Enlist

		private bool IsBroker {
			get { return this.metaData.BrokerID > 0; }
		} // IsBroker

		private int UserID {
			get { return this.IsBroker ? this.metaData.BrokerID : this.customerID; }
		} // UserID

		private enum RowType {
			MetaData,
			Lottery,
		} // enum RowType

		private readonly int customerID;

		private class MetaData {
			public MetaData() {
				IsFilled = false;
			} // constructor

			public bool IsFilled { get; set; }
			public int BrokerID { get; set; }
			public int LoanCount { get; set; }
			public decimal LoanAmount { get; set; }

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format(
					"is filled: {0}, broker id: {1}, loan count: {2}, amount: {3}",
					IsFilled,
					BrokerID,
					LoanCount,
					LoanAmount
				);
			} // ToString
		} // class MetaData

		private readonly MetaData metaData;
		private readonly List<LotteryDataForEnlisting> lotteries;
	} // class EnlistLottery
} // namespace

