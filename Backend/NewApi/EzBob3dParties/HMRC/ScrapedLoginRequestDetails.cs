using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.HMRC {
    /// <summary>
    /// Stores login request details (method, URL, form field names).
    /// </summary>
    public class ScrapedLoginRequestDetails {
        /// <summary>
        /// Gets or sets the HTTP method. (GET, POST, etc.)
        /// </summary>
        /// <value>
        /// The HTTP method.
        /// </value>
        public string HttpMethod { get; set; }

        /// <summary>
        /// Gets or sets the login page URL.
        /// </summary>
        /// <value>
        /// The login page URL.
        /// </value>
        public string LoginPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString() {
            return string.Format(
                "Login method: {0} {1}\nUser name field: {2}\nPassword field: {3}",
                this.HttpMethod,
                this.LoginPageUrl,
                this.UserName,
                this.Password
                );

        }
    }
}
