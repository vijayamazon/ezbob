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
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.LegalDocs;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Collection;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Backend.Strategies.NewLoan.Migration;
	using Ezbob.Database;
	using Ezbob.Utils;
	using EzBob.Models;
	using EzServiceAccessor;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using Newtonsoft.Json;
	using NHibernate.Linq;
	using NUnit.Framework;
	using PaymentServices.Calculators;
	using ServiceClientProxy;
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
				LoanID = 2,
				StopAutoChargeDate = DateTime.UtcNow,
				PartialAutoCharging = true,
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
			var stra = new AddLoanOptions(NL_options, 5137);
			stra.Execute();
		}


		[Test]
		public void BuildLoanFromOffer() {
			NL_Model model = new NL_Model(1394) {
				UserID = 357,
				Loan = new NL_Loans()
			};
			model.Loan.Histories.Add(new NL_LoanHistory() { EventTime = DateTime.UtcNow });
			BuildLoanFromOffer strategy = new BuildLoanFromOffer(model);
			strategy.Context.UserID = model.UserID;
			try {
				strategy.Execute();
				//if (string.IsNullOrEmpty(strategy.Result.Error)) {
				//	this.m_oLog.Debug(strategy.Result.Offer);
				m_oLog.Debug(strategy.Result.Loan);
				//} else
				m_oLog.Debug("error: {0}", strategy.Result.Error);
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}


		[Test]
		public void TestScheduleFormat() {
			var x1 = new NL_LoanSchedules() {
				AmountDue = 55m,
				Balance = 22m,
				Fees = 30.77m,
				//FeesPaid = 20.99m,
				Interest = 12m,
				InterestRate = 2.25m,
				//InterestPaid = 1.55m,
				LoanHistoryID = 1,
				LoanScheduleID = 0,
				LoanScheduleStatusID = 3,
				PlannedDate = new DateTime(2014, 10, 19)
			};
			m_oLog.Debug(x1.ToString());
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

		public void Discounts() {
			int oldLoanID = 2076;
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan oldLoan = loanRep.Get(oldLoanID);
			var cr = oldLoan.Customer.CashRequests.OrderBy(c => c.CreationDate)
				.LastOrDefault();
			if (cr != null) {
				for (int i = 0; i < oldLoan.CashRequest.DiscountPlan.Discounts.Length; i++) {
					Console.WriteLine(oldLoan.CashRequest.DiscountPlan.Discounts[i]);
				}
				//Console.WriteLine(cr.ToString());
				cr.DiscountPlan.Discounts.ForEach(d => m_oLog.Debug("=====" + d.ToString(CultureInfo.InvariantCulture)));
			}
		}
		[Test]
		public void AddLoan() {
			const int userID = 357;
			const int oldLoanID = 7152;
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan oldLoan = loanRep.Get(oldLoanID);
			if (oldLoan == null)
				return;
			DateTime now = DateTime.UtcNow; // oldLoan.Date;
			NL_Model model = new NL_Model(oldLoan.Customer.Id) {
				UserID = userID,
				Loan = new NL_Loans() { OldLoanID = oldLoan.Id, Refnum = oldLoan.RefNumber },
				FundTransfer = new NL_FundTransfers() {
					Amount = oldLoan.LoanAmount,
					FundTransferStatusID = (int)NLFundTransferStatuses.Pending, // (int)NLPacnetTransactionStatuses.Done,
					LoanTransactionMethodID = (int)NLLoanTransactionMethods.Pacnet,
					TransferTime = now,
					PacnetTransactions = new List<NL_PacnetTransactions>()
				}
			};
			model.Loan.Histories.Add(new NL_LoanHistory() {
				EventTime = now,
				AgreementModel = JsonConvert.SerializeObject(oldLoan.AgreementModel)
			});
			model.Loan.LastHistory().Agreements.Add(new NL_LoanAgreements() {
				LoanAgreementTemplateID = 2065, //(int)NLLoanAgreementTemplateTypes.PreContractAgreement,
				FilePath =  "2016/2/21/PAD64J14012/Guaranty Agreement_Deka_Dance_1394_21-02-2016_10-18-25.pdf", // "preContract/cc/dd" + oldLoan.RefNumber + ".pdf"
			});
			model.Loan.LastHistory().Agreements.Add(new NL_LoanAgreements() {
				LoanAgreementTemplateID = 2067, //(int)NLLoanAgreementTemplateTypes.GuarantyAgreement,
				FilePath = "2016/2/21/PAD64J14012/Private Company Loan Agreement_Deka_Dance_1394_21-02-2016_10-18-25.pdf", //"guarantyAgreement/aa/bb" + oldLoan.RefNumber + ".pdf"
			});
			AddLoan strategy = new AddLoan(model);
			strategy.Context.UserID = model.UserID;
			try {
				strategy.Execute();
				m_oLog.Debug(strategy.Error);
				m_oLog.Debug(strategy.LoanID);
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
				discounts.ForEach(d => m_oLog.Debug(d));
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

		/// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
		/// <exception cref="IOException">An I/O error occurred while opening the file. </exception>
		/// <exception cref="UnauthorizedAccessException"><paramref /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref /> specified a directory.-or- The caller does not have the required permission. </exception>
		/// <exception cref="FileNotFoundException">The file specified in <paramref /> was not found. </exception>
		/// <exception cref="SecurityException">The caller does not have the required permission. </exception>
		[Test]
		public void AgreementsSave() {
			DatabaseDataHelper _helper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			// 1
			var preContract = GetTemplateByName("" + "Pre-Contract-Agreement");
			Console.WriteLine(preContract);
			// 2
			// specific LoanAgreementTemplate for current type: 
            //var preContractTemplate = _helper.GetOrCreateLoanAgreementTemplate(preContract, false ? LoanAgreementTemplateType.EzbobAlibabaPreContract : LoanAgreementTemplateType.PreContract);
            //Console.WriteLine(preContractTemplate.TemplateType);
			// 3
			//var preContractAgreement = new LoanAgreement("precontract", new Loan(), preContractTemplate);
			//Console.WriteLine(preContractAgreement.ToString());
			//Console.WriteLine(Path.Combine(@"C:\temp\logs\", @"xxx\yyy.txt"));
		}

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
			Loan loan = loanRep.Get(5);
			var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();
			Console.WriteLine(loan.ToString());
			Console.WriteLine();
			Console.WriteLine(calc.ToString());
			m_oLog.Debug("---------------------------------------: \n {0}", loan);
		}


		[Test]
		public void TestLoanOldCalculator() {
			int	loanID = 5211;
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan loan = loanRep.Get(2);
			/*ChangeLoanDetailsModelBuilder loanModelBuilder = new ChangeLoanDetailsModelBuilder();
			EditLoanDetailsModel model = new EditLoanDetailsModel();
			var loaan =  ObjectFactory.GetInstance<LoanRepository>().Get(loanID);
			// 1. build model from DB loan
			model = loanModelBuilder.BuildModel(loaan);
			//m_oLog.Debug("===========================" + model.InterestFreeze.Count);
			// 2. create DB loan from the model
			Loan loan1 = loanModelBuilder.CreateLoan(model);
			//m_oLog.Debug("----------------------" + loan1.InterestFreeze.Count);*/
			var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();
			m_oLog.Debug("---------------------------------------Loan recalculated: \n {0}", loan);
		}



		[Test]
		public void TestLoanInterestRate() {
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan loan = loanRep.Get(5211);
			var firstSchedule = loan.Schedule.OrderBy(s => s.Date).FirstOrDefault();
			var lastSchedule = loan.Schedule.OrderBy(s => s.Date).LastOrDefault();
			Console.WriteLine(firstSchedule);
			Console.WriteLine(lastSchedule);
			var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			decimal r = 5;
			if (firstSchedule != null && lastSchedule != null)
				//r = calc.GetInterestRate(firstSchedule.Date, lastSchedule.Date);
				r = calc.GetInterestRate(firstSchedule.Date, new DateTime(2099, 01, 01));
			m_oLog.Debug("{0}", loan);
			Console.WriteLine(r);
		}

		[Test]
		public void TestLoanInterestRateBetweenDates() {
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan loan = loanRep.Get(5211);
			DateTime start = new DateTime(2015, 07, 08);
			DateTime end = new DateTime(2015, 10, 21);
			Console.WriteLine(start);
			Console.WriteLine(end);
			var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();
			//this.m_oLog.Debug("{0}",loan);
			decimal I = 0m;
			decimal P = 57024.55m;
			TimeSpan ts = end.Date.Subtract(start.Date);
			Console.WriteLine(ts);
			int dcounter = 1;
			while (dcounter < ts.Days) {
				DateTime s = start.Date.AddDays(dcounter);
				DateTime e = s.Date.AddDays(1);
				Console.WriteLine("{0}, {1}", s, e);
				decimal r = calc.GetInterestRate(s, e);
				dcounter++;
				Console.WriteLine("{0}, {1}", dcounter, r);
				I += P * r;
			}
			Console.WriteLine(I);
		}

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		[Test]
		public void LoanStateStrategy() {
			const long loanID = 9;
			var strategy = new GetLoanState(351, loanID, DateTime.UtcNow, 1, false); // loanID = 17, customer = 56
			strategy.Execute();
			m_oLog.Debug(strategy.Result);
		}

		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		[Test]
		public void GetLoanFees() {
			const long loanid = 9;
			var loanFees = this.m_oDB.Fill<NL_LoanFees>("NL_LoanFeesGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", loanid));
			m_oLog.Debug(AStringable.PrintHeadersLine(typeof(NL_LoanFees)));
			loanFees.ForEach(f => m_oLog.Debug(f));
		}


		[Test]
		public void CalculatorState() {
			DateTime calcTime = DateTime.UtcNow;
			const long loanID = 10007;
			const int customerID = 59;
			GetLoanState dbState = new GetLoanState(customerID, loanID, calcTime, 357, false);
			try {
				dbState.Execute();
			} catch (NL_ExceptionInputDataInvalid nlExceptionInputDataInvalid) {
				Console.WriteLine(nlExceptionInputDataInvalid.Message);
				return;
			}
			try {
				ALoanCalculator calc = new LegacyLoanCalculator(dbState.Result, calcTime);
				calc.GetState();
				m_oLog.Debug("----------------------------------{0}", calc.WorkingModel);
			} catch (Exception exception) {
				m_oLog.Error("{0}", exception.Message);
			}

			return;

			// old loan
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan oldLoan = loanRep.Get(dbState.Result.Loan.OldLoanID);
			// old calc
			LoanRepaymentScheduleCalculator oldCalc = new LoanRepaymentScheduleCalculator(oldLoan, calcTime, 0);
			oldCalc.GetState();

			m_oLog.Debug("++++++++++++++++++++++++++++++old loan: {0}", oldLoan);
			m_oLog.Debug("NextEarlyPayment={0}", oldCalc.NextEarlyPayment());
		}

		[Test]
		public void GetLoanStateTest() {
			DateTime calcTime = DateTime.UtcNow;
			/*const long loanID = 21; const int customerID = 351;*/
			const long loanID = 4;
			const int customerID = 1394;
			GetLoanState state = new GetLoanState(customerID, loanID, calcTime, 357);
			try {
				state.Execute();
				m_oLog.Debug("----------------------------------{0}", state.Result);
			} catch (NL_ExceptionInputDataInvalid nlExceptionInputDataInvalid) {
				Console.WriteLine(nlExceptionInputDataInvalid.Message);
			} catch (Exception ex) {
				m_oLog.Error("{0}", ex.Message);
			}
		}



		[Test]
		public void APR() {
			const int userID = 357;
			const int oldLoanID = 3091;
			LoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			Loan oldLoan = loanRep.Get(oldLoanID);
			DateTime now = oldLoan.Date;
			NL_Model model = new NL_Model(oldLoan.Customer.Id) {
				UserID = userID,
				Loan = new NL_Loans() { OldLoanID = oldLoan.Id, Refnum = oldLoan.RefNumber }
			};
			model.Loan.Histories.Add(new NL_LoanHistory() {
				EventTime = now,
				InterestRate = oldLoan.InterestRate,
				RepaymentCount = oldLoan.Schedule.Count,
				Amount = oldLoan.LoanAmount
			});
			try {
				ALoanCalculator calc = new LegacyLoanCalculator(model);
				double apr = calc.CalculateApr();
				m_oLog.Debug("CalculationDate: {0}, apr: {1}", calc.CalculationDate, apr);
			} catch (Exception exception) {
				m_oLog.Error("{0}", exception.Message);
			}
		}

		[Test]
		public void CreateSchedule() {
			DateTime issueDate = DateTime.UtcNow; // new DateTime(2015, 12, 8, 19, 12, 00);
			NL_Model model = new NL_Model(1394) { UserID = 357, Loan = new NL_Loans() };
			model.Loan.Histories.Add(new NL_LoanHistory() { EventTime = issueDate });
			BuildLoanFromOffer strategy = new BuildLoanFromOffer(model);
			strategy.Execute();
			if (!string.IsNullOrEmpty(strategy.Error)) {
				m_oLog.Debug("error: {0}", strategy.Error);
				return;
			}
			model = strategy.Result;
			m_oLog.Debug("=================================={0}\n", model.Loan);
			try {
				ALoanCalculator calc = new LegacyLoanCalculator(model);
				calc.CreateSchedule();
				m_oLog.Debug("=================Calculator end================={0}\n", model.Loan);
			} catch (Exception exception) {
				m_oLog.Error("{0}", exception.Message);
			}
		}


		[Test]
		public void LateLoanJob() {
			LateLoanJob strategy = new LateLoanJob(DateTime.UtcNow);
			strategy.Execute();
		}

		// n = A/(m-Ar);
		// total = A+A*r*((n+1)/2)

		

		[Test]
		public void RolloverRescheduling() {
			DateTime calcTime = DateTime.UtcNow;
			const long loanID = 17; // 21;
			GetLoanState dbState = new GetLoanState(56, loanID, calcTime, 357);
			try {
				dbState.Execute();
			} catch (NL_ExceptionInputDataInvalid nlExceptionInputDataInvalid) {
				Console.WriteLine(nlExceptionInputDataInvalid.Message);
			}
			NL_Model model = dbState.Result;
			ILoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			var oldLoan = loanRep.Get(model.Loan.OldLoanID);
			var rolloverRep = ObjectFactory.GetInstance<PaymentRolloverRepository>();
			var oldRollover = rolloverRep.GetByLoanId(oldLoan.Id).FirstOrDefault();
			// copy rollover+fee+payment to NL
			if (oldRollover != null && oldRollover.PaidPaymentAmount > 0) {
				DateTime rolloverConfirmationDate = (DateTime)oldRollover.CustomerConfirmationDate;
				var fee = model.Loan.Fees.LastOrDefault();
				model.Loan.Fees.Add(new NL_LoanFees() {
					Amount = CurrentValues.Instance.RolloverCharge,
					AssignTime = rolloverConfirmationDate,
					CreatedTime = rolloverConfirmationDate,
					LoanFeeID = (fee.LoanFeeID + 1),
					LoanFeeTypeID = (int)NLFeeTypes.RolloverFee,
					AssignedByUserID = 1,
					LoanID = loanID,
					Notes = "rollover fee"
				});
				fee = model.Loan.Fees.LastOrDefault();
				model.Loan.AcceptedRollovers.Add(new NL_LoanRollovers() {
					CreatedByUserID = 1,
					CreationTime = oldRollover.Created,
					CustomerActionTime = oldRollover.CustomerConfirmationDate,
					ExpirationTime = rolloverConfirmationDate,
					IsAccepted = true,
					LoanHistoryID = model.Loan.LastHistory().LoanHistoryID,
					LoanFeeID = fee.LoanFeeID
				});
				var	transaction = oldLoan.Transactions.LastOrDefault();
				if (transaction != null) {
					model.Loan.Payments.Add(new NL_Payments() {
						Amount = transaction.Amount,
						CreatedByUserID = 1,
						CreationTime = transaction.PostDate,
						LoanID = model.Loan.LoanID,
						PaymentTime = transaction.PostDate,
						Notes = "dummy payment for rollover",
						PaymentStatusID = (int)NLPaymentStatuses.Active,
						PaymentMethodID = (int)NLLoanTransactionMethods.Manual,
						PaymentID = 15
					});
				}
			}
			/*try {
				ALoanCalculator calc = new LegacyLoanCalculator(model, calcTime);
				calc.RolloverRescheduling();
				m_oLog.Debug("{0}", calc);
				// old calc
				LoanRepaymentScheduleCalculator oldCalc = new LoanRepaymentScheduleCalculator(oldLoan, calcTime, 0);
				oldCalc.GetState();
				m_oLog.Debug("old loan State: {0}", oldLoan);
				m_oLog.Debug("\n\n====================OLD CALC InterestToPay={0}, FeesToPay={1}", oldCalc.InterestToPay, oldCalc.FeesToPay);
			} catch (Exception exception) {
				m_oLog.Error("{0}", exception.Message);
			}*/
		}

		[Test]
		public void AddPaymentTest() {
			const int customerid = 390;
			const long loanID = 15;
			/*NL_Payments nlpayment = new NL_Payments() {
				Amount = 100m,
				CreatedByUserID = 357,
				PaymentTime = new DateTime(2015, 10, 25),
				LoanID = loanID,
				Notes = "offset",
				PaymentStatusID = (int)NLPaymentStatuses.Active,
				PaymentMethodID = (int)NLLoanTransactionMethods.SetupFeeOffset
			};*/
		/*	DateTime prebatedate = DateTime.UtcNow; // new DateTime(2015, 12, 6);
			NL_Payments nlpayment = new NL_Payments() {
				Amount = 5m,
				CreatedByUserID = customerid,
				CreationTime = prebatedate,
				LoanID = loanID,
				PaymentTime = prebatedate,
				Notes = "rebate",
				PaymentStatusID = (int)NLPaymentStatuses.Active,
				PaymentMethodID = (int)NLLoanTransactionMethods.SystemRepay
			};*/
			DateTime pdate = DateTime.Now; // new DateTime(2016, 1, 7);
			NL_Payments nlpayment = new NL_Payments() {
				Amount = 50m,
				CreatedByUserID = 357,
				CreationTime = DateTime.UtcNow,
				LoanID = loanID,
				PaymentTime = pdate,
				Notes = "payment3",
				PaymentStatusID = (int)NLPaymentStatuses.Active,
				PaymentMethodID = (int)NLLoanTransactionMethods.Manual
			};
			try {
				AddPayment pstrategy = new AddPayment(customerid, nlpayment, 357);
				pstrategy.Execute();
				m_oLog.Debug(pstrategy.Error);
			} catch (Exception ex) {
				m_oLog.Debug(ex);
			}
		}

		[Test]
		public void AddLateFeeTest() {
			const long loanID = 17;
			DateTime now = new DateTime(2015, 12, 28);

			NL_LoanFees fee = new NL_LoanFees() {
				LoanFeeTypeID = (int)NLFeeTypes.AdminFee,
				Amount = decimal.Parse(CurrentValues.Instance.AdministrationCharge.Value),    //NL_Model.GetLateFeesAmount(NLFeeTypes.AdminFee),
				AssignedByUserID = 1,
				AssignTime = now,
				CreatedTime = now,
				Notes = "test late fee3",
				LoanID = loanID
			};
			int result = this.m_oDB.ExecuteNonQuery("NL_LoanFeesSave", CommandSpecies.StoredProcedure, this.m_oDB.CreateTableParameter<NL_LoanFees>("Tbl", fee));
			m_oLog.Debug(result);
		}



		[Test]
		public void CancelPaymentTest() {
			const int customerid = 56;
			const long loanID = 17;
			const long paymentToCancel = 10050;
			DateTime canceldate = new DateTime(2015, 12, 30);
			var pp = this.m_oDB.Fill<NL_Payments>("NL_PaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", loanID));
			var nlpayment = pp.FirstOrDefault(p => p.PaymentID == paymentToCancel);
			if (nlpayment != null) {
				nlpayment.PaymentStatusID = (int)NLPaymentStatuses.ChargeBack;
				nlpayment.DeletionTime = canceldate;
				nlpayment.Notes = "charge";
				nlpayment.DeletedByUserID = 1;
				try {
					CancelPayment pstrategy = new CancelPayment(customerid, nlpayment, 357);
					pstrategy.Execute();
					m_oLog.Debug(pstrategy.Error);
				} catch (Exception ex) {
					m_oLog.Debug(ex);
				}
			}
		}

		/// <exception cref="NL_ExceptionCustomerNotFound">Condition. </exception>
		/// <exception cref="NL_ExceptionLoanNotFound">Condition. </exception>
		[Test]
		public void UpdateLoanDBStateTest() {
			const long loanID = 10009;
			const int customerID = 59;
			UpdateLoanDBState reloadLoanDBState = new UpdateLoanDBState(customerID, loanID, 357);
			reloadLoanDBState.Context.UserID = 357;
			try {
				reloadLoanDBState.Execute();
			} catch (NL_ExceptionInputDataInvalid nlExceptionInputDataInvalid) {
				m_oLog.Debug(nlExceptionInputDataInvalid);
			}
		}


		[Test]
		public void TestDateInterval() {
			const long loanID = 17;
			GetLoanState strategy = new GetLoanState(56, loanID, DateTime.UtcNow, 357);
			strategy.Execute();
			NL_Model model = strategy.Result;
			model.Loan.Payments.Clear();
			try {
				DateTime calcDate = new DateTime(2015, 10, 25);
				ALoanCalculator calc = new LegacyLoanCalculator(model);
				DateTime start = calcDate;
				DateTime end = new DateTime(2016, 02, 25);
				int days = end.Subtract(start).Days;
				m_oLog.Debug("total days: {0}", days);
				var schedule = model.Loan.LastHistory().Schedule;
				for (int i=0; i <= days; i++) {
					DateTime theDate = start.AddDays(i);
					var scheduleItem = schedule.FirstOrDefault(s => theDate.Date >= calc.PreviousScheduleDate(s.PlannedDate.Date, RepaymentIntervalTypes.Month) && theDate.Date <= s.PlannedDate.Date);
					m_oLog.Debug("theDate: {0}, {1}", theDate, scheduleItem.PlannedDate);
				}
			} catch (Exception exception) {
				m_oLog.Error("{0}", exception.Message);
			}
		}



		[Test]
		public void PayPoiinApiGetAmountToPay() {
			try {
				ILoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
				var loan = loanRep.Get(2070);
				var facade = new LoanPaymentFacade();
				var installment = loan.Schedule.FirstOrDefault(s => s.Id == 3773);
				var state = facade.GetStateAt(loan, DateTime.Now);
				m_oLog.Debug("Amount to charge is: {0}", state.AmountDue);
			} catch (Exception ex) {
				m_oLog.Debug(ex);
			}
		}

		[Test]
		public void AllPAymentLoanTest() {
			var xx = this.m_oDB.Fill<NL_Payments>("NL_PaymentsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", 17));
		}


		[Test]
		public void GetLoanIDByOldID() {
			int oldLoanId = 1063;
			var strategy = new GetLoanIDByOldID(oldLoanId);
			strategy.Execute();

			m_oLog.Debug(strategy.Error);
			m_oLog.Debug(strategy.LoanID);
		}


		[Test]
		public void TestPaymentFacade() {
			try {
				ILoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
				var loan = loanRep.Get(3117);
				NL_Payments nlPayment = new NL_Payments() {
					LoanID = 1,
					Amount = 1000,
					CreatedByUserID = 1, //this._context.UserId,
					CreationTime = DateTime.UtcNow,
					PaymentMethodID = (int)NLLoanTransactionMethods.SystemRepay
				};
				var f = new LoanPaymentFacade();
				f.PayLoan(loan, "111", 1000, "11.11.11.11", DateTime.UtcNow, "system-repay", false, null, nlPayment);
			} catch (Exception ex) {
				m_oLog.Debug(ex);
			}
		}




		[Test]
		public void GetCustomerLoansTest() {
			GetCustomerLoans s = new GetCustomerLoans(371);
			s.Execute();
			s.Loans.ForEach(l => m_oLog.Debug(l));
		}


		[Test]
		public void TestAddDecision() {
			AddDecision addDecision = new AddDecision(new NL_Decisions {
				UserID = 357,
				DecisionTime = DateTime.UtcNow,
				Notes = " old CR 30364",
				DecisionNameID = (int)DecisionActions.Reject
			}, 30364,
				 new List<NL_DecisionRejectReasons> {
					new NL_DecisionRejectReasons {
						RejectReasonID = 1
					},
					new NL_DecisionRejectReasons {
						RejectReasonID = 3
					}
				}
			);
			addDecision.Execute();
			Console.WriteLine(addDecision.DecisionID);
			Console.WriteLine(addDecision.Error);
		}

		[Test]
		public void DirectQueryTest() {
			AddLoan.LoanTransactionModel rebateTransaction = this.m_oDB.FillFirst<AddLoan.LoanTransactionModel>(
			   "select t.PostDate,t.Amount,t.Description,t.IP,t.PaypointId,c.Id as CardID from LoanTransaction t " +
			   "join PayPointCard c on c.TransactionId = t.PaypointId " +
			   "where Description='system-repay' " +
			   "and Status='Done' " +
			   "and Type ='PaypointTransaction' " +
			   "and LoanId = @LoanID " +
			   "and DateDiff(d, t.PostDate, @dd) = 0 " +
				"and LoanTransactionMethodId = @LoanTransactionMethodId",
			   CommandSpecies.Text,
			   new QueryParameter("@LoanID", 2079), new QueryParameter("@dd", new DateTime(2015, 10, 23)), new QueryParameter("@LoanTransactionMethodId", (int)NLLoanTransactionMethods.Auto)
			);
			m_oLog.Debug(rebateTransaction.Amount);
			m_oLog.Debug(rebateTransaction.PostDate);
			m_oLog.Debug(rebateTransaction.PaypointId);
		}

		[Test]
		public void LongGetCustomerLoansTest() {
			var nlLoansList = ObjectFactory.GetInstance<IEzServiceAccessor>().GetCustomerLoans(371).ToList();
			nlLoansList.ForEach(l => m_oLog.Debug("{0}", l.LoanID));
		}

		[Test]
		public void ShortGetCustomerLoansTest() {
			ServiceClient s = new ServiceClient();
			var nlLoansList = s.Instance.GetCustomerLoans(375, 1).Value;
			nlLoansList.ForEach(l => m_oLog.Debug("{0}", l.LoanID));
		}

		[Test]
		public void TestLateLoanJob() {
			var stra = new LateLoanJob(DateTime.UtcNow);
			stra.Execute();
		}


		[Test]
		public void TestLateNotificationJob() {
			var stra = new LateLoanNotification(DateTime.UtcNow);
			stra.Execute();
		}

		[Test]
		public void ApplayLateCharge() {
			int loanid = 5103;
			ILoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			var loan = loanRep.Get(loanid);
			DateTime now = DateTime.UtcNow;
			ILoanOptionsRepository optRep = ObjectFactory.GetInstance<LoanOptionsRepository>();
			var loanOptions = optRep.GetByLoanId(loanid);
			//	m_oLog.Debug("loanOption={0}", loanOptions.ToString());
			if (loanOptions != null && loanOptions.AutoLateFees == false) {
				if (((loanOptions.StopLateFeeFromDate.HasValue && now >= loanOptions.StopLateFeeFromDate.Value) &&
					(loanOptions.StopLateFeeToDate.HasValue && now <= loanOptions.StopLateFeeToDate.Value)) ||
					(!loanOptions.StopLateFeeFromDate.HasValue && !loanOptions.StopLateFeeToDate.HasValue)) {
					m_oLog.Debug("not applying late fee for loan {0} - auto late fee is disabled", loanid);
					return;
				}
			}
			var charge = new LoanCharge {
				Amount = 20,
				ChargesType = new ConfigurationVariable(CurrentValues.Instance.GetByID(CurrentValues.Instance.LatePaymentCharge.ID)),
				Date = now,
				Loan = loan
			};
			m_oLog.Debug("charge={0}", charge);
			var res = loan.TryAddCharge(charge);
			m_oLog.Debug("result={0}", res);
		}

		[Test]
		public void SetLateStatusTest() {
			SetLateLoanStatus s = new SetLateLoanStatus(null);
			s.Execute();
		}

		[Test]
		public void LateLoanNotificationTest() {
			LateLoanNotification s = new LateLoanNotification(null);
			s.Execute();
		}

		[Test]
		public void LateLoanCuredTest() {
			LateLoanCured s = new LateLoanCured(null);
			s.Execute();
		}

		[Test]
		public void LateLoanJobTest() {
			LateLoanJob s = new LateLoanJob(null);
			s.Execute();
		}

		[Test]
		public void AddSpreadedFeeTest() {
			const long loanID = 4;
			DateTime now = DateTime.UtcNow;
			List<NL_LoanFees> fees = new List<NL_LoanFees>();
			NL_LoanFees f1 = new NL_LoanFees() {
				LoanFeeTypeID = (int)NLFeeTypes.ServicingFee,
				Amount = 20,
				AssignedByUserID = 1,
				AssignTime = now,
				CreatedTime = now,
				Notes = "test servicing",
				LoanID = loanID
			};
			fees.Add(f1);
			NL_LoanFees f2 = new NL_LoanFees() {
				LoanFeeTypeID = (int)NLFeeTypes.ServicingFee,
				Amount = 20,
				AssignedByUserID = 1,
				AssignTime = now.AddMonths(1),
				CreatedTime = now,
				Notes = "test servicing",
				LoanID = loanID
			};
			fees.Add(f2);
			fees.ForEach(f => m_oLog.Debug(f));
			int result = this.m_oDB.ExecuteNonQuery("NL_LoanFeesSave", CommandSpecies.StoredProcedure, this.m_oDB.CreateTableParameter<NL_LoanFees>("Tbl", fees));
			m_oLog.Debug(result);
		}

		/// <exception cref="NL_ExceptionLoanNotFound">Condition. </exception>
		[Test]
		public void SaveRolloverTest() {
			const int oldID = 5116;
			const int customerID = 385;
			DateTime now = DateTime.UtcNow;
			PaymentRolloverRepository rep = ObjectFactory.GetInstance<PaymentRolloverRepository>();
			var rollovers = rep.GetByLoanId(oldID);
			var paymentRollovers = rollovers as IList<PaymentRollover> ?? rollovers.ToList();
			//paymentRollovers.ForEach(rr => this.m_oLog.Debug(rr));
			var r = paymentRollovers.FirstOrDefault(rr => rr.ExpiryDate > now);
			m_oLog.Debug(r);
			if (r == null) {
				return;
			}

			var s = new GetLoanIDByOldID(oldID);
			s.Execute();
			GetLoanState state = new GetLoanState(customerID, s.LoanID, now, 1, false);
			state.Execute();
			NL_LoanRollovers nlr = new NL_LoanRollovers() {
				CreatedByUserID = 1,
				CreationTime = r.Created,
				ExpirationTime = (DateTime)r.ExpiryDate,
				LoanHistoryID = state.Result.Loan.LastHistory().LoanHistoryID
			};
			SaveRollover saver = new SaveRollover(nlr, state.Result.Loan.LoanID);
			saver.Execute();
			m_oLog.Debug(saver.Error);
		}

		/// <exception cref="NL_ExceptionLoanNotFound">Condition. </exception>
		[Test]
		public void UpdateRolloverTest() {
			const int oldID = 5116;
			const int customerID = 385;
			DateTime now = DateTime.UtcNow;
			PaymentRolloverRepository rep = ObjectFactory.GetInstance<PaymentRolloverRepository>();
			var rollovers = rep.GetByLoanId(oldID);
			var paymentRollovers = rollovers as IList<PaymentRollover> ?? rollovers.ToList();
			//paymentRollovers.ForEach(rr => this.m_oLog.Debug(rr));
			var r = paymentRollovers.FirstOrDefault(rr => rr.ExpiryDate > now);
			m_oLog.Debug(r);
			if (r == null) {
				return;
			}

			var s = new GetLoanIDByOldID(oldID);
			s.Execute();
			GetLoanState state = new GetLoanState(customerID, s.LoanID, now, 1, false);
			state.Execute();

			NL_LoanRollovers nlr = state.Result.Loan.Rollovers.FirstOrDefault(nr => nr.CreationTime.Date == r.Created.Date && nr.ExpirationTime.Date == r.ExpiryDate);

			if (nlr == null) 
				return;

			nlr.ExpirationTime = nlr.ExpirationTime.AddDays(4);

			m_oLog.Debug(nlr);

			SaveRollover saver = new SaveRollover(nlr, state.Result.Loan.LoanID);
			saver.Execute();
			m_oLog.Debug(saver.Error);
		}

		
		[Test]
		public void AcceptRolloverTest() {
			const long loanID = 9;
			const int customerID = 387;
			AcceptRollover s = new AcceptRollover(customerID, loanID);
			s.Execute();
		}

	

		[Test]
		public void SaveFeeNewTest() {
			NL_LoanFees f = new NL_LoanFees() {
				LoanID = 3,
				Amount = 54,
				AssignedByUserID = 357,
				AssignTime = DateTime.UtcNow,
				CreatedTime = DateTime.UtcNow,
				LoanFeeTypeID = (int)NLFeeTypes.OtherCharge,
				Notes = "other fee",
				UpdatedByUserID = 357
			};
			SaveFee s = new SaveFee(f);
			try {
				s.Execute();
			} catch (Exception e) {
				m_oLog.Debug(e);
			}
		}

		[Test]
		public void SaveFeeUpdateTest() {
			NL_LoanFees f = m_oDB.FillFirst<NL_LoanFees>("NL_LoanFeesGet", 
				CommandSpecies.StoredProcedure, 
				new QueryParameter("LoanID", 3), 
				new QueryParameter("LoanFeeID", 20011));

			f.UpdatedByUserID=357;
			f.UpdateTime = DateTime.UtcNow;
			f.Amount = 29;
			f.AssignTime = DateTime.Now.Date.AddDays(-30);

			SaveFee s = new SaveFee(f);
			try {
				s.Execute();
			} catch (Exception e) {
				m_oLog.Debug(e);
			}
		}


		[Test]
		public void CompareNlOldFees() {
			int loanid = 5146;
			long lid = 9;
			ILoanRepository loanRep = ObjectFactory.GetInstance<LoanRepository>();
			var loan = loanRep.Get(loanid);

			List<NL_LoanFees> fl = this.m_oDB.Fill<NL_LoanFees>("NL_LoanFeesGet", new QueryParameter("LoanID", lid));

			List<LoanChargesModel> list1 = new List<LoanChargesModel>();
			List<LoanChargesModel> list2 = new List<LoanChargesModel>();

			//foreach (NL_LoanFees nlFee in fl.OrderBy(f => f.AssignTime)) {
			//	list1.Add(LoanChargesModel.CompareFromNLFee(nlFee));
			//}

			//m_oLog.Debug("NL fees");
			//list1.ForEach(f => m_oLog.Debug(f));

			//foreach (LoanCharge ch in loan.Charges.OrderBy(ff => ff.Date)) {
			//	list2.Add(LoanChargesModel.CompareFromCharges(ch));
			//}

			//m_oLog.Debug("old fees fees");
			//list2.ForEach(f => m_oLog.Debug(f));

			//var list3 = list2.Except(list1, new LoanChargeModelIdComparer()).ToList();
			//m_oLog.Debug("\n DIFFERENCES----------------");
			//list3.ForEach(f => m_oLog.Debug(f));
		}

		[Test]
		public void CancelFeeTest() {
			NL_LoanFees f = m_oDB.FillFirst<NL_LoanFees>("NL_LoanFeesGet",
				CommandSpecies.StoredProcedure,
				new QueryParameter("LoanID", 3),
				new QueryParameter("LoanFeeID", 20011));

			f.DeletedByUserID = 357;
			CancelFee s = new CancelFee(f);
			try {
				s.Execute();
			} catch (Exception e) {
				m_oLog.Debug(e);
			}
		}

		[Test]
		public void MigrateCRDecisionOfferTest() {
			MigrateCRDecisionOffer s = new MigrateCRDecisionOffer();
			try {
				s.Execute();
			} catch (Exception ex) {
				m_oLog.Error(ex);
			}
		}


		[Test]
		public void TestManualLegalDocsSyncTemplatesFiles() {
			var stra = new ManualLegalDocsSyncTemplatesFiles(@"C:\ezbob\App\PluginWeb\EzBob.Web\Areas\Customer\Views\Agreement");
			stra.Execute();
		}

	} // class TestNewLoan
} // namespace