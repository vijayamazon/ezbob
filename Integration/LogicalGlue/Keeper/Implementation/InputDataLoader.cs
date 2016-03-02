namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Exceptions.Keeper;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;
	using Newtonsoft.Json;

	class InputDataLoader : ATimedCustomerActionBase {
		public InputDataLoader(
			AConnection db,
			ASafeLog log,
			int customerID,
			DateTime now,
			bool loadMonthlyRepaymentOnly,
			MonthlyPaymentMode monthlyPaymentMode
		) : base(db, log, customerID, now) {
			Result = new InferenceInput();
			this.loadMonthlyRepaymentOnly = loadMonthlyRepaymentOnly;
			this.monthlyPaymentMode = monthlyPaymentMode;
		} // constructor

		public InputDataLoader Execute() {
			if (Executed) {
				Log.Alert("Input data loader({0}, '{1}') has already been executed.", CustomerID, NowStr);
				return this;
			} // if

			Executed = true;

			Log.Debug("Executing input data loader({0}, '{1}')...", CustomerID, NowStr);

			this.openLoanPayments = 0;

			new LoadInputData(DB, Log) {
				CustomerID = CustomerID,
				Now = Now,
				MonthlyRepaymentOnly = this.loadMonthlyRepaymentOnly,
			}.ForEachRowSafe(ProcessInputDataRow);

			Result.MonthlyPayment = (Result.MonthlyPayment ?? 0) +
				(this.monthlyPaymentMode.WithOpen() ? this.openLoanPayments : 0);

			Log.Debug("Executing input data loader({0}, '{1}') complete.", CustomerID, NowStr);

			return this;
		} // Execute

		public InferenceInput Result { get; private set; }
		public int CompanyID { get; private set; }

		private void ProcessInputDataRow(SafeReader sr) {
			string rowTypeName = sr["RowType"];

			RowTypes rowType;

			if (!Enum.TryParse(rowTypeName, false, out rowType)) {
				throw new KeeperAlert(
					Log,
					"Input data loader({1}, '{2}'): unknown row type '{0}'.",
					rowTypeName,
					CustomerID,
					NowStr
				);
			} // if

			switch (rowType) {
			case RowTypes.CompanyRegistrationNumber:
				Result.CompanyRegistrationNumber = sr["CompanyNumber"];
				break;

			case RowTypes.Address:
				Result.Director.SetAddress(sr["Postcode"], sr["Line1"], sr["Line2"], sr["Line3"]);
				break;

			case RowTypes.RequestedLoan:
				ProcessRequestedLoan(sr);
				break;

			case RowTypes.DirectorData:
				Result.Director.FirstName = sr["FirstName"];
				Result.Director.LastName = sr["Surname"];
				Result.Director.DateOfBirth = sr["DateOfBirth"];

				CompanyID = sr["CompanyID"];
				break;

			case RowTypes.EquifaxData:
				string rawResponse = sr["ResponseData"];

				Reply reply = string.IsNullOrWhiteSpace(rawResponse) ? null : JsonConvert.DeserializeObject<Reply>(
					Encrypted.Decrypt(rawResponse)
				);

				// ReSharper disable once PossibleNullReferenceException
				// Null check is done inside this --+
				//                                  v
				Result.EquifaxData = reply.HasEquifaxData() ? reply.Equifax.RawResponse : null;
				break;

			case RowTypes.OpenLoan:
				ProcessOpenLoan(sr);
				break;

			default:
				throw OutOfRangeException(
					"Input data loader({1}, '{2}'): unsupported row type '{0}'.",
					rowTypeName,
					CustomerID,
					NowStr
				);
			} // switch
		} // ProcessInputDataRow

		private void ProcessOpenLoan(SafeReader sr) {
			decimal loanAmount = sr["LoanAmount"];

			if (loanAmount <= 0)
				return;

			int loanID = sr["LoanID"];
			decimal interestRate = sr["InterestRate"];
			int term = sr["Term"];

			if (term <= 0)
				term = 12;

			decimal lastPayment = Math.Truncate(loanAmount / term);
			decimal firstPayment = loanAmount - lastPayment * (term - 1);

			decimal payment = Math.Max(firstPayment, lastPayment) +
				(this.monthlyPaymentMode.WithInterest() ? loanAmount * interestRate : 0);

			this.openLoanPayments += payment;

			Log.Debug(
				"Open loan payment from loan {0} is {1} = MAX({2}, {3}) + ({4} ? {5} * {6} : 0), term {7}.",
				loanID,
				payment.ToString("C2", enGB),
				firstPayment.ToString("C2", enGB),
				lastPayment.ToString("C2", enGB),
				this.monthlyPaymentMode.WithInterest(),
				loanAmount.ToString("C2", enGB),
				interestRate.ToString("P2", enGB),
				Grammar.Number(term, "month")
			);
		} // ProcessOpenLoan

		private void ProcessRequestedLoan(SafeReader sr) {
			decimal defaultAmount = sr["DefaultAmount"];
			if (defaultAmount <= 0)
				defaultAmount = 10000;

			int defaultTerm = sr["DefaultTerm"];
			if (defaultTerm <= 0)
				defaultTerm = 12;

			decimal amount = sr["Amount"];
			if (amount <= 0)
				amount = defaultAmount;

			int term = sr["Term"];
			term = term > 0 ? term : defaultTerm;

			decimal maxInterestRate = sr["MaxInterestRate"];
			if (maxInterestRate <= 0)
				maxInterestRate = 0.0225m;

			// Monthly repayment = principal + interest + fees.
			// Principal = amount / term.
			// Max interest = amount * interest rate.
			// TODO: define default fees.

			Result.RequestedAmount = amount;
			Result.RequestedTerm = term;

			Result.MonthlyPayment = amount / term + (this.monthlyPaymentMode.WithInterest() ? amount * maxInterestRate : 0);
		} // ProcessRequestedLoan

		[StringFormatMethod("format")]
		private KeeperAlert OutOfRangeException(string format, params object[] args) {
			return new KeeperAlert(Log, new ArgumentOutOfRangeException(), format, args);
		} // OutOfRangeException

		private enum RowTypes {
			CompanyRegistrationNumber,
			Address,
			OpenLoan,
			RequestedLoan,
			DirectorData,
			EquifaxData,
		} // enum RowType

		private decimal openLoanPayments;
		private readonly bool loadMonthlyRepaymentOnly;
		private readonly MonthlyPaymentMode monthlyPaymentMode;

		private static readonly CultureInfo enGB = new CultureInfo("en-GB", false);
	} // class InputDataLoader
} // namespace
