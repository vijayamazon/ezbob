using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobDAL {
    /// <summary>
    /// Container of loan related queries
    /// </summary>
    public class LoanQueries : QueryBase {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoanQueries"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public LoanQueries(string connectionString)
            : base(connectionString) {}

        public void GetLoanTypeAndDefault(int loanTypeId) {
            
        }
    }
}
