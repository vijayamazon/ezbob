namespace EzBobPersistence.Currency {
    using System;
    using EzBobCommon;
    using EzBobCommon.Currencies;
    using EzBobModels.Currency;

    /// <summary>
    /// Contains currency related queries
    /// </summary>
    public class CurrencyQueries : QueryBase, ICurrencyQueries {
        public CurrencyQueries(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Gets the currency historical rate.
        /// </summary>
        /// <param name="currencyISOName">Name of the currency iso.</param>
        /// <param name="purchaseDataUtc">The purchase data UTC.</param>
        /// <returns></returns>
        public Optional<decimal> GetCurrencyHistoricalRate(string currencyISOName, DateTime purchaseDataUtc = default(DateTime)) {

            if (string.IsNullOrEmpty(currencyISOName)) {
                return Optional<decimal>.Empty();
            }

            int currencyId = GetCurrencyId(currencyISOName);
            if (currencyId < 0) {
                return Optional<decimal>.Empty();
            }

            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (var sqlCommand = GetEmptyCommand(sqlConnection)) {
                    sqlCommand.CommandText = "SELECT TOP 1 Price FROM MP_CurrencyRateHistory WHERE CurrencyId = @CurrencyId AND Updated <= @UpdatedValue ORDER BY Updated DESC";
                    sqlCommand.Parameters.AddWithValue("@CurrencyId", currencyId);

                    if (purchaseDataUtc != default(DateTime)) {
                        sqlCommand.Parameters.AddWithValue("@UpdatedValue", purchaseDataUtc);
                    } else {
                        sqlCommand.Parameters.AddWithValue("@UpdatedValue", DateTime.UtcNow);
                    }

                    return Optional<decimal>.Of(ExecuteScalarAndLog<decimal>(sqlCommand));
                }
            }
        }


        /// <summary>
        /// Gets the currency identifier.
        /// </summary>
        /// <param name="currencyISOName">ISO Name of the currency</param>
        /// <returns></returns>
        public int GetCurrencyId(string currencyISOName) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                using (var sqlCommand = GetSelectFirstByWhereCommand(sqlConnection, "Name", currencyISOName, "MP_Currency")) {
                    var currencyData = CreateModel<CurrencyData>(sqlCommand.ExecuteReader());
                    return currencyData.Id;
                }
            }
        }

        /// <summary>
        /// Saves the currency rate.
        /// </summary>
        /// <param name="money">The money.</param>
        /// <param name="lastUpdatedUtc">The last updated UTC.</param>
        /// <returns></returns>
        public bool SaveCurrencyRate(Money money, DateTime lastUpdatedUtc) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                CurrencyData data = new CurrencyData {
                    LastUpdated = lastUpdatedUtc,
                    Name = money.ISOCurrencySymbol,
                    Price = money.Amount
                };

                var insertCommand = GetInsertCommand(data, sqlConnection, "MP_Currency", null, isSkipColumn: col => "Id".Equals(col));
                if (!insertCommand.HasValue) {
                    return false;
                }

                using (var sqlCommand = insertCommand.GetValue()) {
                    return ExecuteNonQueryAndLog(sqlCommand);
                }
            }
        }
    }
}
