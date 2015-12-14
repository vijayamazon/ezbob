namespace Ezbob.Backend.Strategies.NewLoan.Collection {
    using System;

    /// <summary>
    /// Late Loan Job
    /// </summary>
    public class LateLoanJob : NewLoanBaseStrategy
    {
        public override string Name { get { return "Late Loan Job"; } }
        public override void NL_Execute() {
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
