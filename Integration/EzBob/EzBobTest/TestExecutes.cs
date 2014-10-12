namespace EzBobTest 
{
	using System;
	using System.Data;
	using System.Diagnostics;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using JetBrains.Annotations;
	using NUnit.Framework;
	using Ezbob.Utils.Security;

	[TestFixture]
	class TestExecutes : BaseTestFixtue
	{
		private Password pwd;

		[SetUp]
		public void SetUp()
		{
			pwd = new Password("bbb");
		}

		[Test]
		public void TestException1()
		{
			try
			{
				BrokerProperties properties = null; // Not initialized
				var sp = new SpBrokerLogin(m_oDB, m_oLog)
				{
					Email = "testchange5@t.t",
					Password = pwd.Primary,
				};

				sp.FillFirst(properties);
			}
			catch (Exception e)
			{
				m_oLog.Fatal("Can you figure it out: {0}", e); //Object to traverse not specified. Parameter name: oInstance
			}

			try
			{
				BrokerProperties properties = null; // Not initialized
				DataTable dt = m_oDB.ExecuteReader("BrokerLogin", CommandSpecies.StoredProcedure,
												new QueryParameter("Email", "testchange5@t.t"),
												new QueryParameter("Password", pwd.Primary));
				if (dt.Rows.Count == 1)
				{
					var sr = new SafeReader(dt.Rows[0]);
					properties.BrokerID = sr["BrokerID"];
					properties.BrokerName = sr["BrokerName"];
					properties.BrokerRegNum = sr["BrokerRegNum"];
					properties.ContactName = sr["ContactName"];
					properties.ContactEmail = sr["ContactEmail"];
					properties.ContactMobile = sr["ContactMobile"];
					properties.ContactOtherPhone = sr["ContactOtherPhone"];
					properties.SourceRef = sr["SourceRef"];
					properties.BrokerWebSiteUrl = sr["BrokerWebSiteUrl"];
					properties.SignedTermsID = sr["SignedTermsID"];
					properties.SignedTextID = sr["SignedTextID"];
					properties.CurrentTermsID = sr["CurrentTermsID"];
					properties.CurrentTextID = sr["CurrentTextID"];
					properties.CurrentTerms = sr["CurrentTerms"];
				}
			}
			catch (Exception e)
			{
				m_oLog.Fatal("Now it is obvious: {0}", e); // We even get a warning from the resharper
			}
		}

		[Test]
		public void TestComparePerformance() 
		{
			Stopwatch sw = Stopwatch.StartNew();
			BrokerProperties properties;
			int iter = 10;
			for (int i = 0; i < iter; i++)
			{
				properties = new BrokerProperties(); // When not initialized error is unclear...
				var sp = new SpBrokerLogin(m_oDB, m_oLog)
					{
						Email = "testchange5@t.t",
						Password = pwd.Primary,
					};

				sp.FillFirst(properties);
			}
			sw.Stop();
			long numOfMs1 = sw.ElapsedMilliseconds;
			m_oLog.Fatal("Elapsed {0}", numOfMs1);

			properties = new BrokerProperties();
			sw.Restart();
			for (int i = 0; i < iter; i++)
			{
				DataTable dt = m_oDB.ExecuteReader("BrokerLogin", CommandSpecies.StoredProcedure,
												new QueryParameter("Email", "testchange5@t.t"),
												new QueryParameter("Password", pwd.Primary));
				if (dt.Rows.Count == 1)
				{
					var sr = new SafeReader(dt.Rows[0]);
					properties.BrokerID = sr["BrokerID"];
					properties.BrokerName = sr["BrokerName"];
					properties.BrokerRegNum = sr["BrokerRegNum"];
					properties.ContactName = sr["ContactName"];
					properties.ContactEmail = sr["ContactEmail"];
					properties.ContactMobile = sr["ContactMobile"];
					properties.ContactOtherPhone = sr["ContactOtherPhone"];
					properties.SourceRef = sr["SourceRef"];
					properties.BrokerWebSiteUrl = sr["BrokerWebSiteUrl"];
					properties.SignedTermsID = sr["SignedTermsID"];
					properties.SignedTextID = sr["SignedTextID"];
					properties.CurrentTermsID = sr["CurrentTermsID"];
					properties.CurrentTextID = sr["CurrentTextID"];
					properties.CurrentTerms = sr["CurrentTerms"];
				}
			}
			sw.Stop();
			long numOfMs2 = sw.ElapsedMilliseconds;
			m_oLog.Fatal("Elapsed {0}", numOfMs2);

			long diff = numOfMs1 - numOfMs2;
			Assert.IsTrue(diff > 0);
		}


		private class SpBrokerLogin : AStoredProc
		{
			#region constructor

			public SpBrokerLogin(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters()
			{
				Email = MiscUtils.ValidateStringArg(Email, "Email");
				Password = MiscUtils.ValidateStringArg(m_sPassword, "Password");

				return true;
			} // HasValidParameters

			#endregion method HasValidParameters

			#region property Email

			[UsedImplicitly]
			public string Email { get; set; }

			#endregion property Email

			#region property Password

			public string Password
			{
				[UsedImplicitly]
				get { return SecurityUtils.HashPassword(Email, m_sPassword); }
				set { m_sPassword = value; }
			} // Password

			private string m_sPassword;

			#endregion property Password
		} // class SpBrokerLogin
	} // class TestDbConnection
} // namespace
