namespace Ezbob.Backend.Strategies.LogicalGlue {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using EZBob.DatabaseLib.Model.Database;

	public class BackfillLogicalGlueForAll : AStrategy {
		public BackfillLogicalGlueForAll() {
			this.engine = InjectorStub.GetEngine();
		} // constructor

		public override string Name {
			get { return "BackfillLogicalGlueForAll"; }
		} // Name

		public override void Execute() {
			this.loanParamsList = DB.Fill<LoanParams>("LoadMinMaxLoanParams", CommandSpecies.StoredProcedure);

			this.pc = new ProgressCounter("{0} customers processed.", Log, 10, Severity.Msg);

			var sp = new LoadLogicalGlueLessCustomers(DB, Log);

			sp.ForEachResult<LoadLogicalGlueLessCustomers.ResultRow>(ProcessCustomer);

			this.pc.Log();
		} // Execute

		private ActionResult ProcessCustomer(LoadLogicalGlueLessCustomers.ResultRow row) {
			try {
				var lp = this.loanParamsList.FirstOrDefault(x => x.OriginID == row.OriginID);

				if (lp == null) {
					Log.Fatal("Could not find min and max loan parameters for customer origin {0}.", row.OriginID);
					return ActionResult.SkipAll;
				} // if

				decimal loanAmount = lp.Enrange(row.Amount ?? this.defaultAmounts[row.OriginID]);

				int term = lp.Enrange(row.Term ?? this.defaultTerms[row.OriginID]);

				this.engine.GetInference(row.CustomerID, loanAmount / term, false, GetInferenceMode.ForceDownload);

				this.pc.Next();
			} catch (Exception e) {
				Log.Alert(e, "Failed to load LG result for customer {0}.", row.CustomerID);
			} // try

			return ActionResult.Continue;
		} // ProcessCustomer

		private ProgressCounter pc;

		private readonly IEngine engine;

		private readonly SortedDictionary<int, decimal> defaultAmounts = new SortedDictionary<int, decimal> {
			{ (int)CustomerOriginEnum.ezbob, 20000 },
			{ (int)CustomerOriginEnum.everline, 24000 },
			{ (int)CustomerOriginEnum.alibaba, 20000 },
		};

		private readonly SortedDictionary<int, int> defaultTerms = new SortedDictionary<int, int> {
			{ (int)CustomerOriginEnum.ezbob, 9 },
			{ (int)CustomerOriginEnum.everline, 24 },
			{ (int)CustomerOriginEnum.alibaba, 12 },
		};

		private List<LoanParams> loanParamsList;

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class LoanParams {
			public int OriginID { get; set; }
			public decimal MinLoanAmount { get; set; }
			public decimal MaxLoanAmount { get; set; }
			public int MinTerm { get; set; }
			public int MaxTerm { get; set; }

			public decimal Enrange(decimal amount) {
				if (amount < MinLoanAmount)
					return MinLoanAmount;

				return amount > MaxLoanAmount ? MaxLoanAmount : amount;
			} // Enrange

			public int Enrange(int term) {
				if (term < MinTerm)
					return MinTerm;

				return term > MaxTerm ? MaxTerm : term;
			} // Enrange
		} // class LoanParams

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class LoadLogicalGlueLessCustomers : AStoredProcedure {
			public LoadLogicalGlueLessCustomers(AConnection db, ASafeLog log) : base(db, log) {}

			public override bool HasValidParameters() {
				return true;
			} // HasValidParameters

			public class ResultRow : AResultRow {
				public int CustomerID { get; set; }
				public int OriginID { get; set; }
				public decimal? Amount { get; set; }
				public int? Term { get; set; }
			} // class ResultRow
		} // class LoadLogicalGlueLessCustomers
	} // class BackfillLogicalGlueForAll
} // namespace

