namespace EzBobRest.Init {
    using Owin;

    public interface IStartup {
        void Configuration(IAppBuilder app);
    }
}
