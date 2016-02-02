namespace EzBobApi.Commands.SimplyPostcode
{
    using EzBobCommon.NSB;
    using EzBobModels.SimplyPostcode;

    /// <summary>
    /// Response to <see cref="SimplyPostcodeGetAddressDetailsCommand"/>.
    /// </summary>
    public class SimplyPostcodeGetAddressDetailsCommandResponse : CommandResponseBase
    {
        public SimplyPostcodeDatailedAddress Address { get; set; }
    }
}
