namespace EzBob3dParties.Amazon
{
    using EzBob3dParties.Amazon.RatingScraper;
    using EzBob3dPartiesApi.Amazon;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref="AmazonGetCustomerInfo3dPartyCommand"/>.
    /// </summary>
    public class AmazonGetCustomerInfoHandler : HandlerBase<AmazonGetCustomerInfo3dPartyCommandResponse>, IHandleMessages<AmazonGetCustomerInfo3dPartyCommand> {

        [Injected]
        public IAmazonCustomerRating CustomerRating { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(AmazonGetCustomerInfo3dPartyCommand command) {

            var ratingInfo = await CustomerRating.GetRating(command.SellerId);
            InfoAccumulator info = new InfoAccumulator();

            SendReply(info, command, resp => resp.BusinessName = ratingInfo.Name);
        }
    }
}
