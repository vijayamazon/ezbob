namespace EzBobTest {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using NUnit.Framework;

	[TestFixture]
	class TestNewLoan {


		[Test]
		public void TestLoader() {

			NL_Loans l = new NL_Loans();
			l.InterestRate = 3.5m;
			l.IssuedTime = DateTime.UtcNow;
			l.CreationTime = DateTime.UtcNow;
			l.LoanSourceID = 1;
			l.LoanStatusID = 2;
			l.Position = 1;

			var sss = l.ToString();

			Console.WriteLine(sss);

		}


	} // class TestNewLoan
} // namespace
