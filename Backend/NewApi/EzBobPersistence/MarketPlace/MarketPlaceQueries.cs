namespace EzBobPersistence.MarketPlace {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EzBobCommon;
    using EzBobModels.MarketPlace;

    //TODO: to do something with marketplace identification there are Guid and Integer and there is a need to transform Guid to integer
    public class MarketPlaceQueries : QueryBase, IMarketPlaceQueries {
        private static readonly string mpCustomermarketplace = "MP_CustomerMarketPlace";

        public MarketPlaceQueries(string connectionString)
            : base(connectionString) {}


        /// <summary>
        /// Validates the customer market place.
        /// </summary>
        /// <param name="marketPlaceTypeId">The market place type identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        public InfoAccumulator ValidateCustomerMarketPlace(Guid marketPlaceTypeId, string displayName) {
            InfoAccumulator info = new InfoAccumulator();
            if (!IsMarketPlaceInWhiteList(marketPlaceTypeId, displayName))
            {
                if (IsMarketPlaceExists(marketPlaceTypeId, displayName)) {
                    string msg = string.Format("the market place with name: '{0}' already exists", displayName);
                    info.AddError(msg);
                }
            }

            return info;
        }

        /// <summary>
        /// Determines whether the market place is in white list
        /// </summary>
        /// <param name="MarketPlaceTypeId">The market place type identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        public bool IsMarketPlaceInWhiteList(Guid MarketPlaceTypeId, string displayName) {
            using (var connection = GetOpenedSqlConnection2()) {
                Dictionary<string, object> whereValues = new Dictionary<string, object> {
                    {
                        "Name", displayName
                    }, {
                        "MarketPlaceTypeGuid", MarketPlaceTypeId
                    }
                };

                using (var whereCommand = GetSelectFirstByWhereCommand(connection.SqlConnection(), whereValues, "MP_WhiteList", "Id")) {
                    var result = ExecuteScalarAndLog<int>(whereCommand);
                    return result.HasValue;
                }
            }
        }

        /// <summary>
        /// Upserts the market place.
        /// </summary>
        /// <param name="marketPlace">The market place.</param>
        /// <param name="marketPlaceTypeId">The market place type identifier.</param>
        /// <returns>marketplace's id</returns>
        public Optional<int> UpsertMarketPlace(CustomerMarketPlace marketPlace, Guid marketPlaceTypeId) {

            var marketPlaceId = GetMarketPlaceIdFromTypeId(marketPlaceTypeId);
            if (!marketPlaceId.HasValue) {
                return -1;
            }

            DateTime utcNow = DateTime.UtcNow;

            marketPlace.MarketPlaceId = marketPlaceId.GetValue();
            marketPlace.Updated = utcNow;
            if (!marketPlace.Created.HasValue) {
                marketPlace.Created = utcNow;
            }

            var upsert = GetUpsertGenerator(marketPlace);
            upsert.WithTableName(mpCustomermarketplace)
                .WithMatchColumns(o => o.CustomerId, o => o.MarketPlaceId, o => o.DisplayName)
                .WithSkipColumns(o => o.Id)
                .WithUpdateColumnIfNull(o => o.Created)
                .WithOutputColumns(o => o.Id);

            using (var connection = GetOpenedSqlConnection2()) {
                using (var sqlCommand = upsert.Verify()
                    .GenerateCommand()) {
                    sqlCommand.Connection = connection.SqlConnection();
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Creates the new market place.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="securityData">The security data.</param>
        /// <param name="marketPlaceInternalId">The market place internal identifier.</param>
        /// <returns></returns>
        public Optional<int> CreateNewMarketPlace(int customerId, string displayName, byte[] securityData, Guid marketPlaceInternalId) {
            CustomerMarketPlace marketPlace = new CustomerMarketPlace
            {
                CustomerId = customerId,
                DisplayName = displayName,
                SecurityData = securityData
            };

            return UpsertMarketPlace(marketPlace, marketPlaceInternalId);
        }

        /// <summary>
        /// Determines whether the specified market place is exists
        /// </summary>
        /// <param name="marketPlaceTypeId">The market place type identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">empty display name</exception>
        public bool IsMarketPlaceExists(Guid marketPlaceTypeId, string displayName) {
            var  res = GetMarketPlaceId(marketPlaceTypeId, displayName);
            return res.HasValue;
        }

        /// <summary>
        /// Gets the market place identifier.
        /// </summary>
        /// <param name="marketPlaceTypeId">The market place type identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">empty display name</exception>
        public Optional<int> GetMarketPlaceId(Guid marketPlaceTypeId, string displayName) {
            if (string.IsNullOrEmpty(displayName)) {
                throw new ArgumentException("empty display name");
            }

            var id = GetMarketPlaceIdFromTypeId(marketPlaceTypeId);

            Dictionary<string, object> whereColumns = new Dictionary<string, object>();
            whereColumns.Add("MarketPlaceId", id.GetValue());
            whereColumns.Add("DisplayName", displayName);

            using (var connection = GetOpenedSqlConnection2()) {
                using (var sqlCommand = GetSelectFirstByWhereCommand(connection.SqlConnection(), whereColumns, "MP_CustomerMarketPlace", "Id")) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Gets the customer market places.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="marketplaceType">Type of the marketplace.</param>
        /// <returns></returns>
        public IList<CustomerMarketPlace> GetCustomerMarketPlaces(int customerId, Guid marketplaceType) {
            //TODO: probably we should not go every time to DB. May be we could create look-up table or configuration
            var marketplaceId = GetMarketPlaceIdFromTypeId(marketplaceType);
            if (!marketplaceId.HasValue) {
                return new List<CustomerMarketPlace>();
            }

            using (var sqlConnecton = GetOpenedSqlConnection()) {
                using (var sqlCommand = GetSelectAllByWhereCommand(sqlConnecton, GetColumnsValues(marketplaceId.GetValue(), customerId, null)
                    .ToList(), mpCustomermarketplace)) {
                    return CreateModels<CustomerMarketPlace>(sqlCommand.ExecuteReader())
                        .ToList();
                }
            }
        }

        /// <summary>
        /// Gets the customer market place.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="marketPlaceTypeId">The market place type identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        public CustomerMarketPlace GetCustomerMarketPlace(int customerId, Guid marketPlaceTypeId, string displayName) {
            Optional<int> marketPlaceId = GetMarketPlaceIdFromTypeId(marketPlaceTypeId);
            if (!marketPlaceId.HasValue) {
                return null;
            }

            using (var connection = GetOpenedSqlConnection2()) {
                Dictionary<string, object> whereColumnValues = new Dictionary<string, object>();
                whereColumnValues.Add("CustomerId", customerId);
                whereColumnValues.Add("DisplayName", displayName);
                whereColumnValues.Add("MarketPlaceId", marketPlaceId.GetValue());
                CustomerMarketPlace marketPlace;
                using (var slqCommand = GetSelectFirstByWhereCommand(connection.SqlConnection(), whereColumnValues, mpCustomermarketplace)) {
                    marketPlace = CreateModel<CustomerMarketPlace>(slqCommand.ExecuteReader());
                }
                return marketPlace;
            }
        }

        /// <summary>
        /// Gets the market place identifier from marketPlaceType.
        /// </summary>
        /// <param name="marketPlaceType">Type of the market place.</param>
        /// <returns></returns>
        public Optional<int> GetMarketPlaceIdFromTypeId(Guid marketPlaceType) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (var command = GetEmptyCommand(sqlConnection)) {
                    string query = "SELECT Id FROM MP_MarketplaceType WHERE InternalId = " + marketPlaceType;
                    command.CommandText = query;
                    return ExecuteScalarAndLog<int>(command);
                }
            }
        }

        /// <summary>
        /// Upserts the market place updating updateHistory.
        /// </summary>
        /// <param name="updateHistory">The updateHistory.</param>
        /// <returns></returns>
        public Optional<int> UpsertMarketPlaceUpdatingHistory(CustomerMarketPlaceUpdateHistory updateHistory) {
            using (var connection = GetOpenedSqlConnection2()) {

                var upsert = GetUpsertGenerator(updateHistory)
                    .WithTableName("MP_CustomerMarketPlaceUpdatingHistory")
                    .WithMatchColumns(o => o.UpdatingStart, o => o.CustomerMarketPlaceId)
                    .WithConnection(connection.SqlConnection())
                    .WithUpdateColumnIfNull(o => o.UpdatingStart)
                    .WithOutputColumns(o => o.Id);

                using (var sqlCommand = upsert.Verify()
                    .GenerateCommand()) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Gets the market place updateHistory by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public CustomerMarketPlaceUpdateHistory GetMarketPlaceUpdatingHistoryById(int id)
        {
            using (var connection = GetOpenedSqlConnection2()) {
                using (var sqlCommand = GetEmptyCommand(connection.SqlConnection())) {
                    string cmd = "SELECT * FROM MP_CustomerMarketPlaceUpdatingHistory WHERE Id = @Id";
                    sqlCommand.CommandText = cmd;
                    sqlCommand.Parameters.AddWithValue("@Id", id);

                    return CreateModel<CustomerMarketPlaceUpdateHistory>(sqlCommand.ExecuteReader());
                }
            }
        }

        /// <summary>
        /// Gets the columns values.
        /// </summary>
        /// <param name="marketPlaceId">The market place identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<string, object>> GetColumnsValues(int marketPlaceId, int customerId, string displayName) {
            yield return new KeyValuePair<string, object>("CustomerId", customerId);
            yield return new KeyValuePair<string, object>("MarketPlaceId", marketPlaceId);
            if (!string.IsNullOrEmpty(displayName)) {
                yield return new KeyValuePair<string, object>("DisplayName", displayName);
            }
        }
    }
}
