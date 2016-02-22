namespace EzBobTest {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DbConstants;
	using Ezbob.Backend.Models.Alibaba;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Utils;
	using Ezbob.Utils.Extensions;
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

		[Test]
		public void DateDiffInWeeks2() {
			//09/24/2016
			DateTime start = new DateTime(2015, 8, 11);
			Console.WriteLine(start);
			DateTime end = new DateTime(2016, 9, 24);
			Console.WriteLine(end);
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





		[Test]
		public void DateDiffInMonth() {
			// 10/11/2016
			DateTime start = new DateTime(2015, 8, 11);
			Console.WriteLine(start);
			DateTime end = new DateTime(2016, 10, 11);
			Console.WriteLine(end);
			int dif = MiscUtils.DateDiffInMonths(start, end);
			Console.WriteLine(dif);
		}

		[Test]
		public void DateDiffs() {
			// 2/11/2016
			DateTime to = new DateTime(2015, 8, 9);
			// 31/10/2016
			DateTime  from = new DateTime(2015, 8, 13);
			Console.WriteLine(from);
			Console.WriteLine(to);
			TimeSpan ts = to.Subtract(from);
			double totalDays = ts.TotalDays;
			int dDays = ts.Days;
			Console.WriteLine("totalDays: {0}, dDays: {1}", totalDays, dDays);
			return;
			// 12/10/2016
			//end = new DateTime(2016, 10, 12);
			//Console.WriteLine(end);
			//ts = start.Subtract(end);
			// totalDays = ts.TotalDays;
			// dDays = ts.Days;
			// Console.WriteLine("totalDays: {0}, dDays: {1}", totalDays, dDays);
		}

		[Test]
		public void TestDateTimeFormat() {
			CustomerDataSharing cds = new CustomerDataSharing();
			DateTime xx = new DateTime(2000, 03, 12, 12, 12, 12, 122);
			Console.WriteLine(xx);
			cds.applicationDate = xx;
			Console.WriteLine(cds.applicationDate.ToString());
		}

		[Test]
		public void TestEnum() {
			string xx = Enum.GetName(typeof(RepaymentIntervalTypes), 3);
			//Console.WriteLine(xx);
			var yy = Enum.Parse(typeof(RepaymentIntervalTypes), xx);
			Console.WriteLine(yy.DescriptionAttr());
			string newStatus = "Done";
			var nlStatus = Enum.GetNames(typeof(NLPacnetTransactionStatuses)).FirstOrDefault(s => s.Equals(newStatus));
			Console.WriteLine(nlStatus);
			try {
				var val = (int)Enum.Parse(typeof(NLPacnetTransactionStatuses), nlStatus);
				Console.WriteLine(val);
			} catch (OverflowException overflowException) {
				Console.WriteLine(overflowException);
			}
		}


		[Test]
		public void TestEnumGetName() {
			//Console.WriteLine(Enum.GetName(typeof(NLScheduleStatuses), 3));
			//Console.WriteLine(NLScheduleStatuses.Paid.DescriptionAttr());
			//Console.WriteLine(Enum.Parse(typeof(NLScheduleStatuses), Enum.GetName(typeof(NLScheduleStatuses), 3).ToString()).DescriptionAttr());
			Console.WriteLine(Enum.GetName(typeof(NLLoanTypes), 2));
			Console.WriteLine(NLLoanTypes.HalfWayLoanType.ToString());
			Console.WriteLine(Enum.GetName(typeof(NLLoanTypes), 2) == NLLoanTypes.HalfWayLoanType.ToString());
		}


		[Test]
		public void TestPrintFormat() {
			/*DateTime? xxx = null;
			//Console.WriteLine("{0:G}", NLFeeTypes.SetupFee);
			Console.WriteLine("{0:d}", DateTime.UtcNow);
			xxx = DateTime.UtcNow;
			Console.WriteLine("{0,-11:d}", xxx);
			Console.WriteLine("{0:F}", 445.6667m);*/

			Console.WriteLine("{0,-21} {1,-14} {2,-9} {3,-13:F} {4,-12:F} {5,-18:d} ", 66, 67, 10050, 0.000000, 0.000000, 0.000000m);

			/*	{0,-21} {1,-14} {2,-9} {3,-13:F} {4,-12:F} {5,-18:d} {6,-17:d} {7,-8} 
	66
	67
	10050
	0.000000
	0.000000
	0.000000
	0.000000
	False*/
		}

		/// <exception cref="InvalidCastException"><paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		[Test]
		public void ArrayListTest() {
			int[] myIntArray = new int[3];
			myIntArray.SetValue(1, 0);
			myIntArray.SetValue(100, 2);
			myIntArray.ForEach(a => Console.WriteLine(a));
		}

		[Test]
		public void PDateSetTest() {
			List<long> idList = new List<long>();
			idList.Add(35);
			idList.Add(36);
			idList.Add(39);
			idList.Add(37);

			List<DateTime> dateList = new List<DateTime>();
			dateList.Add(new DateTime(2015, 10, 25));
			dateList.Add(new DateTime(2015, 10, 25));
			dateList.Add(new DateTime(2015, 12, 10));
			dateList.Add(new DateTime(2015, 12, 20));
		}


		[Test]
		public void TestEnumByString() {
			string s = "bank transfer";
			Console.WriteLine(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s));
			//NLLoanTransactionMethods e = (NLLoanTransactionMethods)Enum.Parse(typeof(NLLoanTransactionMethods), s);
			//Console.WriteLine(e);
		}

		[Test]
		public void IsInTest() {
			NL_LoanFees f = new NL_LoanFees() { LoanFeeTypeID = 8 };
			Console.WriteLine(Array.Exists<NLFeeTypes>(NL_Model.NLFeeTypesEditable, element => element == (NLFeeTypes)f.LoanFeeTypeID));
		}

		[Test]
		public void TomeFormatTest() {
			// 2014 January 09, 16:53:32.000
			DateTime d = new DateTime(2014, 1, 9, 16, 53, 32);
			Console.WriteLine(d.ToShortTimeString());
		}

	} // class Misc
} // namespace