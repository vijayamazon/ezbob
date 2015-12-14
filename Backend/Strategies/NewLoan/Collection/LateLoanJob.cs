namespace Ezbob.Backend.Strategies.NewLoan.Collection {
    using ConfigManager;
    using System;

    /// <summary>
    /// Late Loan Job
    /// </summary>
    public class LateLoanJob : AStrategy
    {
        public override string Name { get { return "Late Loan Job"; } }
		public override void Execute() {
            if (!Convert.ToBoolean(CurrentValues.Instance.NewLoanRun.Value))
		        return;
		    try {
                NL_AddLog(LogType.Info, "Strategy Start", null, null, null, null);
                AStrategy strategy = new SetLateLoanStatus();
                strategy.Execute();

                strategy = new LateLoanNotification();
                strategy.Execute();

                strategy = new LateLoanCured();
                strategy.Execute();
                NL_AddLog(LogType.Info, "Strategy End", null, null, null, null);
		    } catch (Exception ex) {
                NL_AddLog(LogType.Error, "Strategy Faild", null, null, ex.ToString(), ex.StackTrace);
		    }
		}//Execute

    }// class CollectionRobot
} // namespace
