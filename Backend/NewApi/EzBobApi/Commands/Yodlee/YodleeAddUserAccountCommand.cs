namespace EzBobApi.Commands.Yodlee
{
    using EzBobCommon.NSB;

    public class YodleeAddUserAccountCommand : CommandBase
    {
        public int CustomerId { get; set; }
        public int ContentServiceId { get; set; }
    }
}
