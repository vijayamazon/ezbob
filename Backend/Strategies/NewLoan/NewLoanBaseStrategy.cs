namespace Ezbob.Backend.Strategies.NewLoan {
    using System;
    using ConfigManager;
    using Ezbob.Database;
    using Ezbob.Utils;
    using Newtonsoft.Json;

    public abstract class NewLoanBaseStrategy : AStrategy {

        public bool IsNewLoanRunStrategy {
            get { return Convert.ToBoolean(CurrentValues.Instance.NewLoanRun.Value); }
        }
        public abstract void NL_Execute();
        public abstract override string Name { get; }

        public override void Execute() {
            if (!IsNewLoanRunStrategy)
                return;
            NL_Execute();
        }
        public void NL_AddLog(LogType logType,
                                string description,
                                object args,
                                object result,
                                string exception,
                                string stacktrace) {
            var sevirity = GetLogSection(logType);

            var nlLog = new NL_Log() {
                Exception = exception,
                Description = description,
                Referrer = Name,
                TimeStamp = DateTime.Now,
                Result = JsonConvert.SerializeObject(result, MiscUtils.GetJsonDBFormat()),
                Sevirity = sevirity,
                Stacktrace = stacktrace,
                Args = JsonConvert.SerializeObject(args, MiscUtils.GetJsonDBFormat()),
                UserID = Context.UserID,
                CustomerID = Context.CustomerID,
            };

            var logId = DB.ExecuteScalar<long>("NL_AddLog",
                CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Log>("Tbl", nlLog));
        }

        public static string GetLogSection(LogType logType) {
            return logType.ToString();
        }

        public enum LogType {
            DataExsistense,
            Error,
            Info,
            Debug
        }
    }
}
