using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobPersistence.Company
{
    using EzBobCommon;
    using EzBobModels;

    public interface ICompanyQueries {
        /// <summary>
        /// Saves the company.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <returns>empty optional if there some problem</returns>
        Optional<int> SaveCompany(Company company);

        /// <summary>
        /// Saves the directors.
        /// </summary>
        /// <param name="directors">The directors.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns>list of ids, or nothing in case of failure</returns>
        IEnumerable<int> SaveDirectors(IEnumerable<Director> directors, int companyId);

        /// <summary>
        /// Saves the director addresses.
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns></returns>
        bool SaveDirectorAddresses(IEnumerable<CustomerAddress> addresses);
    }
}
