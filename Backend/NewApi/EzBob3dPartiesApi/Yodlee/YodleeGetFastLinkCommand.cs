using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.Yodlee
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Asks to retrieve fast link
    /// </summary>
    public class YodleeGetFastLinkCommand : CommandBase
    {
        /// <summary>
        /// Gets or sets the name of the cobrand user.
        /// </summary>
        /// <value>
        /// The name of the cobrand user.
        /// </value>
        public string CobrandUserName { get; set; }
        /// <summary>
        /// Gets or sets the cobrand password.
        /// </summary>
        /// <value>
        /// The cobrand password.
        /// </value>
        public string CobrandPassword { get; set; }
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the user password.
        /// </summary>
        /// <value>
        /// The user password.
        /// </value>
        public string UserPassword { get; set; }
        /// <summary>
        /// Gets or sets the content service identifier.
        /// </summary>
        /// <value>
        /// The content service identifier.
        /// </value>
        public int ContentServiceId { get; set; }
        /// <summary>
        /// Gets or sets the site identifier.
        /// </summary>
        /// <value>
        /// The site identifier.
        /// </value>
        public int SiteId { get; set; }
    }
}
