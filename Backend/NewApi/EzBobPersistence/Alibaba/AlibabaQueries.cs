namespace EzBobPersistence.Alibaba
{
    using EzBobModels;
    using EzBobPersistence;

    public class AlibabaQueries : QueryBase, IAlibabaQueries {
        public AlibabaQueries(string connectionString)
            : base(connectionString) {}

        public bool? CreateAlibabaBuyer(AlibabaBuyer alibabaBuyer) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                var cmd = GetInsertCommand(alibabaBuyer, sqlConnection, Tables.AlibabaBuyer);
                if (!cmd.HasValue) {
                    return null;
                }

                using (var sqlCommand = cmd.GetValue()) {
                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }
    }
}
