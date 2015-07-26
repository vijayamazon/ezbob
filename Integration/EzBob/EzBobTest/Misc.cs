namespace EzBobTest {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Utils;
	using Ezbob.ValueIntervals;
	using NHibernate.Linq;
	using NUnit.Framework;
	using Reports;

	[TestFixture]
	class Misc {
		[Test]
		public void TestMD5() {
			var request = "/Underwriter/PaymentAccounts/PayPointCallback?customerId=24680&cardMinExpiryDate=01%2F06%2F2015&hideSteps=True&valid=true&trans_id=ac14a8f5-c9aa-4338-bbc6-769ccf7e3c08&code=A&auth_code=001350&expiry=1117&card_no=8827&customer=Mr+Eugene+O'mahoney&amount=5.00&ip=84.95.210.15&cv2avs=SECURITY+CODE+MATCH+ONLY&ezbob2015";
			var md5 = CalculateMD5Hash(request);
			var hash = "3388437329019fe5009ebbe2c0d8ce02";
			Assert.AreEqual(hash, md5);
		} // TestMD5

		protected string CalculateMD5Hash(string input) {
			// step 1, calculate MD5 hash from input
			MD5 md5 = MD5.Create();
			byte[] inputBytes = Encoding.ASCII.GetBytes(input);
			byte[] hash = md5.ComputeHash(inputBytes);

			// step 2, convert byte array to hex string
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < hash.Length; i++)
				sb.Append(hash[i].ToString("X2"));

			return sb.ToString().ToLowerInvariant();
		} // CalculateMD5Hash


		[Test]
		public void DateDiffInWeeks() {
			DateTime start = DateTime.UtcNow;
			DateTime end = start.AddMonths(3)
				.AddDays(27)
				.AddHours(23);
			int weeksa = MiscUtils.DateDiffInWeeks(start, end);
			Console.WriteLine(weeksa);
		}


		public enum BadCustomerStatuses2 {
			// name = Id in [dbo].[CustomerStatuses]
			Default = 1,
			Bad = 7,
			WriteOff = 8,
			DebtManagement = 9,
			Liquidation = 24,
			Bankruptcy = 26
		}

		[Test]
		public void TestDateIntervals() {
			DateTime now = DateTime.UtcNow;
			DateInterval stDefault = new DateInterval(new DateTime(2014, 1, 22), now);
			Console.WriteLine("GOOD DEFAULT==" + stDefault.ToString());
			IList<CustomerStatusChange> statusesHistory = new List<CustomerStatusChange>();
			statusesHistory.Add(new CustomerStatusChange() {
				OldStatus = CustomerStatus.Enabled,
				NewStatus = CustomerStatus.Enabled,
				ChangeDate = new DateTime(2014, 3, 9)
			});
			statusesHistory.Add(new CustomerStatusChange() {
				OldStatus = CustomerStatus.Enabled,
				NewStatus = CustomerStatus.Enabled,
				ChangeDate = new DateTime(2014, 6, 26)
			});
			statusesHistory.Add(new CustomerStatusChange() {
				OldStatus = CustomerStatus.Enabled,
				NewStatus = CustomerStatus.Default,
				ChangeDate = new DateTime(2014, 10, 14) // 14/10/2014
			});
			statusesHistory.Add(new CustomerStatusChange() {
				OldStatus = CustomerStatus.Default,
				NewStatus = CustomerStatus.Default,
				ChangeDate = new DateTime(2015, 2, 15) // 15/02/2015
			});
			statusesHistory.Add(new CustomerStatusChange() {
				OldStatus = CustomerStatus.Default,
				NewStatus = CustomerStatus.Enabled,
				ChangeDate = new DateTime(2015, 3, 16)
			});
			statusesHistory.Add(new CustomerStatusChange() {
				OldStatus = CustomerStatus.Enabled,
				NewStatus = CustomerStatus.WriteOff,
				ChangeDate = new DateTime(2015, 5, 3)
			});
			statusesHistory.ForEach(b => Console.WriteLine(b));
			string[] badStatusesNames = Enum.GetNames(typeof(BadCustomerStatuses2));
			List<DateInterval> bads = new List<DateInterval>();
			var badsstarts = statusesHistory.Where(s => badStatusesNames.Contains<string>(s.NewStatus.ToString())).ToList();
			badsstarts.ForEach(b => Console.WriteLine("started===================" + b));
			var badsendeds = (List<CustomerStatusChange>)statusesHistory.Where(s => badStatusesNames.Contains<string>(s.OldStatus.ToString())).ToList();
			badsendeds.Add(new CustomerStatusChange() { ChangeDate = now });
			badsendeds.ForEach(bb => Console.WriteLine("ended--------------" + bb));
			if (badsstarts.Count > 0) {
				foreach (CustomerStatusChange s in badsstarts) {
					int index = badsstarts.LastIndexOf(s);
					try {
						DateInterval i = new DateInterval(s.ChangeDate.Date, badsendeds.ElementAt(index).ChangeDate.Date);
						bads.Add(i);
					} catch (Exception e) {
						// ignored
					}
				}
			}
			bads.ForEach(bb => Console.WriteLine("interval====" + bb));
		}


		// n = A/(m-Ar);
		// total = A+A*r*((n+1)/2)
		[Test]
		public void TestLoanCalculator() {

			decimal A = 6000m;
			decimal m = 600m;
			decimal r = 0.06m;
			decimal F = 100m;

			Console.WriteLine("A: {0}, F: {1}, r: {2}, m: {3}", A, F, r, m);

			//decimal earned = A * r;

			decimal n = Math.Ceiling(A / (m - A * r));
			
			//decimal total1 = A + A * r * ((n + 1) / 2);
			//Console.WriteLine(total1);

			//decimal B = (A + F);

			decimal k = Math.Ceiling((A + F) / (m - (A + F) * r));

			Console.WriteLine("n==================" + n);

			List<decimal> paymentsN = new List<decimal>();
			decimal pi = 0m;
			decimal sum = 0m;
			for (int i = 1; i <= n; i++) {
				pi = (A + F) / k + ((n - i + 1) / n) * (A + F) * r;
				Console.WriteLine(pi);
				paymentsN.Add(pi);
				sum += pi;
			}

			Console.WriteLine(sum);Console.WriteLine();

			Console.WriteLine("k=======================" + k);
			List<decimal> paymentsK = new List<decimal>();
			decimal pik = 0m;
			decimal sumK = 0;
			for (int i = 1; i <= k; i++) {
				pik = (A + F) / k + ((n - i + 1) / n) * (A + F) * r;
				Console.WriteLine(pik);
				paymentsK.Add(pik);
				sumK += pik;
			}
			Console.WriteLine(sumK);Console.WriteLine();

			Console.WriteLine("kkk=======================" + (n + 1));
			List<decimal> paymentsNK = new List<decimal>();
			decimal pink = 0m;
			decimal sumNK = 0m;

			for (int i = 1; i <= (n + 1); i++) {
				pink = (A + F) / k + ((n - i + 1) / n) * (A + F) * r;
				Console.WriteLine(pink);
				paymentsK.Add(pink);
				sumNK += pink;
			}

			Console.WriteLine(sumNK);Console.WriteLine();

			decimal totalNK = (A + F) + (A + F) * r * ((n + 1) / 2);
			
			Console.WriteLine(totalNK);

			return;

			// new instance of loan calculator - for new schedules list
			LoanCalculatorModel calculatorModel = new LoanCalculatorModel() {
				LoanIssueTime = DateTime.UtcNow,
				LoanAmount = A,
				RepaymentCount = 25,
				MonthlyInterestRate = 0.06m,
				InterestOnlyRepayments = 0,
				RepaymentIntervalType = RepaymentIntervalTypes.Month
			};

			Console.WriteLine("Calc model for new schedules list: " + calculatorModel);

			ALoanCalculator calculator = new LegacyLoanCalculator(calculatorModel);

			Console.WriteLine();
			var scheduleswithinterests = calculator.CreateScheduleAndPlan();
		}


	} // class Misc
} // namespace