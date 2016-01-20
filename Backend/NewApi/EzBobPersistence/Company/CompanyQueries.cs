namespace EzBobPersistence.Company {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EzBobCommon;
    using EzBobModels;

    /// <summary>
    /// Contains company related queries
    /// </summary>
    public class CompanyQueries : QueryBase, ICompanyQueries {
        public CompanyQueries(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Saves the company.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <returns>company id , or null is there is some problem</returns>
        public Optional<int> SaveCompany(Company company) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {
                IEnumerable<KeyValuePair<string, object>> ids = null;
                if (company.Id.HasValue) {
                    ids = Enumerable.Repeat(new KeyValuePair<string, object>("Id", company.Id.Value), 1);
                    company.Id = null; //we do not want to update Id column
                }

                var cmd = GetUpsertCommand(company, sqlConnection.SqlConnection(), Tables.Company, ids, "Id", o => "Id".Equals(o, StringComparison.InvariantCultureIgnoreCase));
                if (!cmd.HasValue) {
                    return Optional<int>.Empty();
                }

                using (var sqlCommand = cmd.GetValue()) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Saves the directors.
        /// </summary>
        /// <param name="directors">The directors.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns>
        /// list of ids, or nothing in case of failure
        /// </returns>
        public IEnumerable<int> SaveDirectors(IEnumerable<Director> directors, int companyId) {
            List<int> directorsIds = new List<int>();
            using (var sqlConnection = GetOpenedSqlConnection()) {
                foreach (var director in directors) {
                    var cmd = GetInsertCommand(director, sqlConnection, Tables.Director, "Id", c => "Id".Equals(c, StringComparison.InvariantCultureIgnoreCase));
                    if (!cmd.HasValue) {
                        return Enumerable.Empty<int>();
                    }

                    using (var sqlCommand = cmd.GetValue()) {
                        var res = ExecuteScalarAndLog<int>(sqlCommand);
                        if (res.HasValue) {
                            directorsIds.Add(res.GetValue());
                        } else {
                            string msg = "could not save director";
                            Log.Error(msg);
                            throw new InvalidOperationException(msg);
                        }
                    }
                }
            }

            return directorsIds;
        }

        /// <summary>
        /// Saves the directors addresses.
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns></returns>
        public bool SaveDirectorAddresses(IEnumerable<CustomerAddress> addresses) {
            using (var sqlConnection = GetOpenedSqlConnection()) {
                foreach (var address in addresses) {
                    if (address != null) {
                        var cmd = GetInsertCommand(address, sqlConnection, Tables.CustomerAddress);
                        if (!cmd.HasValue) {
                            return false;
                        }

                        using (var sqlCommand = cmd.GetValue()) {
                            var res = ExecuteScalarAndLog<int>(sqlCommand);
                            return res.HasValue;
                        }
                    }
                }
            }

            return true;
        }
    }
}
