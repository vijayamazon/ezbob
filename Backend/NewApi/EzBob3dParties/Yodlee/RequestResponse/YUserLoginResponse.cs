namespace EzBob3dParties.Yodlee.RequestResponse
{
    using EzBob3dParties.Yodlee.Models.Login;

    class YUserLoginResponse : YResponseBase
    {
        public UserContext userContext { get; set; }

        public string loginName { get; set; }
    }
}
