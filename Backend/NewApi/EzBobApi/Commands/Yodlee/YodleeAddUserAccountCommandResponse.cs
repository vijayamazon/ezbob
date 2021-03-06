﻿namespace EzBobApi.Commands.Yodlee
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Response to add user account command
    /// </summary>
    public class YodleeAddUserAccountCommandResponse : CommandResponseBase
    {
        /// <summary>
        /// Gets or sets the fast link URL.
        /// </summary>
        /// <value>
        /// The fast link URL.
        /// </value>
        public string FastlinkUrl { get; set; }
    }
}
