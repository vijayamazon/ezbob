using System;
using System.Globalization;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using Moq;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
	using ConfigManager;

	/// <summary>
	/// A base class for all tests cases that tests loan payment logic
	/// </summary>
	public class LoanPaymentsTestBase
	{
		protected LoanPaymentFacade _facade;
		protected Loan _loan;
		protected LoanScheduleCalculator _calculator;
		private Customer _customer;
		//protected IConfigurationVariablesRepository _configurationVariablesRepository;

		[SetUp]
		public void Init()
		{
			Assert.AreEqual(CurrentValues.Instance.AmountToChargeFrom, GetAmountToChargeFrom());

			_loan = new Loan();
			// _configurationVariablesRepository = config.Object;
			_facade = new LoanPaymentFacade(null, null);
			_calculator = new LoanScheduleCalculator() { Interest = 0.06M };

			_customer = new Customer();

			SetUp();
		}

		protected virtual int GetAmountToChargeFrom()
		{
			return 0;
		}

		protected virtual void SetUp()
		{

		}

		protected void MakePayment(decimal amount, DateTime date)
		{
			Console.WriteLine("Making payment {0} on {1}", amount, date);
			_facade.PayLoan(_loan, "", amount, "", date);
			Console.WriteLine(_loan);
		}

		protected LoanScheduleItem GetStateAt(Loan loan, DateTime date)
		{
			return _facade.GetStateAt(loan, date);
		}

		protected static DateTime Parse(string date)
		{
			return DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss.000", CultureInfo.InvariantCulture);
		}

		protected void CreateHalfWayLoan(DateTime startDate, int amount)
		{
			_calculator.Term = 6;
			_loan.LoanType = new HalfWayLoanType();
			CreateLoan(startDate, amount);
		}

		protected void CreateLoan(DateTime startDate, int amount)
		{
			_calculator.Calculate(amount, _loan, startDate);
			_customer.Loans.Add(_loan);
			var cr = new CashRequest();
			_customer.CashRequests.Add(cr);
			_loan.Customer = _customer;
		}
	}
}