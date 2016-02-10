namespace PaymentServices.Calculators {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using EzServiceAccessor;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using log4net;
	using NHibernate;
	using NHibernate.Linq;
	using StructureMap;

	public class LoanPaymentFacade {
		public LoanPaymentFacade() {
			this.loanTransactionMethodRepository = ObjectFactory.GetInstance<LoanTransactionMethodRepository>();
			this._historyRepository = ObjectFactory.GetInstance<LoanHistoryRepository>();
			this.amountToChargeFrom = CurrentValues.Instance.AmountToChargeFrom;
			serviceInstance = ObjectFactory.GetInstance<IEzServiceAccessor>();
			this.session = ObjectFactory.GetInstance<ISession>();
		} // constructor

		public LoanPaymentFacade(
			ILoanHistoryRepository historyRepository,
			ILoanTransactionMethodRepository loanTransactionMethodRepository,
			int? amountToChargeFrom = null
		) {
			this._historyRepository = historyRepository;
			this.loanTransactionMethodRepository = loanTransactionMethodRepository;
			this.amountToChargeFrom = amountToChargeFrom ?? CurrentValues.Instance.AmountToChargeFrom;
		} // constructor

		/// <summary>
		/// Заплатить за кредит. Платёж может быть произвольный. Early, On time, Late.
		/// Perform loan payment. Payment can be manual. Early, On time, Late.
		/// </summary>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public virtual decimal PayLoan(
			Loan loan,
			string transId,
			decimal amount,
			string ip,
			DateTime? term = null,
			string description = "payment from customer",
			bool interestOnly = false,
			string sManualPaymentMethod = null,
			NL_Payments nlPayment = null) {

			int customerID = loan.Customer.Id;

			var paymentTime = term ?? DateTime.UtcNow;

			var oldLoan = loan.Clone();

			const string Manual = "--- manual ---";
			string otherMethod = transId == Manual ? "Manual" : "Auto";

			var loanTransactionMethod = this.loanTransactionMethodRepository.FindOrDefault(sManualPaymentMethod, otherMethod);
			var transaction = this.session.BeginTransaction();

			var transactionItem = new PaypointTransaction {
				Amount = amount,
				Description = description,
				PostDate = paymentTime,
				Status = LoanTransactionStatus.Done,
				PaypointId = transId,
				IP = ip,
				LoanRepayment = oldLoan.Principal - loan.Principal,
				Interest = loan.InterestPaid - oldLoan.InterestPaid,
				InterestOnly = interestOnly,
				LoanTransactionMethod = loanTransactionMethod
			};

			try {
				loan.AddTransaction(transactionItem);

				List<InstallmentDelta> deltas = loan.Schedule.Select(inst => new InstallmentDelta(inst))
					.ToList();

				var calculator = new LoanRepaymentScheduleCalculator(loan, paymentTime, this.amountToChargeFrom);
				calculator.RecalculateSchedule();

				if (this._historyRepository != null) {
					var historyRecord = new LoanHistory(loan, paymentTime);
					this._historyRepository.SaveOrUpdate(historyRecord);
				} // if

				loan.UpdateStatus(paymentTime);

				if (loan.Customer != null)
					loan.Customer.UpdateCreditResultStatus();
				if (loan.Id > 0) {
					foreach (InstallmentDelta dlt in deltas) {
						dlt.SetEndValues();

						if (dlt.IsZero)
							continue;

						loan.ScheduleTransactions.Add(new LoanScheduleTransaction {
							Date = DateTime.UtcNow,
							FeesDelta = dlt.Fees.EndValue - dlt.Fees.StartValue,
							InterestDelta = dlt.Interest.EndValue - dlt.Interest.StartValue,
							Loan = loan,
							PrincipalDelta = dlt.Principal.EndValue - dlt.Principal.StartValue,
							Schedule = dlt.Installment,
							StatusAfter = dlt.Status.EndValue,
							StatusBefore = dlt.Status.StartValue,
							Transaction = transactionItem
						});
					} // for each delta
				} // if


				if (nlPayment != null) {

					Log.InfoFormat("PayLoan: oldLoanID: {0} customer: {1} nlpayment {2}", loan.Id, customerID, nlPayment);

					// override those for backword compatibility
					nlPayment.PaymentMethodID = loanTransactionMethod.Id;
					nlPayment.Notes = description;
					nlPayment.CreationTime = DateTime.UtcNow;
					nlPayment.PaymentTime = paymentTime;
					nlPayment.Amount = amount;
					nlPayment.PaymentStatusID = (int)NLPaymentStatuses.Active;

					Log.InfoFormat("PayLoan: overriden nlpayment {0}", nlPayment);

					long nlLoanId = serviceInstance.GetLoanByOldID(loan.Id, customerID);

					if (nlLoanId == 0) {
						Log.InfoFormat("Failed to find nl loan for oldLoanID {0}, customer {1}", loan.Id, customerID);
					} else {

						nlPayment.LoanID = nlLoanId;

						// use argument's nlPayment data: CreatedByUserID 

						if (nlPayment.PaymentSystemType == NLPaymentSystemTypes.Paypoint) {

							// workaround - from MakeAutomaticPayment sent transactionid with timestamp concated

							var card = loan.Customer.PayPointCards.FirstOrDefault(x => transId.StartsWith(x.TransactionId));

							if (card == null) {
								Log.ErrorFormat("PayPointCard for customer {0}, transId={1}, oldLoanID={2}, nl loanID={3} not found. Can't to write NL_PaypointTransactions for nl payment\n {4}{5}",
									customerID, transId, loan.Id, nlPayment.LoanID, AStringable.PrintHeadersLine(typeof(NL_Payments)), nlPayment.ToStringAsTable());
							} else {
								nlPayment.PaypointTransactions.Clear();
								nlPayment.PaypointTransactions.Add(new NL_PaypointTransactions() {
									TransactionTime = paymentTime,
									Amount = amount,
									Notes = description,
									PaypointTransactionStatusID = (int)NLPaypointTransactionStatuses.Done,
									PaypointUniqueID = transId,
									PaypointCardID = card.Id,
									IP = ip
								});
							}
						}

						serviceInstance.AddPayment(customerID, nlPayment, nlPayment.CreatedByUserID);
					}
				}
				transaction.Commit();
			} catch (Exception ex) {
				Log.ErrorFormat("Failed to pay {1} pounds for loan {0}, rollbacking \n {2}", loan.Id, amount, ex);
				transaction.Rollback();
			}

			Log.InfoFormat("LinkPaymentToInvestor {0} {1} {2} {3} {4} begin", transactionItem.Id, loan.Id, loan.Customer.Id, amount, paymentTime);
			serviceInstance.LinkPaymentToInvestor(1, transactionItem.Id, loan.Id, loan.Customer.Id, amount, paymentTime); // modified by elinar at 9/02/2016 EZ-4678 bugfix

			return amount;
		} // PayLoan

		/// <summary>
		/// Сколько будет сэкономлено денег, если пользователь погасит все свои кредиты.
		/// How much money will be saved, if customer pay all his loans now
		/// </summary>
		/// <param name="customer"></param>
		/// <param name="term"></param>
		/// <returns></returns>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public decimal CalculateSavings(Customer customer, DateTime? term = null) {
			if (term == null)
				term = DateTime.UtcNow;

			decimal totalToPay = customer.TotalEarlyPayment();

			decimal oldInterest = customer.Loans.Sum(l => l.Interest);

			Customer clonedCustomer = new Customer();
			clonedCustomer.Loans.AddAll(customer.Loans.Select(l => l.Clone()).ToList());
			clonedCustomer.Id = customer.Id;
			PayAllLoansForCustomer(clonedCustomer, totalToPay, "", term);

			decimal newInterest = clonedCustomer.Loans.Sum(l => l.Interest);

			decimal savedSetupFee = 0;
			string setupFeeCharge = new ConfigurationVariable(CurrentValues.Instance.SpreadSetupFeeCharge).Name;

			clonedCustomer.Loans.ForEach(l => l.Charges
				.Where(c => (c.ChargesType.Name == setupFeeCharge) && (c.State == "Active"))
				.ForEach(c => savedSetupFee += c.Amount - c.AmountPaid)
			);

			return oldInterest - newInterest + savedSetupFee;
		} // CalculateSavings

		/// <summary>
		/// Оплатить все кредиты клиента.
		/// </summary>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public void PayAllLoansForCustomer(
			Customer customer,
			decimal amount,
			string transId,
			DateTime? term = null,
			string description = null,
			string sManualPaymentMethod = null,
		NL_Payments nlPaymentCommomData = null) {

			var date = term ?? DateTime.Now;

			var loans = customer.Loans.Where(x => x.Status != LoanStatus.PaidOff || x.Id != 0).ToList();

			var nlLoansList = serviceInstance.GetCustomerLoans(customer.Id).ToList();

			foreach (var loan in loans) {

				if (amount <= 0)
					break;

				var money = Math.Min(amount, loan.TotalEarlyPayment(term));

				NL_Payments nlPayment = null;

				// customer's nl loans
				if (nlLoansList.Count > 0) {

					// current loan 
					var nlLoan = nlLoansList.FirstOrDefault(x => x.OldLoanID == loan.Id);

					if (nlLoan != null) {

						var nlModel = serviceInstance.GetLoanState(customer.Id, nlLoan.LoanID, DateTime.UtcNow, 1);

						Log.InfoFormat("<<< NL_Compare Loan: {0} NLModel: {1}.\n money={2}, nlModel.TotalEarlyPayment={3} >>>", loan, nlModel, money, nlModel.TotalEarlyPayment);

						nlPayment = new NL_Payments() {
							Amount = money,
							CreatedByUserID = 1,
							CreationTime = DateTime.UtcNow,
							LoanID = nlLoan.LoanID,
							PaymentTime = DateTime.UtcNow,
							Notes = "TotalEarlyPayment",
							PaymentStatusID = (int)NLPaymentStatuses.Active,
							PaymentSystemType = nlPaymentCommomData != null ? nlPaymentCommomData.PaymentSystemType : NLPaymentSystemTypes.None,
							PaymentMethodID = nlPaymentCommomData != null ? nlPaymentCommomData.PaymentMethodID : (int)NLLoanTransactionMethods.Manual // put better check
						};
					}
				}

				PayLoan(loan, transId, money, null, date, description, false, sManualPaymentMethod, nlPayment);

				amount = amount - money;

			} // for

		} // PayAllLoansForCustomer

		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="ArgumentNullException"><paramref /> or <paramref /> is null.</exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public void PayAllLateLoansForCustomer(
			Customer customer,
			decimal amount,
			string transId,
			DateTime? term = null,
			string description = null,
			string sManualPaymentMethod = null,
			NL_Payments nlPaymentCommomData = null) {

			DateTime date = term ?? DateTime.Now;

			IEnumerable<Loan> loans = customer.ActiveLoans.Where(l => l.Status == LoanStatus.Late);

			var nlLoansList = serviceInstance.GetCustomerLoans(customer.Id).ToList();

			if (nlLoansList.Count > 0) {
				nlLoansList.ForEach(l => Log.InfoFormat("PayAllLateLoansForCustomer NLLoanID={0}", l.LoanID));
			}

			foreach (var loan in loans) {

				if (amount <= 0)
					break;

				LoanRepaymentScheduleCalculator c = new LoanRepaymentScheduleCalculator(loan, term, this.amountToChargeFrom);
				LoanScheduleItem state = c.GetState();

				decimal late = loan.Schedule.Where(s => s.Status == LoanScheduleStatus.Late).Sum(s => s.LoanRepayment) + state.Interest + state.Fees + state.LateCharges;

				decimal money = Math.Min(amount, late);

				NL_Payments nlPayment = null;

				// customer's nl loans
				if (nlLoansList.Count > 0) {

					// current loan 
					var nlLoan = nlLoansList.FirstOrDefault(x => x.OldLoanID == loan.Id);

					if (nlLoan != null) {

						var nlModel = serviceInstance.GetLoanState(loan.Customer.Id, nlLoan.LoanID, DateTime.UtcNow, 1);

						decimal nlLate = nlModel.Interest + nlModel.Fees;
						nlModel.Loan.Histories.ForEach(h => h.Schedule.Where(s => s.LoanScheduleStatusID == (int)NLScheduleStatuses.Late).Sum(s => nlLate += s.Principal));
						decimal nlMoney = Math.Min(amount, nlLate);

						Log.InfoFormat("<<< NL_Compare: Loan:{0} NLModel:{1}.\n late={2}, nlLate={3}, amount={4}, money={5}, nlMoney={6} >>>", loan, nlModel, late, nlLate, amount, money, nlMoney);

						nlPayment = new NL_Payments() {
							Amount = money,
							CreatedByUserID = 1,
							CreationTime = DateTime.UtcNow,
							LoanID = nlLoan.LoanID,
							PaymentTime = DateTime.UtcNow,
							Notes = "PayAllLateLoansForCustomer",
							//PaymentMethodID = (int)NLLoanTransactionMethods.CustomerAuto,
							PaymentStatusID = (int)NLPaymentStatuses.Active,
							PaymentSystemType = nlPaymentCommomData != null ? nlPaymentCommomData.PaymentSystemType : NLPaymentSystemTypes.None,
							PaymentMethodID = nlPaymentCommomData != null ? nlPaymentCommomData.PaymentMethodID : (int)NLLoanTransactionMethods.Manual // put better check
						};
					}
				}

				PayLoan(loan, transId, money, null, date, description, false, sManualPaymentMethod, nlPayment);

				amount = amount - money;
			} // for
		} // PayAllLateLoansForCustomer

		/// <summary>
		/// Main method for making payments
		/// </summary>
		/// <param name="transId">pay point transaction id</param>
		/// <param name="amount"></param>
		/// <param name="ip"></param>
		/// <param name="type"></param>
		/// <param name="loanId"></param>
		/// <param name="customer"></param>
		/// <param name="date">payment date</param>
		/// <param name="description"></param>
		/// <param name="paymentType">If payment type is null - ordinary payment(reduces principal),
		/// if nextInterest then it is for Interest Only loans, and reduces interest in the future.</param>
		/// <param name="sManualPaymentMethod"></param>
		/// <param name="nlPayment"></param>
		/// <returns></returns>
		/// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public PaymentResult MakePayment(
			string transId,
			decimal amount,
			string ip,
			string type,
			int loanId,
			Customer customer,
			DateTime? date = null,
			string description = "payment from customer",
			string paymentType = null,
			string sManualPaymentMethod = null,
			NL_Payments nlPayment = null) {

			Log.DebugFormat(
				"MakePayment transId: {0}, amount: {1}, ip: {2}, type: {3}, loanId: {4}, customer: {5}," +
				"date: {6}, description: {7}, paymentType: {8}, manualPaymentMethod: {9}, nlPayment: {10}",
				transId,
				amount,
				ip,
				type,
				loanId,
				customer.Id,
				date,
				description,
				paymentType,
				sManualPaymentMethod,
				nlPayment
			);

			decimal oldInterest;
			decimal newInterest;
			bool rolloverWasPaid = false;
			description = string.Format("{0} {1} {2}", description, type, paymentType);

			SortedSet<int> openLoansBefore = new SortedSet<int>(
				customer.Loans.Where(l => l.Status != LoanStatus.PaidOff).Select(l => l.Id)
			);

			if (type == "total") {
				oldInterest = customer.Loans.Sum(l => l.Interest);

				rolloverWasPaid = (
					from l in customer.Loans
					from s in l.Schedule
					from r in s.Rollovers
					where r.Status == RolloverStatus.New
					select r
				).Any();

				PayAllLoansForCustomer(
					customer,
					amount,
					transId,
					date,
					description,
					sManualPaymentMethod
					, nlPayment
				);

				newInterest = customer.Loans.Sum(l => l.Interest);

			} else if (type == "totalLate") {
				rolloverWasPaid = (
					from l in customer.Loans
					from s in l.Schedule
					from r in s.Rollovers
					where r.Status == RolloverStatus.New
					select r
				).Any();

				oldInterest = customer.Loans.Sum(l => l.Interest);

				PayAllLateLoansForCustomer(
					customer,
					amount,
					transId,
					date,
					description,
					sManualPaymentMethod
				);

				newInterest = customer.Loans.Sum(l => l.Interest);

			} else if (paymentType == "nextInterest") {
				oldInterest = 0;
				var loan = customer.GetLoan(loanId);

				if (nlPayment != null) {
					var nlLoanId = serviceInstance.GetLoanByOldID(loanId, customer.Id);
					if (nlLoanId > 0) {
						nlPayment.Amount = amount;
						nlPayment.CreationTime = DateTime.UtcNow;
						nlPayment.LoanID = nlLoanId;
						nlPayment.PaymentTime = DateTime.UtcNow;
						nlPayment.Notes = description;
						nlPayment.PaymentStatusID = (int)NLPaymentStatuses.Active;
						//CreatedByUserID = userId,
						//PaymentMethodID = (int)NLLoanTransactionMethods.CustomerAuto,
						//PaymentSystemType = (transId == PaypointTransaction.Manual ? NLPaymentSystemTypes.None : NLPaymentSystemTypes.Paypoint)
					}
				}

				PayLoan(loan, transId, amount, ip, date, description, true, sManualPaymentMethod, nlPayment);
				newInterest = 0;
			} else {
				Loan loan = customer.GetLoan(loanId);

				oldInterest = loan.Interest;

				var rollover = (
					from s in loan.Schedule
					from r in s.Rollovers
					where r.Status == RolloverStatus.New
					select r
				).FirstOrDefault();

				long nlLoanId = 0;
				if (nlPayment != null) {
					nlLoanId = serviceInstance.GetLoanByOldID(loanId, customer.Id);
					if (nlLoanId > 0) {
						nlPayment.Amount = amount;
						nlPayment.CreationTime = DateTime.UtcNow;
						nlPayment.LoanID = nlLoanId;
						nlPayment.PaymentTime = DateTime.UtcNow;
						nlPayment.Notes = description;
						nlPayment.PaymentStatusID = (int)NLPaymentStatuses.Active;
						nlPayment.PaymentDestination = rollover != null ? NLPaymentDestinations.Rollover.ToString() : null;
						//CreatedByUserID = userId,
						//PaymentMethodID = (int)NLLoanTransactionMethods.CustomerAuto,
						//PaymentSystemType = (transId == PaypointTransaction.Manual ? NLPaymentSystemTypes.None : NLPaymentSystemTypes.Paypoint)
					}
				}

				PayLoan(loan, transId, amount, ip, date, description, false, sManualPaymentMethod, nlPayment);

				newInterest = loan.Interest;

				rolloverWasPaid = rollover != null && rollover.Status == RolloverStatus.Paid;

				if (rolloverWasPaid && nlLoanId > 0) {
					serviceInstance.AcceptRollover(customer.Id, nlLoanId);
				}

			} // if

			SortedSet<int> openLoansAfter = new SortedSet<int>(
				customer.Loans.Where(l => l.Status != LoanStatus.PaidOff).Select(l => l.Id)
			);

			var loansClosedNow = new SortedSet<int>(openLoansBefore.Except(openLoansAfter));

			decimal savedSetupFee = 0;
			string setupFeeCharge = new ConfigurationVariable(CurrentValues.Instance.SpreadSetupFeeCharge).Name;

			customer.Loans.Where(l => loansClosedNow.Contains(l.Id)).ForEach(l => l.Charges
				.Where(c => (c.ChargesType.Name == setupFeeCharge) && (c.State == "Active"))
				.ForEach(c => savedSetupFee += c.Amount - c.AmountPaid)
			);

			var savedPounds = oldInterest - newInterest + savedSetupFee;

			var transactionRefNumbers =
				from l in customer.Loans
				from t in l.TransactionsWithPaypoint
				where t.Id == 0
				select t.RefNumber;

			var payFastModel = new PaymentResult {
				PaymentAmount = amount,
				Saved = oldInterest + savedSetupFee > 0 ? Math.Round(savedPounds / (oldInterest + savedSetupFee) * 100) : 0,
				SavedPounds = savedPounds,
				TransactionRefNumbers = transactionRefNumbers.ToList(),
				RolloverWasPaid = rolloverWasPaid
			};

			customer.TotalPrincipalRepaid = customer.Loans
				.SelectMany(l => l.Transactions)
				.OfType<PaypointTransaction>()
				.Where(l => l.Status != LoanTransactionStatus.Error)
				.Sum(l => l.LoanRepayment);

			return payFastModel;
		} // MakePayment

		public LoanScheduleItem GetStateAt(Loan loan, DateTime dateTime) {
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, dateTime, this.amountToChargeFrom);
			var result = payEarlyCalc.GetState();

			try {
				long nlLoanId = serviceInstance.GetLoanByOldID(loan.Id, loan.Customer.Id);
				if (nlLoanId > 0) {
					var nlModel = serviceInstance.GetLoanState(loan.Customer.Id, nlLoanId, dateTime, 1);
					Log.InfoFormat("<<<GetStateAt NL_Compare at : {0} ;  nlModel : {1} loan: {2} >>>", System.Environment.StackTrace, nlModel, loan);
				} else {
					Log.InfoFormat("<<<GetStateAt NL loan for oldid {0} not found >>>", loan.Id);
				}
			} catch (Exception) {
				Log.InfoFormat("<<<GetStateAt NL_Compare Fail at : {0}", System.Environment.StackTrace);
			}

			return result;
		} // GetStateAt

		public void Recalculate(Loan loan, DateTime dateTime) {
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, dateTime, this.amountToChargeFrom);
			payEarlyCalc.GetState();

			try {
				long nlLoanId = serviceInstance.GetLoanByOldID(loan.Id, loan.Customer.Id);
				if (nlLoanId > 0) {
					var nlModel = serviceInstance.GetLoanState(loan.Customer.Id, nlLoanId, dateTime, 1);
					Log.InfoFormat("<<<Recalculate NL_Compare {0}\n  'old' loan: {1} >>>", nlModel, loan);
				} else {
					Log.InfoFormat("<<<Recalculate NL loan for oldid {0} not found >>>", loan.Id);
				}
			} catch (Exception) {
				Log.InfoFormat("<<< NL_Compare Fail at: {0}", System.Environment.StackTrace);
			}

		} // Recalculate

		private readonly ILoanHistoryRepository _historyRepository;
		private static readonly ILog Log = LogManager.GetLogger(typeof(LoanPaymentFacade));
		private readonly ILoanTransactionMethodRepository loanTransactionMethodRepository;
		private readonly int amountToChargeFrom;
		private ISession session;

		public IEzServiceAccessor serviceInstance { get; private set; }
	} // class LoanPaymentFacade
} // namespace
