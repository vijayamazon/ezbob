namespace EzBobCommon.NSB {
    using EzBobCommon.Utils;

    /// <summary>
    /// Base class for command responses
    /// </summary>
    public abstract class CommandResponseBase : CommandBase {
        public string[] Errors { get; set; }
        public string[] Warnings { get; set; }
        public string[] Infos { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has errors.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has errors; otherwise, <c>false</c>.
        /// </value>
        public bool HasErrors
        {
            get { return this.IsFailed || CollectionUtils.IsNotEmpty(Errors); }
        }
    }
}
