namespace EzBobPersistence {
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using EzBobModels.Configurations;

    //TODO: should be part of configuration manager
    public class ConfigurationQueries : QueryBase {
        /// <summary>
        /// The name of stored procedure
        /// </summary>
        private const string GetUserManagementConfig = "GetUserManagementConfiguration";

        private const string LoginvalidationstringForWeb = "LOGINVALIDATIONSTRINGFORWEB";
        private const string NumOfInvalidPasswordAttempts = "NUMOFINVALIDPASSWORDATTEMPTS";
        private const string InvalidPasswordAttemptsPeriodSeconds = "INVALIDPASSWORDATTEMPTSPERIODSECONDS";
        private const string InvalidPasswordBlockSeconds = "INVALIDPASSWORDBLOCKSECONDS";
        private const string PasswordValidity = "PASSWORDVALIDITY";
        private const string LoginValidity = "LOGINVALIDITY";
        private const string UnderwriterLogin = "__UNDERWRITERLOGIN__";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationQueries"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public ConfigurationQueries(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Gets the user management configuration.
        /// </summary>
        /// <returns></returns>
        public UserManagementConfiguration GetUserManagementConfiguration() {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (SqlCommand sqlCommand = new SqlCommand(GetUserManagementConfig, sqlConnection)) {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    UserManagementConfiguration configuration = new UserManagementConfiguration();

                    var sqlDataReader = sqlCommand.ExecuteReader();
                    while (sqlDataReader.Read()) {
                        try {
                            string key = sqlDataReader.GetString(0);
                            object value = sqlDataReader.GetValue(1);
                            if (key != null && !(value is DBNull)) {
                                AssignUserManagementConfigProperties(configuration, key, value);
                            }
                        } catch (InvalidCastException ex) {
                            //TODO: log error
                        }
                    }

                    return configuration;
                }
            }
        }

        /// <summary>
        /// Assigns the user management configuration properties.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        private void AssignUserManagementConfigProperties(UserManagementConfiguration configuration, String propertyName, Object propertyValue) {
            switch (propertyName.ToUpperInvariant()) {
            case LoginvalidationstringForWeb:
                configuration.LoginValidationStringForWeb = (String)propertyValue;
                break;
            case NumOfInvalidPasswordAttempts:
                configuration.NumOfInvalidPasswordAttempts = Convert.ToInt32(propertyValue);
                break;
            case InvalidPasswordAttemptsPeriodSeconds:
                configuration.InvalidPasswordAttemptsPeriodSeconds = Convert.ToInt32(propertyValue);
                break;
            case InvalidPasswordBlockSeconds:
                configuration.InvalidPasswordBlockSeconds = Convert.ToInt32(propertyValue);
                break;
            case PasswordValidity:
                configuration.PasswordValidity = (string)propertyValue;
                break;
            case LoginValidity:
                configuration.LoginValidity = (string)propertyValue;
                break;
            case UnderwriterLogin:
                configuration.Underwriters.Add((string)propertyValue);
                break;
            default:
                //TODO: log unexpected property
                break;
            }
        }
    }
}
