namespace EzBobAcceptanceTests.Customer.Responses {
    using EzBobCommon.Utils;

    /// <summary>
    /// represents customer sign-up response 
    /// </summary>
    public class CustomerSignupResponse {
        public string CustomerId { get; set; }
        public string[] Errors { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has errors.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has errors; otherwise, <c>false</c>.
        /// </value>
        public bool HasErrors
        {
            get { return CollectionUtils.IsNotEmpty(Errors); }
        }

        /// <summary>
        /// Validates itself.
        /// </summary>
        /// <returns></returns>
        public bool SelfValidate() {
            if (StringUtils.IsEmpty(CustomerId) && CollectionUtils.IsEmpty(Errors)) {
                return false;
            }

            return true;
        }
    }
}
