namespace UIAutomationTests.Tests.Shared {
    using System;
    using System.Linq;
    using System.Resources;
    using Ezbob.Database;
    using log4net;
    using UIAutomationTests.Core;

    class SharedDBClass : WebTestBase {
        public AConnection oDB { get; private set; }
        private static readonly ILog log = LogManager.GetLogger(typeof(SharedDBClass));

        public SharedDBClass(ResourceManager EnvironmentConfig) {
            this.EnvironmentConfig = EnvironmentConfig;
            try {
                oDB = new SqlConnection(null, this.EnvironmentConfig.GetString("DBConnectionString"));
                log.Debug("Connection to DB successfully opened." + Environment.NewLine);
            } catch (Exception ex) {
                log.Debug("Connection to DB failed." + Environment.NewLine);
            }
        }

        //TODO: check if logs while running SQL SPs needed.
/*
        public T ExecuteScalar<T>(string SP, CommandSpecies CS, string param = null) {

            T result;
            switch (param) {
                case null:
                    result = this.oDB.ExecuteScalar<T>(SP, CS);
                    break;
                default:
                    result = this.oDB.ExecuteScalar<T>(SP, CS, new QueryParameter(param));
                    break;
            }

            return result;
        }

        public void ExecuteNonQuery(string SP, CommandSpecies CS, QueryParameter[] param = null) {
            switch (param == null ? 0 : param.Count()) {
                case 0:
                    this.oDB.ExecuteNonQuery(SP, CS);
                    break;
                default:
                    this.oDB.ExecuteNonQuery(SP, CS, param);
                    break;
            }
        }

        public SafeReader GetFirst(string SP, CommandSpecies CS, QueryParameter[] param = null) {

            SafeReader result;
            switch (param == null ? 0 : param.Count()) {
                case 0:
                    result = this.oDB.GetFirst(SP, CS);
                    break;
                default:
                    result = this.oDB.GetFirst(SP, CS, param);
                    break;
            }

            return result;
        }*/
    }
}
