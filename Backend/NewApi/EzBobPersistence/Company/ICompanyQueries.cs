namespace EzBobPersistence.Company
{
    using System.Collections.Generic;
    using EzBobCommon;
    using EzBobModels.Company;
    using EzBobModels.Customer;
    using EzBobModels.Enums;

    public interface ICompanyQueries {
        /// <summary>
        /// Upserts the company.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <returns>empty optional if there some problem</returns>
        Optional<int> UpsertCompany(Company company);

        /// <summary>
        /// Upserts the director.
        /// </summary>
        /// <param name="director">The director.</param>
        /// <returns></returns>
        Optional<int> UpsertDirector(Director director);

        /// <summary>
        /// Upserts the director address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        Optional<int> UpsertDirectorAddress(CustomerAddress address);

        /// <summary>
        /// Gets the type of the company business.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        Optional<TypeOfBusiness> GetCompanyBusinessType(int companyId);

        /// <summary>
        /// Gets the company by identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        Optional<Company> GetCompanyById(int companyId);

        /// <summary>
        /// Gets the directors.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        Optional<IEnumerable<Director>> GetDirectors(int customerId, int companyId);

        /// <summary>
        /// Gets the directors addresses.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        Optional<IEnumerable<CustomerAddress>> GetDirectorsAddresses(int customerId, int companyId);

        /// <summary>
        /// Upserts the company employee count.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        Optional<int> UpsertCompanyEmployeeCount(CompanyEmployeeCount count);

        /// <summary>
        /// Gets the company employee count.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        Optional<CompanyEmployeeCount> GetCompanyEmployeeCount(int customerId, int companyId);
    }
}
