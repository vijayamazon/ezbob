using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.EBay
{
    using EzBobCommon.NSB;
    using EzBobModels.EBay;

    /// <summary>
    /// Requests user's ebay data
    /// </summary>
    public class EbayGetUserData3dPartyCommandResponse : CommandResponseBase
    {
        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        /// <remarks>
        /// payload of command should be return with response
        /// </remarks>
        public IDictionary<string, object> Payload { get; set; }
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        public string Token { get; set; }
        /// <summary>
        /// Gets or sets the ebay user data.
        /// </summary>
        /// <value>
        /// The ebay user data.
        /// </value>
        public EbayUserData EbayUserData { get; set; }
        /// <summary>
        /// Gets or sets the ebay user registration address data.
        /// </summary>
        /// <value>
        /// The ebay user address data.
        /// </value>
        public EbayUserAddressData EbayUserRegistrationAddressData { get; set; }

        /// <summary>
        /// Gets or sets the ebay user seller payment address data.
        /// </summary>
        /// <value>
        /// The ebay user seller payment address data.
        /// </value>
        public EbayUserAddressData EbayUserSellerPaymentAddressData { get; set; }

        /// <summary>
        /// Gets or sets the ebay user account data.
        /// </summary>
        /// <value>
        /// The ebay user account data.
        /// </value>
        public EbayUserAccountData EbayUserAccountData { get; set; }

        /// <summary>
        /// Gets or sets the additional user accounts.
        /// </summary>
        /// <value>
        /// The additional user accounts.
        /// </value>
        public EbayAdditionalUserAccountData[] AdditionalUserAccounts { get; set; }

        /// <summary>
        /// Gets or sets the ebay feedback.
        /// </summary>
        /// <value>
        /// The ebay feedback.
        /// </value>
        public EbayFeedbackData EbayFeedback { get; set; }

        /// <summary>
        /// Gets or sets the ebay ratings.
        /// </summary>
        /// <value>
        /// The ebay ratings.
        /// </value>
        public ICollection<EbayRatingData> EbayRatings { get; set; }

        /// <summary>
        /// Gets or sets the ebay feedbacks.
        /// </summary>
        /// <value>
        /// The ebay feedbacks.
        /// </value>
        public IEnumerable<EbayFeedbackItem> EbayFeedbackItems { get; set; }

        /// <summary>
        /// Gets or sets the ebay orders.
        /// </summary>
        /// <value>
        /// The ebay orders.
        /// </value>
        public IEnumerable<EbayOrderInfo> EbayOrders { get; set; } 
    }
}
