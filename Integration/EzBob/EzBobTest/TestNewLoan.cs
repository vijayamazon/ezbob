namespace EzBobTest {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Security;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Database;
	using Ezbob.Utils;
	using EzBob.Backend.Models;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using Newtonsoft.Json;
	using NUnit.Framework;
	using PaymentServices.Calculators;
	using StructureMap;

	[TestFixture]
	class TestNewLoan : BaseTestFixtue {


		[Test]
		public void TestLoanLegals() {
			//NL_Loans l = new NL_Loans();
			//l.InterestRate = 3.5m;
			//l.IssuedTime = DateTime.UtcNow;
			//l.CreationTime = DateTime.UtcNow;
			//l.LoanSourceID = 1;
			//l.LoanStatusID = 2;
			//l.Position = 1;
			//var sss = l.ToString();
			//Console.WriteLine(sss);
			NL_LoanLegals loanLegals = new NL_LoanLegals {
				Amount = 20000m,
				RepaymentPeriod = 5,
				SignatureTime = DateTime.UtcNow,
				EUAgreementAgreed = true,
				COSMEAgreementAgreed = true,
				CreditActAgreementAgreed = true,
				PreContractAgreementAgreed = false,
				PrivateCompanyLoanAgreementAgreed = true,
				GuarantyAgreementAgreed = false,
				SignedName = "elina roytman",
				NotInBankruptcy = false
			};
			var s = loanLegals.ToString();
			Console.WriteLine(s);
		}


		[Test]
		public void TestRepaymentIntervalTypes() {
			Console.WriteLine(Enum.ToObject(typeof(RepaymentIntervalTypes), 1));
		}

		[Test]
		public void TestLoanOptions1() {
			NL_LoanOptions NL_options = new NL_LoanOptions {
				LoanID = 123,
				AutoCharge = true,
				StopAutoChargeDate = DateTime.UtcNow,
				AutoLateFees = true,
				StopAutoLateFeesDate = DateTime.UtcNow,
				AutoInterest = true,
				StopAutoInterestDate = DateTime.UtcNow,
				ReductionFee = true,
				LatePaymentNotification = true,
				CaisAccountStatus = "asd",
				ManualCaisFlag = "qwe",
				EmailSendingAllowed = true,
				SmsSendingAllowed = true,
				MailSendingAllowed = true,
				UserID = 25,
				InsertDate = DateTime.UtcNow,
				IsActive = true,
				Notes = null
			};
			//var stra = new AddLoanOptions(NL_options, null);
			//stra.Execute();
		}


		[Test]
		public void BuildLoanFromOffer() {
			NL_Model model = new NL_Model(56) {
				UserID = 357,
				CalculatorImplementation = typeof(BankLikeLoanCalculator).AssemblyQualifiedName,
				Loan = new NL_Loans()
			};
			model.Loan.Histories.Add(new NL_LoanHistory() { EventTime = DateTime.UtcNow });
			BuildLoanFromOffer strategy = new BuildLoanFromOffer(model);
			strategy.Context.UserID = model.UserID;
			try {
				strategy.Execute();
				Console.WriteLine(strategy.Result.Error);
				m_oLog.Debug(strategy.Result);
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}


		[Test]
		public void TestScheduleFormat() {
			var x1 = new NL_LoanSchedules() {
				AmountDue = 55m,
				Balance = 22m,
				FeesAmount = 30.77m,
				FeesPaid = 20.99m,
				Interest = 12m,
				InterestRate = 2.25m,
				InterestPaid = 1.55m,
				LoanHistoryID = 1,
				LoanScheduleID = 0,
				LoanScheduleStatusID = 3,
				PlannedDate = new DateTime(2014, 10, 19)
			};
			this.m_oLog.Debug(x1.ToString());
		}


		[Test]
		public void AddOffer() {

			GetLastOffer lastOfferstrategy = new GetLastOffer(374);
			lastOfferstrategy.Execute();
			NL_Offers lastOffer = lastOfferstrategy.Offer;
			long crID = 337;
			long decisionID = 23;

			ICashRequestRepository crRep = ObjectFactory.GetInstance<CashRequestRepository>();
			CashRequest	oldCashRequest = crRep.Get(crID);

			lastOffer.DecisionID = decisionID;
			lastOffer.LoanSourceID = oldCashRequest.LoanSource.ID;
			lastOffer.LoanTypeID = oldCashRequest.LoanType.Id;
			lastOffer.RepaymentIntervalTypeID = (int)RepaymentIntervalTypes.Month;
			lastOffer.StartTime = (DateTime)oldCashRequest.OfferStart;
			lastOffer.EndTime = (DateTime)oldCashRequest.OfferValidUntil;
			lastOffer.RepaymentCount = oldCashRequest.ApprovedRepaymentPeriod ?? 0;
			lastOffer.Amount = (decimal)oldCashRequest.ManagerApprovedSum;
			lastOffer.MonthlyInterestRate = oldCashRequest.InterestRate;
			lastOffer.CreatedTime = DateTime.UtcNow;
			lastOffer.BrokerSetupFeePercent = oldCashRequest.BrokerSetupFeePercent;
			lastOffer.Notes = "bbb";
			lastOffer.DiscountPlanID = oldCashRequest.DiscountPlan.Id;
			lastOffer.IsLoanTypeSelectionAllowed = oldCashRequest.IsLoanTypeSelectionAllowed == 1;
			lastOffer.SendEmailNotification = !oldCashRequest.EmailSendingBanned;
			lastOffer.IsRepaymentPeriodSelectionAllowed = oldCashRequest.IsCustomerRepaymentPeriodSelectionAllowed;
			lastOffer.IsAmountSelectionAllowed = true;

			// offer-fees
			NL_OfferFees setupFee = new NL_OfferFees() {
				LoanFeeTypeID = (int)NLFeeTypes.SetupFee,
				Percent = oldCashRequest.ManualSetupFeePercent,
				//OneTimePartPercent = 1, DistributedPartPercent = 0 // default
			};
			if (oldCashRequest.SpreadSetupFee != null && oldCashRequest.SpreadSetupFee == true) {
				setupFee.LoanFeeTypeID = (int)NLFeeTypes.ServicingFee;
				setupFee.OneTimePartPercent = 0;
				setupFee.DistributedPartPercent = 1;
			}
			List<NL_OfferFees> offerFees = new List<NL_OfferFees>() { setupFee };
			//this.m_oLog.Debug("NL: offer: {0}, offerFees: {1}" + "", lastOffer, offerFees);
			AddOffer offerStrategy = new AddOffer(lastOffer, offerFees);
			offerStrategy.Execute();
			Console.WriteLine(offerStrategy.OfferID);
			Console.WriteLine(offerStrategy.Error);
		}


		[Test]
		public void AddLoan() {
			DateTime now = DateTime.UtcNow;

			AgreementModel agreementModel = new AgreementModel() {
				CustomerEmail = "alexbo+003@ezbob.com.test.test.test",
				APR = 35.35,
				FullName = "Jane Doe",
				CountRepayment = 3
			};

			NL_Model model = new NL_Model(56) {
				UserID = 357,
				CalculatorImplementation = typeof(BankLikeLoanCalculator).AssemblyQualifiedName,
				Loan = new NL_Loans() { OldLoanID = 2074, Refnum = "01574390005" },
				FundTransfer = new NL_FundTransfers() {
					Amount = 1000,
					FundTransferStatusID = (int) NLFundTransferStatuses.Pending, // (int)NLPacnetTransactionStatuses.Done,
					LoanTransactionMethodID = (int)NLLoanTransactionMethods.Pacnet,
					TransferTime = now,
					PacnetTransactions = new List<NL_PacnetTransactions>()
				}
			};
			model.FundTransfer.PacnetTransactions.Clear();
			model.FundTransfer.PacnetTransactions.Add(new NL_PacnetTransactions() {
				Amount = 1000,
				Notes = "addloan utest",
				PacnetTransactionStatusID = (int)NLPacnetTransactionStatuses.Done,
				StatusUpdatedTime = now,
				TrackingNumber = "1111",
				TransactionTime = now
			});
			model.Loan.Histories.Add(new NL_LoanHistory() {
				EventTime = now,
				AgreementModel = JsonConvert.SerializeObject(agreementModel)
			});
			model.Loan.LastHistory().Agreements.Add(new NL_LoanAgreements() {
				LoanAgreementTemplateID = (int)NLLoanAgreementTemplateTypes.PreContractAgreement,
				FilePath = "preContract/cc/dd.pdf"
			});
			model.Loan.LastHistory().Agreements.Add(new NL_LoanAgreements() {
				LoanAgreementTemplateID = (int)NLLoanAgreementTemplateTypes.GuarantyAgreement,
				FilePath = "guarantyAgreement/aa/bb.pdf"
			});
			
			model.Loan.LastHistory().AgreementModel = JsonConvert.SerializeObject(agreementModel);

			AddLoan strategy = new AddLoan(model);
			strategy.Context.UserID = model.UserID;
			try {
				strategy.Execute();

				this.m_oLog.Debug(strategy.Error);
				this.m_oLog.Debug(strategy.LoanID);

				Console.WriteLine("LoanID: {0}, Error: {1}", strategy.LoanID, strategy.Error);
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}


		/// <exception cref="OverflowException"><paramref /> represents a number less than <see cref="F:System.Decimal.MinValue" /> or greater than <see cref="F:System.Decimal.MaxValue" />. </exception>
		[Test]
		public void DiscountPlan() {
			var discounts = this.m_oDB.Fill<NL_DiscountPlanEntries>(
						"NL_DiscountPlanEntriesGet",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@DiscountPlanID", 2)
						);
			if (discounts != null) {
				discounts.ForEach(d => this.m_oLog.Debug(d));
				NL_Model model = new NL_Model(374);
				foreach (NL_DiscountPlanEntries dpe in discounts) {
					model.Offer.DiscountPlan.Add(Decimal.Parse(dpe.InterestDiscount.ToString(CultureInfo.InvariantCulture)));
				}
				model.Offer.DiscountPlan.ForEach(d => Console.WriteLine(d));
			}
		}

		/// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
		/// <exception cref="IOException">An I/O error occurred while opening the file. </exception>
		/// <exception cref="UnauthorizedAccessException"><paramref name="path" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
		/// <exception cref="FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
		/// <exception cref="SecurityException">The caller does not have the required permission. </exception>
		public string GetTemplate(string path, string name) {
			return File.ReadAllText(string.Format("{0}{1}{2}.cshtml", @"D:\ezbob\App\PluginWeb\EzBob.Web\", path, name));
		}

		/// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
		/// <exception cref="IOException">An I/O error occurred while opening the file. </exception>
		/// <exception cref="UnauthorizedAccessException"><paramref /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref /> specified a directory.-or- The caller does not have the required permission. </exception>
		/// <exception cref="FileNotFoundException">The file specified in <paramref /> was not found. </exception>
		/// <exception cref="SecurityException">The caller does not have the required permission. </exception>
		public string GetTemplateByName(string name) {
			return GetTemplate("\\Areas\\Customer\\Views\\Agreement\\", name);
		} // GetTemplateByName



		[Test]
		public void AgreementsSave() {
			DatabaseDataHelper _helper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			// 1
			var preContract = GetTemplateByName("" + "Pre-Contract-Agreement");
			Console.WriteLine(preContract);
			// 2
			// specific LoanAgreementTemplate for current type: 
			var preContractTemplate = _helper.GetOrCreateLoanAgreementTemplate(preContract, false ? LoanAgreementTemplateType.EzbobAlibabaPreContract : LoanAgreementTemplateType.PreContract);
			Console.WriteLine(preContractTemplate.TemplateType);
			// 3
			//var preContractAgreement = new LoanAgreement("precontract", new Loan(), preContractTemplate);
			//Console.WriteLine(preContractAgreement.ToString());
			//Console.WriteLine(Path.Combine(@"C:\temp\logs\", @"xxx\yyy.txt"));
		}

		[Test]
		public void TestNL_AddPayment() {
			int customerID = 369;
			int loanID = 5;
			decimal amount = 5;

			NL_Model nlModel = new NL_Model(customerID);

			nlModel.Loan = new NL_Loans() {
				LoanID = loanID
			};

			//nlModel.PaypointTransactionStatus = "Done";

			//nlModel.Payment = new NL_Payments() {
			//	PaymentMethodID = (int)NLLoanTransactionMethods.SystemRepay, //2,
			//	PaymentTime = DateTime.UtcNow,
			//	PaymentStatusID = (int)NLPaymentStatuses.Active, //???
			//	Amount = amount,
			//	Notes = "system-repay"
			//};

			//nlModel.PaypointTransaction = new NL_PaypointTransactions() {
			//	TransactionTime = DateTime.UtcNow,
			//	Amount = amount,
			//	Notes = "system-repay",
			//	PaypointUniqueID = "4f0fce47-deb0-4667-bc65-f6edd3c978b5",
			//	IP = "127.0.0.1",
			//	PaypointTransactionStatusID = (int)NLPaypointTransactionStatuses.Done
			//};

			var s = new AddPayment(nlModel);
			try {
				s.Execute();
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		/*[Test]
		public void TestLoanState() {
			int loanID = 2151; // cust 329;   
			var s = new LoanState<Loan>(new Loan(), loanID, DateTime.UtcNow);
			try {
				s.Execute();
				LoanCalculatorModel calculatorModel = s.CalcModel;
				//Console.WriteLine(calculatorModel.ToString());
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}*/


		private int _daysInMonth = 0;
		private const decimal InterestRate = 0.6m;


		[Test]
		public void CalcInterestRate() {
			DateTime start = new DateTime(2015, 01, 01);
			DateTime end = new DateTime(2015, 01, 15);
			Console.WriteLine(GetInterestRate(start, end));
		}

		public decimal GetInterestRate(DateTime start, DateTime end) {

			//Console.WriteLine("-start: {0}, end: {1}", start, end);

			var rate = 0m;

			for (var start2 = start.AddMonths(1); start2 <= end; ) {

				Console.WriteLine("---start: {0}, end: {1}", start, end);

				if (start2.Year == end.Year && start2.Month == end.Month) {
					start2 = end;
				}

				rate += GetInterestRateOneMonth(start, start2);
				this._daysInMonth = MiscUtils.DaysInMonth(start2);
				start = start2;
				start2 = start2.AddMonths(1);
			}

			rate += GetInterestRateOneMonth(start, end);
			return rate;
		}

		private decimal GetInterestRateOneMonth(DateTime start, DateTime end) {

			TimeSpan span = (end.Date - start.Date);
			decimal days = (decimal)Math.Floor(span.TotalDays);

			Console.WriteLine("-------------start: {0}, end: {1}, days: {2}", start, end, days);

			decimal nTotalInterest = 0;

			List<NL_LoanInterestFreeze> activeFreezes = new List<NL_LoanInterestFreeze>();  //this._loan.InterestFreeze.Where(f => f.DeactivationDate == null).ToList();

			if (activeFreezes.Count == 0) {
				nTotalInterest = (this._daysInMonth == 0) ? 0 : (days * InterestRate / this._daysInMonth);
				return nTotalInterest;
			}

			decimal nCurrentInterestRate = InterestRate;

			decimal nStdOneDayInterest = nCurrentInterestRate / this._daysInMonth;

			for (DateTime oCurrent = start.Date; oCurrent < end.Date; oCurrent = oCurrent.AddDays(1)) {
				var relevantFreeze = activeFreezes.FirstOrDefault(f => (f.StartDate >= oCurrent && oCurrent <= f.EndDate));
				nTotalInterest += (relevantFreeze == null) ? nStdOneDayInterest : (relevantFreeze.InterestRate / this._daysInMonth);
			}

			return nTotalInterest;
		}

		[Test]
		public void OldCalc() {
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan loan = loanRep.Get(2);
			var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();
			this.m_oLog.Debug("---------------------------------------: \n {0}", loan);
		}


	} // class TestNewLoan
} // namespace