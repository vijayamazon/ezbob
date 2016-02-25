namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Utils;
	using EzBob.Models;
	using EzBob.Web.Code;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using EZBob.DatabaseLib.Repository;
	using log4net;
	using NHibernate;
	using PaymentServices.Calculators;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	[RestfullErrorHandling]
	public class LoanEditorController : Controller {
		private readonly ILoanRepository _loans;
		private readonly ICashRequestRepository _cashRequests;
		private readonly ChangeLoanDetailsModelBuilder _loanModelBuilder;
		private readonly LoanBuilder _loanBuilder;
		private readonly ILoanChangesHistoryRepository _history;
		private readonly ILoanOptionsRepository loanOptionsRepository;
		private readonly IWorkplaceContext _context;
		private readonly ServiceClient serviceClient;

		private static readonly ILog Log = LogManager.GetLogger(typeof(LoanEditorController));
		public static readonly DateTime NoLimitDate = new DateTime(2099, 1, 1);

		public LoanEditorController(
			ILoanRepository loans,
			ILoanOptionsRepository loanOptions,
			ChangeLoanDetailsModelBuilder builder,
			ICashRequestRepository cashRequests,
			ChangeLoanDetailsModelBuilder loanModelBuilder,
			LoanBuilder loanBuilder,
			ILoanChangesHistoryRepository history,
			IWorkplaceContext context,
			ILoanOptionsRepository loanOptionsRepository,
			ISession session) {
			this._loans = loans;
			this._cashRequests = cashRequests;
			this._loanModelBuilder = loanModelBuilder;
			this._loanBuilder = loanBuilder;
			this._history = history;
			this._context = context;
			this.loanOptionsRepository = loanOptionsRepository;

			this.serviceClient = new ServiceClient();
		}

		[Ajax]
		[HttpGet]
		[NoCache]
		public JsonResult Loan(int id) {
			var loan = this._loans.Get(id);

			var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();

			try {
				long nlLoanId = this.serviceClient.Instance.GetLoanByOldID(id, loan.Customer.Id, this._context.UserId).Value;
				if (nlLoanId > 0) {
					var nlModel = this.serviceClient.Instance.GetLoanState(loan.Customer.Id, nlLoanId, DateTime.UtcNow, this._context.UserId, true).Value;
					Log.InfoFormat("<<< NL_Compare: nlModel : {0} loan: {1}  >>>", nlModel, loan);
				}
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.InfoFormat("<<< NL_Compare Fail at: {0}, err: {1}", Environment.StackTrace, ex.Message);
			}

			var model = this._loanModelBuilder.BuildModel(loan);

			RescheduleSetmodel(id, model);

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult RecalculateCR(EditLoanDetailsModel model) {
			var cr = this._cashRequests.Get(model.CashRequestId);
			return Json(RecalculateModel(model, cr, model.Date));
		}

		/// <summary>
		/// called on deleting/adding one installment etc.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult Recalculate(int id, EditLoanDetailsModel model) {
			var cr = this._loans.Get(id).CashRequest;
			return Json(RecalculateModel(model, cr, DateTime.UtcNow));
		}

		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonResult LoanCR(long id) {
			var cr = this._cashRequests.Get(id);
			var amount = cr.ApprovedSum();
			var loan = this._loanBuilder.CreateLoan(cr, amount, DateTime.UtcNow);
			var model = this._loanModelBuilder.BuildModel(loan);

			RescheduleSetmodel(loan.Id, model);

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult LoanCR(EditLoanDetailsModel model) {
			var cr = this._cashRequests.Get(model.CashRequestId);

			model = RecalculateModel(model, cr, model.Date);

			cr.LoanTemplate = model.ToJSON();

			RescheduleSetmodel(model.Id, model);

			return Json(model);
		}

		/// <summary>
		/// saving all modifications - deleting/adding of installments etc.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "OldEditLoanDetails")]
		public JsonResult Loan(EditLoanDetailsModel model) {
			var loan = this._loans.Get(model.Id);

			var historyItem = new LoanChangesHistory {
				Data = this._loanModelBuilder.BuildModel(loan).ToJSON(),
				Date = DateTime.UtcNow,
				Loan = loan,
				User = this._context.User
			};
			this._history.Save(historyItem);

			//  remove/add fees/installment/pp transaction

			// model - from UI (modified), loan - from DB
			this._loanModelBuilder.UpdateLoan(model, loan);

			Log.DebugFormat("model {0} =================== loan {1} ", model, loan);

			//TODO update loan (apply all modifications)
			Log.DebugFormat("apply loan modifications for customer {0}", loan.Customer.Id);

			var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();

			// NL cancel payment
			try {
				long nlLoanId = this.serviceClient.Instance.GetLoanByOldID(model.Id, loan.Customer.Id, this._context.UserId).Value;
				if (nlLoanId > 0) {
					NL_Model nlModel = this.serviceClient.Instance.GetLoanState(loan.Customer.Id, nlLoanId, DateTime.UtcNow, this._context.UserId, false).Value;
					Log.InfoFormat("nlModel : {0} loan: {1}  >>>", nlModel, loan);
					foreach (PaypointTransaction t in loan.TransactionsWithPaypoint.Where(t => t.Cancelled)) {
						foreach (NL_Payments zz in nlModel.Loan.Payments) {

							if (zz.Amount == t.CancelledAmount && zz.PaymentTime.Date.Equals(t.PostDate.Date)){ //&& (zz.PaypointTransactions.FirstOrDefault(x => x.PaypointUniqueID == t.PaypointId && x.IP == t.IP) != null)) 
								zz.DeletedByUserID = this._context.UserId;
								zz.DeletionTime = DateTime.UtcNow;
								zz.Notes = t.Description;
								zz.PaymentStatusID = (int)NLPaymentStatuses.WrongPayment;
								Log.InfoFormat("cancelling NL payment {0}", zz);
								this.serviceClient.Instance.CancelPayment(this._context.UserId, zz, loan.Customer.Id);
							}
						}
					}
				}

				// ReSharper disable once CatchAllClause
			} catch (Exception fex) {
				Log.InfoFormat("Fail to cancel NL payment(s). {0}, err: {1}", Environment.StackTrace, fex.Message);
			}

			RescheduleSetmodel(loan.Id, model);

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		private void RescheduleSetmodel(int loanID, EditLoanDetailsModel model) {

			var	loan = this._loans.Get(loanID);
			model.Id = loanID;

			model.Options = this.loanOptionsRepository.GetByLoanId(model.Id) ?? LoanOptions.GetDefault(model.Id);

			Log.DebugFormat("RescheduleSetmodel: loanID: {2} model {0}, loan: {1}", model, loan, loanID);

			ReschedulingArgument renewModel = new ReschedulingArgument();
			renewModel.LoanType = loan.GetType().AssemblyQualifiedName;
			renewModel.LoanID = loanID;
			renewModel.SaveToDB = false;
			renewModel.ReschedulingDate = DateTime.UtcNow;
			renewModel.ReschedulingRepaymentIntervalType = DbConstants.RepaymentIntervalTypes.Month;
			renewModel.RescheduleIn = true;

			try {
				ReschedulingActionResult result = this.serviceClient.Instance.RescheduleLoan(this._context.User.Id, loan.Customer.Id, renewModel);
				model.ReResultIn = result.Value;
				//Log.Debug(string.Format("IN=={0}, {1}", renewModel, result.Value));
				// ReSharper disable once CatchAllClause
			} catch (Exception editex) {
				Log.Error(editex);
			}

			renewModel.RescheduleIn = false;
			renewModel.PaymentPerInterval = 0m;
			try {
				ReschedulingActionResult result = this.serviceClient.Instance.RescheduleLoan(this._context.User.Id, loan.Customer.Id, renewModel);
				model.ReResultOut = result.Value;
				//Log.Debug(string.Format("OUT=={0}, {1}", renewModel, result.Value));
				// ReSharper disable once CatchAllClause
			} catch (Exception editex) {
				Log.Error(editex);
			}
		}

		private EditLoanDetailsModel RecalculateModel(EditLoanDetailsModel model, CashRequest cr, DateTime now) {
			model.Validate();

			if (model.HasErrors)
				return model;

			var loan = this._loanModelBuilder.CreateLoan(model);
			loan.LoanType = cr.LoanType;
			loan.CashRequest = cr;

			RescheduleSetmodel(model.Id, model);

			try {
				var calc = new LoanRepaymentScheduleCalculator(loan, now, CurrentValues.Instance.AmountToChargeFrom);
				calc.GetState();

				//try {
				//	long nlLoanId = this.serviceClient.Instance.GetLoanByOldID(loan.Id, cr.Customer.Id, this._context.UserId).Value;
				//	if (nlLoanId > 0) {
				//		var nlModel = this.serviceClient.Instance.GetLoanState(loan.Customer.Id, nlLoanId, now, this._context.UserId, true).Value;
				//		Log.InfoFormat("<<< NL_Compare: nlModel : {0} loan: {1}  >>>", nlModel, loan);
				//	}
				//	// ReSharper disable once CatchAllClause
				//} catch (Exception ex) {
				//	Log.InfoFormat("<<< NL_Compare Fail at: {0}, err: {1}", Environment.StackTrace, ex.Message);
				//}

			} catch (Exception e) {
				model.Errors.Add(e.Message);
				return model;
			}

			return this._loanModelBuilder.BuildModel(loan);
		}

		[Ajax]
		[HttpPost]
		public JsonResult RescheduleLoan(int loanID,
			DbConstants.RepaymentIntervalTypes? intervalType,  // month/week
			decimal? AmountPerInterval, // for "out" reschedule
			bool? rescheduleIn,
			DateTime reschedulingDate,
			bool save = false,
			bool stopFutureInterest = false) {
			ReschedulingActionResult result = null;
			try {
				Loan loan = this._loans.Get(loanID);

				if (rescheduleIn != null) {
					ReschedulingArgument reModel = new ReschedulingArgument();
					reModel.LoanType = loan.GetType().AssemblyQualifiedName;
					reModel.LoanID = loanID;
					reModel.SaveToDB = save;
					reModel.ReschedulingDate = reschedulingDate;
					if (intervalType != null)
						reModel.ReschedulingRepaymentIntervalType = (DbConstants.RepaymentIntervalTypes)intervalType;
					reModel.RescheduleIn = (bool)rescheduleIn;
					reModel.StopFutureInterest = stopFutureInterest;

					if (reModel.RescheduleIn == false) // "out"
						reModel.PaymentPerInterval = AmountPerInterval;

					// re strategy
					result = this.serviceClient.Instance.RescheduleLoan(this._context.User.Id, loan.Customer.Id, reModel);

					Log.Debug(string.Format("RescheduleLoanSubmitted: {0}, {1}", reModel, result.Value));
				}
			} catch (Exception editex) {
				Log.Error("rescheduling editor EXCEPTION: " + editex);
			}
			return result == null ? null : Json(result.Value);
		}

		/// <exception cref="NotImplementedException">Always.</exception>
		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "LoanOptions")]
		public JsonResult SaveLateFeeOption(int id) {
			DateTime? lateFeeStartDate = Convert.ToDateTime(HttpContext.Request.QueryString["lateFeeStartDate"]);

			string lateFeeEndDateStr = HttpContext.Request.QueryString["lateFeeEndDate"];

			DateTime? lateFeeEndDate = string.IsNullOrEmpty(lateFeeEndDateStr) ? NoLimitDate : Convert.ToDateTime(lateFeeEndDateStr);

			LoanOptions options = this.loanOptionsRepository.GetByLoanId(id) ?? LoanOptions.GetDefault(id);

			EditLoanDetailsModel model = this._loanModelBuilder.BuildModel(this._loans.Get(id));

			if (options.StopLateFeeFromDate != null && options.StopLateFeeToDate != null) {
				// to.Subtract(from)
				if (options.StopLateFeeToDate.Value.Subtract(options.StopLateFeeFromDate.Value).Days < 0) {
					model.Errors.Add("'Until date must be greater then From date");
					RescheduleSetmodel(id, model);
					return Json(model);
				}
			}

			options.AutoLateFees = true;
			options.StopLateFeeFromDate = lateFeeStartDate;
			options.StopLateFeeToDate = lateFeeEndDate;

			this.loanOptionsRepository.SaveOrUpdate(options);

			var PropertiesUpdateList = new List<String>() {
		        "StopLateFeeFromDate",
		        "StopLateFeeToDate"
		    };

			NL_SaveLoanOptions(options, PropertiesUpdateList);

			model.Options = this.loanOptionsRepository.GetByLoanId(id);
			RescheduleSetmodel(id, model);
			return Json(model);
		} // SaveLateFeeOption

		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "LoanOptions")]
		public JsonResult RemoveLateFeeOption(int id) {
			LoanOptions options = this.loanOptionsRepository.GetByLoanId(id) ?? LoanOptions.GetDefault(id);

			options.AutoLateFees = false;
			options.StopLateFeeFromDate = null;
			options.StopLateFeeToDate = null;

			this.loanOptionsRepository.SaveOrUpdate(options);

			var PropertiesUpdateList = new List<String>() {
		        "StopLateFeeFromDate",
		        "StopLateFeeToDate"
		    };
			NL_SaveLoanOptions(options, PropertiesUpdateList);


			EditLoanDetailsModel model = this._loanModelBuilder.BuildModel(this._loans.Get(id));
			model.Options = this.loanOptionsRepository.GetByLoanId(id);

			RescheduleSetmodel(id, model);

			return Json(model);
		} // RemoveLateFeeOption

		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "LoanOptions")]
		public JsonResult SaveAutoChargesOption(int id, int schedultItemId) {
			DateTime now = DateTime.UtcNow;
			LoanOptions options = this.loanOptionsRepository.GetByLoanId(id) ?? LoanOptions.GetDefault(id);
			var loan = this._loans.Get(id);

			options.AutoPayment = false;
			options.StopAutoChargeDate = null;

			if (schedultItemId > -1) {
				var loanScheduleItem = loan.Schedule.Where(x => x.Date > now).FirstOrDefault(x => x.Id == schedultItemId);
				if (loanScheduleItem != null) {
					options.StopAutoChargeDate = loanScheduleItem.Date;
				} else {
					Log.ErrorFormat("The date selected from DDL is not valid");
				}
			}

			this.loanOptionsRepository.SaveOrUpdate(options);

			var PropertiesUpdateList = new List<String>() {
		        "StopAutoChargeDate",
		    };
			NL_SaveLoanOptions(options, PropertiesUpdateList);

			EditLoanDetailsModel model = this._loanModelBuilder.BuildModel(this._loans.Get(id));
			model.Options = this.loanOptionsRepository.GetByLoanId(id);
			RescheduleSetmodel(id, model);
			return Json(model);
		} // SaveAutoChargesOption

		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "LoanOptions")]
		public JsonResult RemoveAutoChargesOption(int id) {
			LoanOptions options = this.loanOptionsRepository.GetByLoanId(id);

			options.AutoPayment = true;
			options.StopAutoChargeDate = null;
			this.loanOptionsRepository.SaveOrUpdate(options);

			var PropertiesUpdateList = new List<String>() {
		        "StopAutoChargeDate",
		    };

			NL_SaveLoanOptions(options, PropertiesUpdateList);

			EditLoanDetailsModel model = this._loanModelBuilder.BuildModel(this._loans.Get(id));
			model.Options = this.loanOptionsRepository.GetByLoanId(id);
			RescheduleSetmodel(id, model);
			return Json(model);
		} // RemoveAutoChargesOption

		/// <exception cref="NotImplementedException">Always.</exception>
		[Ajax]
		[HttpPost]
		public JsonResult SaveFreezeInterval(int id) {

			DateTime? freezeStartDate = Convert.ToDateTime(HttpContext.Request.QueryString["startdate"]);

			string freezeEndDateStr = HttpContext.Request.QueryString["enddate"];
			DateTime? freezeEndDate = string.IsNullOrEmpty(freezeEndDateStr) ? NoLimitDate : Convert.ToDateTime(freezeEndDateStr);

			EditLoanDetailsModel model = this._loanModelBuilder.BuildModel(this._loans.Get(id));

			if (freezeStartDate > freezeEndDate) {

				model.Errors.Add("Until date must be greater then From date");
				RescheduleSetmodel(id, model);
				return Json(model);
			}

			Loan loan = this._loans.Get(id);
			var loan1 = loan;
			new Transactional(() => {
				loan1.InterestFreeze.Add(new LoanInterestFreeze {
					Loan = loan1,
					StartDate = freezeStartDate,
					EndDate = freezeEndDate,
					InterestRate = 0,
					ActivationDate = DateTime.UtcNow,
					DeactivationDate = null
				});

				this._loans.SaveOrUpdate(loan1);
			}).Execute();

			SaveLoanInterestFreeze(loan.InterestFreeze.Last(), id);

			loan = this._loans.Get(id);
			model = this._loanModelBuilder.BuildModel(loan);
			model.Options = this.loanOptionsRepository.GetByLoanId(id);

			RescheduleSetmodel(loan.Id, model);

			return Json(model);
		} //SaveFreezeInterval

		[Ajax]
		[HttpPost]
		public JsonResult RemoveFreezeInterval(int id, int intervalid) {
			Loan loan = this._loans.Get(id);
			LoanInterestFreeze lif = loan.InterestFreeze.FirstOrDefault(v => v.Id == intervalid);

			var loan1 = loan;
			new Transactional(() => {

				if (lif != null)
					lif.DeactivationDate = DateTime.UtcNow;
				this._loans.SaveOrUpdate(loan1);

			}).Execute();

			DeactivateLoanInterestFreeze(lif);

			Log.DebugFormat("remove freeze interest for customer {0}", loan.Customer.Id);

			loan = this._loans.Get(id);

			var calc = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();

			try {
				long nlLoanId = this.serviceClient.Instance.GetLoanByOldID(id, loan.Customer.Id, this._context.UserId).Value;
				if (nlLoanId > 0) {
					var nlModel = this.serviceClient.Instance.GetLoanState(loan.Customer.Id, nlLoanId, DateTime.UtcNow, this._context.UserId, true).Value;
					Log.InfoFormat("<<< NL_Compare: {0}\n===============loan: {1}  >>>", nlModel, loan);
				}
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.InfoFormat("<<< NL_Compare fail at: {0}, err: {1}", Environment.StackTrace, ex.Message);
			}

			EditLoanDetailsModel model = this._loanModelBuilder.BuildModel(loan);
			model.Options = this.loanOptionsRepository.GetByLoanId(id) ?? LoanOptions.GetDefault(id);

			RescheduleSetmodel(id, model);

			return Json(model);
		} // RemoveFreezeInterval


		private void DeactivateLoanInterestFreeze(LoanInterestFreeze loanInterestFreeze) {
			int customerId = this._loans.Get(loanInterestFreeze.Loan.Id).Customer.Id;

			long newLoanId = this.serviceClient.Instance.GetLoanByOldID(loanInterestFreeze.Loan.Id, customerId, this._context.UserId).Value;
			if (newLoanId < 0)
				return;
			NL_LoanInterestFreeze nlLoanInterestFreeze = new NL_LoanInterestFreeze() {
				OldID = loanInterestFreeze.Id,
				DeactivationDate = loanInterestFreeze.DeactivationDate,
				LoanID = newLoanId,
				AssignedByUserID = this._context.UserId,
				DeletedByUserID = null,
			};
			this.serviceClient.Instance.DeactivateLoanInterestFreeze(this._context.UserId,
																					  customerId,
																					  nlLoanInterestFreeze);
		}


		private void SaveLoanInterestFreeze(LoanInterestFreeze loanInterestFreeze, int loanID) {

			int customerId = this._loans.Get(loanInterestFreeze.Loan.Id).Customer.Id;

			long newLoanId = this.serviceClient.Instance.GetLoanByOldID(loanID, customerId, this._context.UserId).Value;
			if (newLoanId < 0)
				return;
			NL_LoanInterestFreeze nlLoanInterestFreeze = new NL_LoanInterestFreeze() {
				StartDate = loanInterestFreeze.StartDate,
				OldID = loanInterestFreeze.Id,
				ActivationDate = loanInterestFreeze.ActivationDate,
				DeactivationDate = loanInterestFreeze.DeactivationDate,
				EndDate = loanInterestFreeze.EndDate,
				InterestRate = loanInterestFreeze.InterestRate,
				LoanID = newLoanId,
				AssignedByUserID = this._context.UserId,
				DeletedByUserID = null,
			};
			this.serviceClient.Instance.AddLoanInterestFreeze(this._context.UserId, customerId, nlLoanInterestFreeze);
		}

		private void NL_SaveLoanOptions(LoanOptions options, List<String> PropertiesUpdateList) {

			int customerId = this._loans.Get(options.LoanId).Customer.Id;

			//NL Loan Options
			NL_LoanOptions nlOptions = new NL_LoanOptions() {
				LoanID = options.LoanId,
				CaisAccountStatus = options.CaisAccountStatus,
				EmailSendingAllowed = options.EmailSendingAllowed,
				LatePaymentNotification = options.LatePaymentNotification,
				LoanOptionsID = options.Id,
				MailSendingAllowed = options.MailSendingAllowed,
				ManualCaisFlag = options.ManualCaisFlag,
				PartialAutoCharging = options.ReductionFee,
				SmsSendingAllowed = options.SmsSendingAllowed,
				StopAutoChargeDate = MiscUtils.NL_GetStopAutoChargeDate(options.AutoPayment, options.StopAutoChargeDate),
				StopLateFeeFromDate = MiscUtils.NL_GetLateFeeDates(options.AutoLateFees, options.StopLateFeeFromDate, options.StopLateFeeToDate).Item1,
				StopLateFeeToDate = MiscUtils.NL_GetLateFeeDates(options.AutoLateFees, options.StopLateFeeFromDate, options.StopLateFeeToDate).Item2,
				UserID = this._context.UserId,
				InsertDate = DateTime.Now,
				IsActive = true,
				Notes = "From Loan Editor Controller",
			};

			var nlStrategy = this.serviceClient.Instance.AddLoanOptions(this._context.UserId, customerId, nlOptions, options.LoanId, PropertiesUpdateList.ToArray());
			Log.DebugFormat("NL LoanOptions save: LoanOptionsID: {0}, Error: {1}", nlStrategy.Value, nlStrategy.Error);
		}

		[Ajax]
		[HttpPost]
		//[Transactional]
		public void NL_SyncFees(int id) {

			Log.DebugFormat("sync NL fees for old loan {0}", id);

			var loan = this._loans.Get(id);

			try {
				long nlLoanId = this.serviceClient.Instance.GetLoanByOldID(id, loan.Customer.Id, this._context.UserId).Value;

				if (nlLoanId > 0) {

					NL_Model nlModel = this.serviceClient.Instance.GetLoanState(loan.Customer.Id, nlLoanId, DateTime.UtcNow, this._context.UserId, false).Value;
					Log.InfoFormat("nlModel : {0} loan: {1}  >>>", nlModel, loan);

					// new/edit fee
					foreach (LoanCharge ch in loan.Charges.OrderBy(ff => ff.Date)) {
						NL_LoanFees f = nlModel.Loan.Fees.FirstOrDefault(ff => ff.OldFeeID == ch.Id);
						if (f == null) {
							f = new NL_LoanFees() {
								AssignedByUserID = this._context.UserId,
								LoanFeeTypeID = (int)NL_Model.ConfVarToNLFeeType(ch.ChargesType),
								LoanID = nlLoanId,
								OldFeeID = ch.Id, // update in strategy after this [Transactional] 
								Amount = ch.Amount,
								AssignTime = ch.Date.Date,
								Notes = !string.IsNullOrEmpty(ch.Description) ? ch.Description : "add fee from edit loan"
							};
							this.serviceClient.Instance.SaveFee(this._context.UserId, loan.Customer.Id, f);

							// fee type is editable + modified (amount/assign date/notes)
						} else if (Array.Exists<NLFeeTypes>(NL_Model.NLFeeTypesEditable, element => element == (NLFeeTypes)f.LoanFeeTypeID) && (f.Amount != ch.Amount || f.AssignTime.Date != ch.Date.Date || f.Notes != ch.Description)) {
							f.UpdatedByUserID = this._context.UserId;
							f.Amount = ch.Amount;
							f.AssignTime = ch.Date.Date;
							f.Notes = !string.IsNullOrEmpty(ch.Description) ? ch.Description : "add fee from edit loan";
							this.serviceClient.Instance.SaveFee(this._context.UserId, loan.Customer.Id, f);
						}
					}

					//	removed fee
					foreach (NL_LoanFees f in nlModel.Loan.Fees.OrderBy(ff => ff.AssignTime)) {
						var charge = loan.Charges.FirstOrDefault(c => c.Id == f.LoanFeeID);
						if (charge == null) {
							f.DeletedByUserID = this._context.UserId;
							this.serviceClient.Instance.CancelFee(this._context.UserId, loan.Customer.Id, f);
						}
					}
				}
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.InfoFormat("Fail to sync NL loan fees. {0}, err: {1}", Environment.StackTrace, ex.Message);
			}
		}

	}
}