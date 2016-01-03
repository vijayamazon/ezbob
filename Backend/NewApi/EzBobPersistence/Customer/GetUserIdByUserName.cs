namespace EzBobPersistence.Customer.Commands {
    using System.Data;
    using System.Data.SqlClient;

    /// <summary>
    /// Gets user id by user name from Security_User table
    /// </summary>
    internal class GetUserIdByUserName {
        private static readonly string userNameParam = "@UserName";
        private static readonly string getUserIdByUserName = string.Format("SELECT UserId FROM Security_User WHERE UserName = {0} AND IsDeleted = 0", userNameParam);

        private string name;

        /// <summary>
        /// Sets the name of the user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public GetUserIdByUserName SetUserName(string userName) {
            this.name = userName;
            return this;
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        internal SqlCommand Get(SqlConnection connection) {
            var sqlCommand = new SqlCommand(getUserIdByUserName);
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.Connection = connection;

            sqlCommand.Parameters.Add(new SqlParameter(userNameParam, SqlDbType.NVarChar));
            sqlCommand.Parameters[userNameParam].Value = this.name;

            return sqlCommand;
        }
    }
}
