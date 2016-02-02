namespace EzBob3dPartiesApi.SimplyPostcode
{
    using EzBobCommon.NSB;
    using EzBobModels.SimplyPostcode;

    /// <summary>
    /// Response to <see cref="SimplyPostcodeGetAddressDetails3dPartyCommand"/>.
    /// </summary>
    public class SimplyPostcodeGetAddressDetails3dPartyCommandResponse : CommandResponseBase
    {
        public SimplyPostcodeDatailedAddress Address { get; set; }
    }
}
