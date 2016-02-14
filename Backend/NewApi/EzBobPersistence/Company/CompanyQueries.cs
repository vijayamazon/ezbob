namespace EzBobPersistence.Company {
    using System;
    using System.Collections.Generic;
    using EzBobCommon;
    using EzBobModels.Company;
    using EzBobModels.Customer;
    using EzBobModels.Enums;
    using EzBobPersistence.QueryGenerators;

    /// <summary>
    /// Contains company related queries.
    /// </summary>
    public class CompanyQueries : QueryBase, ICompanyQueries {
        public CompanyQueries(string connectionString)
            : base(connectionString) {}

        /// <summary>
        /// Upserts the company.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <returns></returns>
        public Optional<int> UpsertCompany(Company company) {
            using (var sqlConnection = GetOpenedSqlConnection2()) {
                var cmd = GetUpsertGenerator(company)
                    .WithConnection(sqlConnection.SqlConnection())
                    .WithTableName(Tables.Company)
                    .WithMatchColumns(o => o.Id)
                    .WithSkipColumns(o => o.Id)
                    .WithOutputColumns(o => o.Id)
                    .Verify()
                    .GenerateCommand();

                using (var sqlCommand = cmd) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Gets the company by identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public Optional<Company> GetCompanyById(int companyId) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = new SelectWhereGenerator<Company>()
                    .WithOptionalConnection(connection.SqlConnection())
                    .WithTableName(Tables.Company)
                    .WithSelect() //select *
                    .WithWhere(o => o.Id, companyId)
                    .Verify()
                    .GenerateCommand();

                using (var sqlCommand = cmd) {
                    return CreateModel<Company>(sqlCommand.ExecuteReader());
                }
            }
        }

        /// <summary>
        /// Upserts the company employee count.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public Optional<int> UpsertCompanyEmployeeCount(CompanyEmployeeCount count) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = GetUpsertGenerator(count)
                    .WithConnection(connection.SqlConnection())
                    .WithTableName(Tables.CompanyEmployeeCount)
                    .WithMatchColumns(o => o.CompanyId, o => o.CustomerId)
                    .WithSkipColumns(o => o.Id)
                    .WithOutputColumns(o => o.Id)
                    .Verify()
                    .GenerateCommand();
                using (var sqlCommand = cmd) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Gets the company employee count.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public Optional<CompanyEmployeeCount> GetCompanyEmployeeCount(int customerId, int companyId) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = new SelectWhereGenerator<CompanyEmployeeCount>()
                    .WithOptionalConnection(connection.SqlConnection())
                    .WithTableName(Tables.CompanyEmployeeCount)
                    .WithWhere(o => o.CompanyId, companyId)
                    .WithWhere(o => o.CustomerId, customerId)
                    .Verify()
                    .GenerateCommand();
                using (var sqlCommand = cmd) {
                    return CreateModel<CompanyEmployeeCount>(sqlCommand.ExecuteReader());
                }
            }
        }

        /// <summary>
        /// Upserts the director.
        /// </summary>
        /// <param name="director">The director.</param>
        /// <returns></returns>
        public Optional<int> UpsertDirector(Director director) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = GetUpsertGenerator(director)
                    .WithConnection(connection.SqlConnection())
                    .WithMatchColumns(o => director.id)
                    .WithOutputColumns(o => director.id)
                    .WithSkipColumns(o => director.id)
                    .WithTableName(Tables.Director)
                    .Verify()
                    .GenerateCommand();

                using (var sqlCommand = cmd) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Gets the directors.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public Optional<IEnumerable<Director>> GetDirectors(int customerId, int companyId) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = new SelectWhereGenerator<Director>()
                    .WithOptionalConnection(connection.SqlConnection())
                    .WithTableName(Tables.Director)
                    .WithSelect() //select *
                    .WithWhere(o => o.CustomerId, customerId)
                    .WithWhere(o => o.CompanyId, companyId)
                    .Verify()
                    .GenerateCommand();

                using (var sqlCommand = cmd) {
                    return CreateModels<Director>(sqlCommand.ExecuteReader())
                        .AsOptional();
                }
            }
        }

        /// <summary>
        /// Upserts the director address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        public Optional<int> UpsertDirectorAddress(CustomerAddress address) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = GetUpsertGenerator(address)
                    .WithConnection(connection.SqlConnection())
                    .WithMatchColumns(o => o.CompanyId, o => o.CustomerId, o => o.DirectorId)
                    .WithSkipColumns(o => o.addressId)
                    .WithOutputColumns(o => o.addressId)
                    .WithTableName(Tables.CustomerAddress)
                    .Verify()
                    .GenerateCommand();

                using (var sqlCommand = cmd) {
                    return ExecuteScalarAndLog<int>(sqlCommand);
                }
            }
        }

        /// <summary>
        /// Gets the directors addresses.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public Optional<IEnumerable<CustomerAddress>> GetDirectorsAddresses(int customerId, int companyId) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = new SelectWhereGenerator<CustomerAddress>()
                    .WithOptionalConnection(connection.SqlConnection())
                    .WithTableName(Tables.CustomerAddress)
                    .WithSelect()
                    .WithWhere(o => o.CustomerId, customerId)
                    .WithWhere(o => o.CompanyId, companyId)
                    .Verify()
                    .GenerateCommand();
                using (var sqlCommand = cmd) {
                    return CreateModels<CustomerAddress>(sqlCommand.ExecuteReader())
                        .AsOptional();
                }
            }
        }

        /// <summary>
        /// Gets the type of the company business.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public Optional<TypeOfBusiness> GetCompanyBusinessType(int companyId) {
            using (var connection = GetOpenedSqlConnection2()) {
                var cmd = new SelectWhereGenerator<Company>()
                    .WithOptionalConnection(connection.SqlConnection())
                    .WithTableName(Tables.Company)
                    .WithSelect(o => o.TypeOfBusiness)
                    .WithWhere(o => o.Id, companyId)
                    .Verify()
                    .GenerateCommand();

                using (var sqlCommand = cmd) {
                    var res = ExecuteScalarAndLog<string>(sqlCommand);
                    if (res.HasValue) {
                        return (TypeOfBusiness)Enum.Parse(typeof(TypeOfBusiness), res.Value);
                    }

                    return Optional<TypeOfBusiness>.Empty();
                }
            }
        }
    }
}
