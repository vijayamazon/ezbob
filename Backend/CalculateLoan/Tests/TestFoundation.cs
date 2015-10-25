namespace Ezbob.Backend.CalculateLoan.Tests {
	using ConfigManager;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Database;
	using Ezbob.Database.Pool;
	using Ezbob.Logger;
	using NUnit.Framework;

	[TestFixture]
	public class TestFoundation {
		[SetUp]
		public void Init() {
			var oLog4NetCfg = new Log4Net().Init();

			Env = oLog4NetCfg.Environment;

			Log = new ConsoleLog(new SafeILog(this));

			DB = new SqlConnection(oLog4NetCfg.Environment, Log);

			ConfigManager.CurrentValues.Init(DB, Log);
			DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
			AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);
			
			Library.Initialize(Env, DB, Log);
		} // Init

		protected AConnection DB { get; private set; }
		protected ASafeLog Log { get; private set; }
		protected Ezbob.Context.Environment Env { get; private set; }
	} // class TestFoundation
} // namespace
