namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using NHibernate.Linq;

	public class AddOffer : AStrategy {
		public AddOffer(NL_Offers offer, IEnumerable<NL_OfferFees> fees = null) {
			this.offer = offer;
			this.fees = (fees != null) ? fees.Where(f => f.Percent > 0) : null;
		}//constructor

		public override string Name { get { return "AddOffer"; } }

		public long OfferID { get; set; }
		public string Error { get; private set; }

		private readonly NL_Offers offer;
		private readonly IEnumerable<NL_OfferFees> fees;

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", this.offer, null, null, null);

			if (this.offer.DecisionID == 0) {
				Error = "input DecisionID is 0";
				Log.Info(Error);
				NL_AddLog(LogType.Info, "Strategy end", this.offer, null, Error, null);
				return;
			}

			Log.Debug("{0}", this.offer);

			try {
				OfferID = DB.ExecuteScalar<long>("NL_OffersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Offers>("Tbl", this.offer));

				Log.Debug("NL_Offer saved: {0}", OfferID);

				if (this.fees != null && this.fees.Any()) {
					this.fees.ForEach(f => f.OfferID = OfferID);
					this.fees.ForEach(f => Log.Debug("{0}", f));
					DB.ExecuteNonQuery("NL_OfferFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_OfferFees>("Tbl", this.fees));
				}

				NL_AddLog(LogType.Info, "Strategy End", this.offer, OfferID, null, null);

			} catch (DbException dbException) {

				if (dbException.Message.Contains("Violation of UNIQUE KEY constraint"))
					NL_AddLog(LogType.Info, "Strategy End", this.offer, OfferID, null, null);
				else
					LogAndExit(dbException);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				LogAndExit(ex);
			}
		}


		private void LogAndExit(Exception ex) {
			Log.Alert("Failed to add NL Offer or OfferFee. Rolling back. {0} ", ex);
			Error = string.Format("Failed to add NL Offer|OfferFees, err: {0} ", ex.Message);
			NL_AddLog(LogType.Error, "Strategy Faild", this.offer, null, ex.ToString(), ex.StackTrace);
		}


	}
}