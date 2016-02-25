namespace EzBob.Models {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using NHibernate.Linq;

	public class ChangeLoanDetailsModelBuilder {
		public EditLoanDetailsModel BuildModel(Loan loan) {
			var model = new EditLoanDetailsModel {
				Amount = loan.LoanAmount,
				InterestRate = loan.InterestRate,
				SetupFee = loan.SetupFee,
				Date = loan.Date,
				Id = loan.Id,
				LoanType = loan.LoanType.Type
			};

			model.LoanStatus = Enum.GetName(typeof(LoanStatus), loan.Status);

			if (loan.CashRequest != null)
				model.CashRequestId = loan.CashRequest.Id;

			model.Items = new List<SchedultItemModel>();

			AddInstallments(loan.Schedule, model.Items);
			AddTransactions(loan.PacnetTransactions, model.Items);
			AddPaypointTransactions(loan.TransactionsWithPaypointSuccesefull, model.Items);
			AddFees(loan.Charges, model.Items);

			model.Items = model.Items.OrderBy(i => i.Date).ToList();

			DateTime now = DateTime.UtcNow;

			var loanFutureScheduleItems = loan.Schedule.Where(x => x.Date > now)
					 .OrderBy(x => x.Date)
					 .ToDictionary(pair => pair.Date.ToString("dd/MM/yyyy"), pair => pair.Id.ToString());

			model.LoanFutureScheduleItems = new Dictionary<string, string>();
			model.LoanFutureScheduleItems.Add("Immediately Stop...", "-1");

			for (int i = 0; i <= loanFutureScheduleItems.Count - 1; i++) {
				model.LoanFutureScheduleItems.Add(string.Format("{0} : {1}", i + 1, loanFutureScheduleItems.ElementAt(i)
					.Key), loanFutureScheduleItems.ElementAt(i)
						.Value);
			}

			model.SInterestFreeze = loan.InterestFreeze.OrderBy(f => f.StartDate).Select(f => f.ToString()).ToList();

			foreach (LoanInterestFreeze fr in loan.InterestFreeze.OrderBy(fr => fr.StartDate).Where(fr => fr.DeactivationDate == null)) {
				model.InterestFreeze.Add(new InterestFreezeModel {
					Id = fr.Id,
					StartDate = fr.StartDate,
					EndDate = fr.EndDate,
					InterestRate = fr.InterestRate,
					ActivationDate = fr.ActivationDate,
					DeactivationDate = fr.DeactivationDate
				});
			}

			return model;
		}

		private void AddInstallments(IEnumerable<LoanScheduleItem> schedule, List<SchedultItemModel> items) {
			items.AddRange(schedule.Select(InstallmentModel));
		}

		private void AddTransactions(IEnumerable<PacnetTransaction> schedule, List<SchedultItemModel> items) {
			items.AddRange(schedule.Select(PacnetTransactionModel));
		}

		private void AddPaypointTransactions(IEnumerable<PaypointTransaction> schedule, List<SchedultItemModel> items) {
			items.AddRange(schedule.Select(PaypointTransactionModel));
		}

		private void AddFees(IEnumerable<LoanCharge> schedule, List<SchedultItemModel> items) {
			items.AddRange(schedule.Select(FeeModel));
		}

		private static SchedultItemModel InstallmentModel(LoanScheduleItem item) {
			var model = new SchedultItemModel {
				Id = item.Id,
				Date = item.Date,
				Principal = item.LoanRepayment,
				Balance = item.Balance,
				BalanceBeforeRepayment = item.BalanceBeforeRepayment,
				Interest = item.Interest,
				InterestRate = item.InterestRate,
				Fees = item.Fees,
				Deletable = false,
				Editable = true,
				Editor = "Installment",
				Status = item.Status.ToString(),
				Total = item.AmountDue,
				Type = "Installment"
			};
			return model;
		}

		private static SchedultItemModel PacnetTransactionModel(PacnetTransaction item) {
			var model = new SchedultItemModel {
				Id = item.Id,
				Date = item.PostDate,
				Description = item.Description,
				Status = item.Status.ToString(),
				Editable = false,
				Total = item.Amount,
				Type = "Pacnet"
			};
			return model;
		}

		private static SchedultItemModel PaypointTransactionModel(PaypointTransaction item) {
			var model = new SchedultItemModel {
				Id = item.Id,
				Date = item.PostDate,
				Description = item.Description,
				Status = item.Status.ToString(),
				Editable = false,
				Deletable = (item.Amount != 0),
				Principal = item.LoanRepayment,
				Interest = item.Interest,
				Total = item.Amount,
				Type = "Paypoint"
			};
			return model;
		}

		private static SchedultItemModel FeeModel(LoanCharge item) {
			var model = new SchedultItemModel {
				Id = item.Id,
				Fees = item.Amount,
				Total = item.Amount,
				Date = item.Date,
				Status = item.State ?? "Active",
				Description = item.Description,
				Editable = true,
				Deletable = true,
				Type = "Fee",
				Editor = "Fee"
			};
			return model;
		}

		public Loan CreateLoan(EditLoanDetailsModel model) {
			var loan = new Loan {
				Id = model.Id,
				LoanAmount = model.Amount,
				InterestRate = model.InterestRate,
				SetupFee = model.SetupFee,
				Date = model.Date
			};

			foreach (var item in model.Items.Where(i => i.Type == "Installment").OrderBy(i => i.Date)) {
				var x = CreateInstallment(item);
				x.Loan = loan;
				loan.Schedule.Add(x);
			}

			foreach (var item in model.Items.Where(i => i.Type == "Pacnet").OrderBy(i => i.Date)) {
				var x = CreatePacnetTransaction(item);
				x.Loan = loan;
				loan.Transactions.Add(x);
			}

			foreach (var item in model.Items.Where(i => i.Type == "Paypoint").OrderBy(i => i.Date)) {
				var x = CreatePaypointTransaction(item);
				x.Loan = loan;
				loan.Transactions.Add(x);
			}

			foreach (var item in model.Items.Where(i => i.Type == "Fee").OrderBy(i => i.Date)) {
				var x = CreateFee(item);
				x.Loan = loan;
				loan.Charges.Add(x);
			}

			loan.InterestFreeze = model.InterestFreeze.Select(item => new LoanInterestFreeze() {
				StartDate = item.StartDate,
				EndDate = item.EndDate,
				ActivationDate = item.ActivationDate,
				DeactivationDate = item.DeactivationDate,
				InterestRate = item.InterestRate,
				Id = item.Id
			}).ToList();

			return loan;
		}

		private static LoanCharge CreateFee(SchedultItemModel item) {
			var charge =
				CurrentValues.Instance.OtherCharge ??
				CurrentValues.Instance.AdministrationCharge;

			return new LoanCharge {
				Id = item.Id,
				Amount = item.Fees,
				Date = item.Date,
				State = item.Status,
				ChargesType = new ConfigurationVariable(charge),
				Description = item.Description
			};
		}

		private static PaypointTransaction CreatePaypointTransaction(SchedultItemModel item) {
			return new PaypointTransaction {
				Id = item.Id,
				PostDate = item.Date,
				Description = item.Description,
				Status = (LoanTransactionStatus)Enum.Parse(typeof(LoanTransactionStatus), item.Status),
				LoanRepayment = item.Principal,
				Interest = item.Interest,
				Amount = item.Total,
			};
		}

		private static PacnetTransaction CreatePacnetTransaction(SchedultItemModel item) {
			return new PacnetTransaction {
				Id = item.Id,
				PostDate = item.Date,
				Description = item.Description,
				Status = (LoanTransactionStatus)Enum.Parse(typeof(LoanTransactionStatus), item.Status),
				Amount = item.Total,
			};
		}

		private LoanScheduleItem CreateInstallment(SchedultItemModel item) {
			return new LoanScheduleItem {
				Id = item.Id,
				Balance = item.Balance,
				LoanRepayment = item.Principal,
				Date = item.Date,
				Interest = item.Interest,
				AmountDue = item.Total,
				InterestRate = item.InterestRate,
				Fees = item.Fees
			};
		}

		// model - from UI, loan - from DB
		public void UpdateLoan(EditLoanDetailsModel model, Loan loan) {
			var actual = CreateLoan(model);

			UpdateInstallments(loan, actual);
			UpdateFees(loan, actual);
			// loan - from DB, actual - from UI
			RemovePayments(loan, actual);

			loan.Modified = true;
		}

		private static void UpdateInstallments(Loan loan, Loan actual) {
			for (int i = loan.Schedule.Count - 1; i >= 0; i--) {
				var item = loan.Schedule[i];
				//если в модели есть installment с таким id, то обновляем его
				if (actual.Schedule.Any(x => x.Id == item.Id)) {
					var installment = actual.Schedule.First(x => x.Id == item.Id);
					actual.Schedule.Remove(installment);
					item.Balance = installment.Balance;
					item.Date = installment.Date;
					item.InterestRate = installment.InterestRate;
					continue;
				}

				//иначе Installment был удален
				loan.Schedule.Remove(item);
			}
			foreach (var item in actual.Schedule) {
				loan.Schedule.Add(item);
				item.Loan = loan;
			}
		}

		private static void UpdateFees(Loan loan, Loan actual) {
			for (int i = loan.Charges.Count - 1; i >= 0; i--) {
				var item = loan.Charges[i];
				//если в модели есть fee с таким id, то обновляем его
				if (actual.Charges.Any(x => x.Id == item.Id)) {
					var fee = actual.Charges.Single(x => x.Id == item.Id);
					actual.Charges.Remove(fee);
					item.Amount = fee.Amount;
					item.Date = fee.Date;
					item.Description = fee.Description;
					continue;
				}

				//иначе Installment был удален
				loan.Charges.Remove(item);
			}

			foreach (var item in actual.Charges) {
				loan.Charges.Add(item);
				item.Loan = loan;
			}
		}

		// loan - from DB, actual - from UI
		private static void RemovePayments(Loan loan, Loan actual) {

			Trace.WriteLine("DB state:");
			loan.TransactionsWithPaypoint.ForEach(xxx => Trace.WriteLine(xxx));

			Trace.WriteLine("\n\n from UI:");
			actual.TransactionsWithPaypoint.ForEach(xxx => Trace.WriteLine(xxx));

			IEnumerable<PaypointTransaction> removedPayments = loan.TransactionsWithPaypoint.Except(actual.TransactionsWithPaypoint);
			var paypointTransactions = removedPayments as IList<PaypointTransaction> ?? removedPayments.ToList();

			paypointTransactions.ForEach(t => Trace.WriteLine(string.Format("Removed payment: {0}", t)));

			foreach (PaypointTransaction transaction in loan.TransactionsWithPaypoint) {

				if (paypointTransactions.FirstOrDefault(t => t.Id == transaction.Id) == null) {
					continue;
				}

				transaction.Description = transaction.Description + "; removed amount = " + transaction.Amount;
				transaction.Interest = 0;
				transaction.Fees = 0;
				transaction.LoanRepayment = 0;
				transaction.Rollover = 0;

				transaction.Cancelled = true;
				transaction.CancelledAmount = transaction.Amount;

				transaction.Amount = 0;

				var transaction1 = transaction;

				// reset paid charges 
				loan.Charges.Where(f => f.Date <= transaction1.PostDate).ForEach(f => f.AmountPaid = 0);
				loan.Charges.Where(f => f.Date <= transaction1.PostDate).ForEach(f => f.State = null);

				IEnumerable<LoanScheduleTransaction> schTransactions = loan.ScheduleTransactions.Where(st => st.Transaction.Id == transaction1.Id);

				foreach (LoanScheduleTransaction st in schTransactions) {
					var paidSchedule = loan.Schedule.FirstOrDefault(s => s.Id == st.Schedule.Id);
					if (paidSchedule != null) {
						// reset paid rollover
						foreach (PaymentRollover r in paidSchedule.Rollovers) {
							r.PaidPaymentAmount = 0;
							r.CustomerConfirmationDate = null;
							r.PaymentNewDate = null;
							r.Status = (r.ExpiryDate.HasValue && r.ExpiryDate.Value <= DateTime.UtcNow) ? RolloverStatus.Expired : RolloverStatus.New;
						}
					}
				}
			}
		}
		
		public bool IsAmountChangingAllowed(CashRequest cr) {
			if (cr == null || string.IsNullOrEmpty(cr.LoanTemplate))
				return true;

			var model = EditLoanDetailsModel.Parse(cr.LoanTemplate);

			//compare number of installments/other actions
			if (model.Items.Count(i => i.Type == "Installment") != cr.RepaymentPeriod)
				return false;
			if (model.Items.Count != cr.RepaymentPeriod)
				return false;

			//compare template balances with actual
			var expectedBalances = cr.LoanType.GetBalances(cr.ApprovedSum(), cr.RepaymentPeriod);
			var actualBalances = model.Items.Where(i => i.Type == "Installment").Select(i => i.Balance);
			if (expectedBalances.Except(actualBalances).Any())
				return false;

			return true;
		}
	} // class ChangeLoanDetailsModelBuilder
} // namespace