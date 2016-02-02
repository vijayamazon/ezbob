namespace EzBob3dPartiesApi.SimplyPostcode
{
    using System.Collections.Generic;
    using EzBobCommon.NSB;
    using EzBobModels.SimplyPostcode;

    /// <summary>
    /// Resposne to <see cref="SimplyPostcodeGetAddresses3dPartyCommand"/>.
    /// </summary>
    public class SimplyPostcodeGetAddresses3dPartyCommandResponse : CommandResponseBase
    {
        public IList<SimplyPostcodeAddress> Addresses { get; set; }
    }
}
