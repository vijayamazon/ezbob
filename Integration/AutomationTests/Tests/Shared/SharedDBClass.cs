﻿namespace UIAutomationTests.Tests.Shared {
    using System.Linq;
    using System.Resources;
    using Ezbob.Database;

    class SharedDBClass {
        private readonly AConnection oDB;
        private readonly ResourceManager _EnvironmentConfig;

        public SharedDBClass(ResourceManager EnvironmentConfig) {
            this._EnvironmentConfig = EnvironmentConfig;
            this.oDB = new SqlConnection(null,this._EnvironmentConfig.GetString("QA2DBConnectionString"));
        }

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
        }
    }
}
