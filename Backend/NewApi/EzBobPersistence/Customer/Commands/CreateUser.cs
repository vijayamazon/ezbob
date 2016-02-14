namespace EzBobPersistence.Customer.Commands {
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using EzBobCommon.Utils;

    /// <summary>
    /// Builds sql command to call 'CreateWebUser' stored procedure
    /// This class does not validate parameters. It assumes that parameters were validated by caller
    /// </summary>
    internal class CreateUser {
        private static readonly string CreateWebUserProcedure = "CreateWebUser";

        #region Stored procedure parameters

        private static readonly string emailParam = "@Email";
        private static readonly string passwordParam = "@EzPassword";
        private static readonly string securityQuestionIdParam = "@SecurityQuestionID";
        private static readonly string securityAnswerParam = "@SecurityAnswer";
        private static readonly string rolenameParam = "@RoleName";
        private static readonly string branchIdParam = "@BranchID";
        private static readonly string ipParam = "@Ip";
        private static readonly string nowParam = "@Now";

        #endregion

        private string email;
        private string hashedUserNameAndPassword;
        private int securityQuestionId;
        private string securityAnswer;
        private string roleName;
        private int branchId = 0; //TODO: clarify why we need this
        private string remoteIp;

        /// <summary>
        /// Sets the email and password.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        internal CreateUser SetEmailAndPassword(String emailAddress, String password) {
            this.email = emailAddress;
            this.hashedUserNameAndPassword = HashingUtils.HashUserNameAndPassword(emailAddress, password);
            return this;
        }

        /// <summary>
        /// Sets the security question identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        internal CreateUser SetSecurityQuestionId(int id) {
            this.securityQuestionId = id;
            return this;
        }

        /// <summary>
        /// Sets the security question answer.
        /// </summary>
        /// <param name="answer">The answer.</param>
        /// <returns></returns>
        internal CreateUser SetSecurityQuestionAnswer(String answer) {
            this.securityAnswer = answer;
            return this;
        }

        /// <summary>
        /// Sets the name of the role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns></returns>
        internal CreateUser SetRoleName(String role) {
            this.roleName = role;
            return this;
        }

        /// <summary>
        /// Sets the remote ip.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns></returns>
        internal CreateUser SetRemoteIp(String ip) {
            this.remoteIp = ip;
            return this;
        }

        /// <summary>
        /// Gets the command using specified connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        internal SqlCommand Get(SqlConnection connection) {
            var sqlCommand = new SqlCommand(CreateWebUserProcedure, connection);
            sqlCommand.CommandType = CommandType.StoredProcedure;

            sqlCommand.Parameters.Add(new SqlParameter(emailParam, SqlDbType.NVarChar));
            sqlCommand.Parameters[emailParam].Value = this.email;

            sqlCommand.Parameters.Add(new SqlParameter(passwordParam, SqlDbType.VarChar));
            sqlCommand.Parameters[passwordParam].Value = this.hashedUserNameAndPassword;

            sqlCommand.Parameters.Add(new SqlParameter(securityQuestionIdParam, SqlDbType.Int));
            sqlCommand.Parameters[securityQuestionIdParam].Value = this.securityQuestionId;

            sqlCommand.Parameters.Add(new SqlParameter(securityAnswerParam, SqlDbType.VarChar));
            sqlCommand.Parameters[securityAnswerParam].Value = this.securityAnswer ?? "default";

            if (StringUtils.IsNotEmpty(this.roleName)) {
                sqlCommand.Parameters.Add(new SqlParameter(rolenameParam, SqlDbType.NVarChar));
                sqlCommand.Parameters[rolenameParam].Value = this.roleName;
            }

            sqlCommand.Parameters.Add(new SqlParameter(branchIdParam, SqlDbType.Int));
            sqlCommand.Parameters[branchIdParam].Value = this.branchId;

            sqlCommand.Parameters.Add(new SqlParameter(ipParam, SqlDbType.NVarChar));
            sqlCommand.Parameters[ipParam].Value = this.remoteIp ?? "unknown";

            sqlCommand.Parameters.Add(new SqlParameter(nowParam, SqlDbType.DateTime));
            sqlCommand.Parameters[nowParam].Value = DateTime.UtcNow;

            return sqlCommand;
        }
    }
}
