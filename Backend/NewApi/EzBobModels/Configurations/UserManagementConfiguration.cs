using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Configurations
{
    /// <summary>
    /// User management configuration (currently is taken from DB)
    /// </summary>
    public class UserManagementConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagementConfiguration"/> class.
        /// </summary>
        public UserManagementConfiguration() {
            this.Underwriters = new HashSet<string>();
        }
        /// <summary>
        /// Gets or sets the login validation string for web.
        /// </summary>
        /// <value>
        /// The login validation string for web.
        /// </value>
        public string LoginValidationStringForWeb { get; set; }

        /// <summary>
        /// Gets or sets the number of invalid password attempts.
        /// </summary>
        /// <value>
        /// The number of invalid password attempts.
        /// </value>
        public int NumOfInvalidPasswordAttempts { get; set; }

        /// <summary>
        /// Gets or sets the invalid password attempts period seconds.
        /// </summary>
        /// <value>
        /// The invalid password attempts period seconds.
        /// </value>
        public int InvalidPasswordAttemptsPeriodSeconds { get; set; }

        /// <summary>
        /// Gets or sets the invalid password block seconds.
        /// </summary>
        /// <value>
        /// The invalid password block seconds.
        /// </value>
        public int InvalidPasswordBlockSeconds { get; set; }

        /// <summary>
        /// Gets or sets the password validity.
        /// </summary>
        /// <value>
        /// The password validity.
        /// </value>
        public string PasswordValidity { get; set; }

        /// <summary>
        /// Gets or sets the login validity.
        /// </summary>
        /// <value>
        /// The login validity.
        /// </value>
        public string LoginValidity { get; set; }

        /// <summary>
        /// Gets the underwriters.
        /// </summary>
        /// <value>
        /// The underwriters.
        /// </value>
        public ISet<string> Underwriters { get; private set; }
    }
}
