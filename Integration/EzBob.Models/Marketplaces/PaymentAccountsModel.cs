namespace EzBob.Models.Marketplaces {
	using System;
	using Ezbob.Database;
	using Ezbob.Utils;
	using EzBob.Models;

	public class ChannelGrabberPaymentAccountsModel : PaymentAccountsModel {
		protected override string LoadSpName { get { return "LoadPaymentAccountModelChaGra"; } }
	} // class ChannelGrabberPaymentAccountsModel

	public class FreeAgentPaymentAccountsModel : PaymentAccountsModel {
		protected override string LoadSpName { get { return "LoadPaymentAccountModelFreeAgent"; } }
	} // class FreeAgentPaymentAccountsModel

	public class PaymentAccountsModel : SimpleMarketPlaceModel {
		public virtual int id { get; set; }
		public virtual bool IsNew { get; set; }

		[Traversable]
		public virtual decimal MonthInPayments { get; set; }

		public virtual decimal MonthInPaymentsAnnualized {
			get {
				return MonthInPayments * 12;
			}
		}

		public virtual string Status { get; set; }

		[Traversable]
		public virtual decimal TotalNetInPayments { get; set; }
		[Traversable]
		public virtual decimal TotalNetOutPayments { get; set; }

		[Traversable]
		public virtual decimal TransactionsNumber { get; set; }

		public virtual void Init() {
			MonthInPayments = 0;
			TotalNetInPayments = 0;
			TotalNetOutPayments = 0;
			TransactionsNumber = 0;
		} // Init

		public virtual void Load(int mpID, DateTime? history, AConnection db) {
			if ((db == null) || string.IsNullOrWhiteSpace(LoadSpName))
				return;

			SafeReader sr = db.GetFirst(
				LoadSpName,
				CommandSpecies.StoredProcedure,
				new QueryParameter("MpID", mpID),
				new QueryParameter("Now", history ?? DateTime.UtcNow),
				new QueryParameter("ShowCurrent", history == null)
			);

			if (sr.IsEmpty)
				return;

			sr.Fill(this);
		} // Load

		protected virtual string LoadSpName { get { return null; } }
	} // class PaymentAccountsModel

	public class PayPalPaymentAccountsModel : PaymentAccountsModel {
		protected override string LoadSpName { get { return "LoadPaymentAccountModelPayPal"; } }
	} // class PayPalPaymentAccountsModel

	public class SagePaymentAccountsModel : PaymentAccountsModel {
		protected override string LoadSpName { get { return "LoadPaymentAccountModelSage"; } }
	} // class SagePaymentAccountsModel

	public class YodleePaymentAccountsModel : PaymentAccountsModel {
		protected override string LoadSpName { get { return "LoadPaymentAccountModelYodlee"; } }
	} // class YodleePaymentAccountsModel
} // namespace
