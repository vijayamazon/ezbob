namespace EzBobApi.Commands.Yodlee
{
    using EzBobCommon.NSB;

    public class YodleeLoginUserCommandResponse : CommandResponseBase
    {
        public string UserToken { get; set; }
    }
}
