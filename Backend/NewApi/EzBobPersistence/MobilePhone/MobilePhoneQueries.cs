namespace EzBobPersistence.MobilePhone {
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using EzBobModels.MobilePhone;
    using EzBobModels.ThirdParties.Twilio;
    using EzBobPersistence;

    /// <summary>
    /// Encapsulates mobile phone related queries
    /// </summary>
    public class MobilePhoneQueries : QueryBase, IMobilePhoneQueries {
        private static readonly string ValidateMobileCode = "ValidateMobileCode";
        private static readonly string Phone = "@Phone";
        private static readonly string Code = "@Code";

        private static readonly string StoreMobileCode = "StoreMobileCode";
        private static readonly string SaveSmsMessage = "SaveSmsMessage";

        /// <summary>
        /// Initializes a new instance of the <see cref="MobilePhoneQueries"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public MobilePhoneQueries(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Validates the mobile phone.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public bool ValidateMobilePhoneNumber(string phoneNumber, string code) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (SqlCommand sqlCommand = new SqlCommand(ValidateMobileCode, sqlConnection)) {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(Phone, SqlDbType.VarChar));
                    sqlCommand.Parameters[Phone].Value = phoneNumber;

                    sqlCommand.Parameters.Add(new SqlParameter(Code, SqlDbType.Char));
                    sqlCommand.Parameters[Code].Value = code;

                    object score = sqlCommand.ExecuteScalar();
                    if (score == null || score is DBNull) {
                        Log.Error("Got null when validated phone number");
                        return false;
                    }

                    return Convert.ToBoolean(score);
                }
            }
        }

        /// <summary>
        /// Gets the current mobile code count information.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        public CountInfo GetCurrentMobileCodeCountInfo(string phoneNumber) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (SqlCommand sqlCommand = new SqlCommand("GetCurrentMobileCodeCount", sqlConnection)) {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(Phone, SqlDbType.Char));
                    sqlCommand.Parameters[Phone].Value = phoneNumber;

                    var sqlDataReader = sqlCommand.ExecuteReader();

                    return CreateModel<CountInfo>(sqlDataReader);
                }
            }
        }

        /// <summary>
        /// Stores the mobile phone and code.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public bool StoreMobilePhoneAndCode(string phoneNumber, string code) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (SqlCommand sqlCommand = new SqlCommand(StoreMobileCode, sqlConnection)) {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(Phone, SqlDbType.VarChar));
                    sqlCommand.Parameters[Phone].Value = phoneNumber;

                    sqlCommand.Parameters.Add(new SqlParameter(Code, SqlDbType.Char));
                    sqlCommand.Parameters[Code].Value = code;

                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Saves the Twilio SMS.
        /// </summary>
        /// <param name="sms">The SMS.</param>
        /// <returns></returns>
        public bool SaveTwilioSms(TwilioSms sms) {
            DataTable dataTable = ConvertToDataTable(sms);

            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (SqlCommand sqlCommand = new SqlCommand(SaveSmsMessage, sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    SqlParameter tableValueParameter = sqlCommand.Parameters.AddWithValue("@Tbl", dataTable);
                    tableValueParameter.SqlDbType = SqlDbType.Structured;

                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }
    }
}
