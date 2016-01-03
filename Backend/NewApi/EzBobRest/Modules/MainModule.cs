namespace EzBobRest.Modules
{
    using Nancy;
    using Nancy.Security;

    public class MainModule : NancyModule
    {
        public MainModule() {
//            this.RequiresHttps();
//            this.RequiresMSOwinAuthentication();
            Get["/"] = o =>
            {
                var user = this.Context.GetMSOwinUser();
                return "ku";
            };
        }
    }
}
