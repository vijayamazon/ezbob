namespace EzBobPersistence.Broker
{
    using EzBobModels.Broker;
    using EzBobPersistence.QueryGenerators;

    public class BrokerQueries : QueryBase, IBrokerQueries
    {

        public BrokerQueries(string connectionString)
            : base(connectionString) {}


        /// <summary>
        /// Determines whether there is a broker with specified emailAddress
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns></returns>
        public bool IsExistsBroker(string emailAddress)
        {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = new SelectWhereGenerator<Broker>()
                    .WithOptionalConnection(connection.SqlConnection())
                    .WithTableName(Tables.Broker)
                    .WithSelect(o => o.BrokerID)
                    .WithWhere(o => o.ContactEmail, emailAddress)
                    .Verify()
                    .GenerateCommand();

                using (var sqlCommand = cmd) {
                    int id = (int)ExecuteScalarAndLog<int>(cmd);
                    return id > 0;
                }
            }
        }
    }
}
