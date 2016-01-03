using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.EBay
{
    public class EBayConfig
    {
        /// <summary>
        /// Gets or sets the Ru name.
        /// </summary>
        /// <value>
        /// The RuName.
        /// </value>
        public string RuName { get; set; }
        /// <summary>
        /// Gets or sets the API server URL.
        /// </summary>
        /// <value>
        /// The API server URL.
        /// </value>
        public string ApiServerUrl { get; set; }
        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        public string Application { get; set; }
        /// <summary>
        /// Gets or sets the certificate.
        /// </summary>
        /// <value>
        /// The certificate.
        /// </value>
        public string Certificate { get; set; }
        /// <summary>
        /// Gets or sets the developer.
        /// </summary>
        /// <value>
        /// The developer.
        /// </value>
        public string Developer { get; set; }
        /// <summary>
        /// Gets or sets the sign in URL.<br/>
        /// (For example: https://signin.sandbox.ebay.com/ws/eBayISAPI.dll?SignIn&amp;ru=)
        /// </summary>
        /// <value>
        /// The sign-in URL.
        /// </value>
        public string SignInUrl { get; set; }

        /// <summary>
        /// Gets or sets the size of the get orders page.
        /// </summary>
        /// <value>
        /// The size of the get orders page.
        /// </value>
        public int GetOrdersCallPageSize { get; set; }

        /// <summary>
        /// Gets or sets the get orders delay milli sec.
        /// </summary>
        /// <value>
        /// The get orders delay milli sec.
        /// </value>
        public int GetOrdersCallRetryDelayMilliSec { get; set; }

        /// <summary>
        /// Gets or sets the get orders maximum retries.
        /// </summary>
        /// <value>
        /// The get orders maximum retries.
        /// </value>
        public int GetOrdersCallMaximumRetries { get; set; }

        /// <summary>
        /// Gets or sets the maximum days interval for orders call.
        /// </summary>
        /// <value>
        /// The maximum days interval for orders call.
        /// </value>
        public int MaxDaysIntervalForGetOrdersCall { get; set; }
    }
}
