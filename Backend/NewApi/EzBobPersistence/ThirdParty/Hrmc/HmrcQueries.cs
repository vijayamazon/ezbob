namespace EzBobPersistence.ThirdParty.Hrmc {
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using EzBobModels.Hmrc;

    /// <summary>
    /// Hmrc related queries
    /// </summary>
    public class HmrcQueries : QueryBase, IHmrcQueries {
        public HmrcQueries(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Saves the vat returns.
        /// </summary>
        /// <param name="vatReturnsPerBusinesses">The vat returns per businesses.</param>
        /// <param name="rtiMonthEntries">The rti month entries.</param>
        /// <param name="marketPlaceId">The market place identifier.</param>
        /// <param name="marketPlaceHistoryId">The market place history identifier.</param>
        /// <returns></returns>
        public bool SaveVatReturns(IEnumerable<VatReturnsPerBusiness> vatReturnsPerBusinesses, IEnumerable<RtiTaxMonthEntry> rtiMonthEntries, int marketPlaceId, int marketPlaceHistoryId) {
            using (var connection = GetOpenedSqlConnection2()) {
                foreach (var vatReturnPerBusiness in vatReturnsPerBusinesses) {
                    vatReturnPerBusiness.VatReturnRecord.CustomerMarketPlaceId = marketPlaceId;
                    vatReturnPerBusiness.VatReturnRecord.CustomerMarketPlaceUpdatingHistoryRecordId = marketPlaceHistoryId;
                    bool res = SaveVatReturnsPerBusiness(vatReturnPerBusiness, connection.SqlConnection());
                    if (!res) {
                        return false;
                    }

                    if (CollectionUtils.IsNotEmpty(rtiMonthEntries)) {
                        res = SaveRtiTaxMonthsEntries(rtiMonthEntries, connection.SqlConnection(), marketPlaceId, marketPlaceHistoryId);
                        if (!res) {
                            return false;
                        }
                    } else {
                        Log.Warn("no month entries");
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Saves the rti tax months entries.
        /// </summary>
        /// <param name="rtiMonthEntries">The rti month entries.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="marketPlaceId">The market place identifier.</param>
        /// <param name="marketPlaceHistoryId">The market place history identifier.</param>
        /// <returns></returns>
        private bool SaveRtiTaxMonthsEntries(IEnumerable<RtiTaxMonthEntry> rtiMonthEntries, SqlConnection connection, int marketPlaceId, int marketPlaceHistoryId) {
            RtiTaxMonthRecord record = new RtiTaxMonthRecord {
                Created = DateTime.UtcNow,
                CustomerMarketPlaceId = marketPlaceId,
                CustomerMarketPlaceUpdatingHistoryRecordId = marketPlaceHistoryId,
                //SourceID = //TODO: linked
            };

            int recordId = SaveRtiTaxMonthRecord(record, connection);
            if (recordId < 1) {
                return false;
            }

            foreach (var batch in rtiMonthEntries.Batch(800)) {
                bool res = SaveRtiMonthEntries(batch, connection);
                if (!res) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Saves the rti month entries.
        /// </summary>
        /// <param name="entries">The entries.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        private bool SaveRtiMonthEntries(IEnumerable<RtiTaxMonthEntry> entries, SqlConnection connection) {
            var cmd = GetMultiInsertCommand(entries, connection, "MP_RtiTaxMonthEntries", SkipColumns("Id"));
            if (!cmd.HasValue) {
                return false;
            }

            using (var sqlCommand = (SqlCommand)cmd) {
                return ExecuteNonQueryAndLog(sqlCommand);
            }
        }

        /// <summary>
        /// Saves the rti tax month record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        private int SaveRtiTaxMonthRecord(RtiTaxMonthRecord record, SqlConnection connection) {
            var cmd = GetInsertCommand(record, connection, "MP_RtiTaxMonthRecords", "Id", SkipColumns("Id"));
            return ExecuteInsertCommand(cmd);
        }

        /// <summary>
        /// Saves the vat returns per business.
        /// </summary>
        /// <param name="vatPerBusiness">The vat per business.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        private bool SaveVatReturnsPerBusiness(VatReturnsPerBusiness vatPerBusiness, SqlConnection connection) {
            //Business
            int businessId = SaveBusiness(vatPerBusiness.Business, connection);
            if (businessId > 0) {
                vatPerBusiness.VatReturnRecord.BusinessId = businessId;
            }

            //VatReturnRecord
            bool res = true;
            int vatRecordId = SaveVatReturnRecord(vatPerBusiness.VatReturnRecord, connection);

            //VatReturnEntries
            foreach (var batch in vatPerBusiness.Entries
                .ForEach(entry => entry.RecordId = vatRecordId)
                .Batch(800)) {
                res &= SaveVatReturnEntries(batch, connection);
                if (!res) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Saves the vat return entries.
        /// </summary>
        /// <param name="vatReturnEntries">The vat return entries.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        private bool SaveVatReturnEntries(IEnumerable<VatReturnEntry> vatReturnEntries, SqlConnection connection) {
            var cmd = GetMultiInsertCommand(vatReturnEntries, connection, "MP_VatReturnEntries", SkipColumns("Id"));
            if (!cmd.HasValue) {
                return false;
            }

            using (var sqlCommand = (SqlCommand)cmd) {
                return ExecuteNonQueryAndLog(sqlCommand);
            }
        }

        /// <summary>
        /// Saves the vat return record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        private int SaveVatReturnRecord(VatReturnRecord record, SqlConnection connection) {
            var cmd = GetInsertCommand(record, connection, "MP_VatReturnRecords", "Id", SkipColumns("Id"));
            return ExecuteInsertCommand(cmd);
        }


        /// <summary>
        /// Saves the business.
        /// </summary>
        /// <param name="business">The business.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        private int SaveBusiness(HmrcBusiness business, SqlConnection connection) {
            var cmd = GetInsertCommand(business, connection, "Business", "Id", SkipColumns("Id"));
            return ExecuteInsertCommand(cmd);
        }

        /// <summary>
        /// Executes the insert command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private int ExecuteInsertCommand(Optional<SqlCommand> command) {
            if (!command.HasValue) {
                return -1;
            }

            using (var sqlCommand = (SqlCommand)command) {
                return (int)ExecuteScalarAndLog<int>(sqlCommand);
            }
        }
    }
}
