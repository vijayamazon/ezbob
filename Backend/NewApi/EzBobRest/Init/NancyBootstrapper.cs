namespace EzBobRest.Init {
    using EzBobCommon;
    using Nancy.Bootstrappers.StructureMap;
    using Nancy.ModelBinding;
    using Nancy.Serialization.JsonNet;
    using StructureMap;

    /// <summary>
    /// Configures Nancy to use the application's DI container
    /// Replaces Nancy's default json serialization to use Json.net
    /// </summary>
    public class NancyBootstrapper : StructureMapNancyBootstrapper {
        [Injected]
        public IContainer Container { get; set; }

        /// <summary>
        /// Configure the application level container with any additional registrations.
        /// </summary>
        /// <param name="container">Container instance</param>
        protected override void ConfigureApplicationContainer(IContainer container) {
            base.ConfigureApplicationContainer(container);

            //changes Nancy's default json serializer to JSON.NET
            //the implementation is taken from Nancy.Serialization.JsonNet package

            container.Configure(c => c.For<IBodyDeserializer>()
                .Use<JsonNetBodyDeserializer>());

            container.Configure(c => c.For<Nancy.ISerializer>()
                .Use<JsonNetSerializer>());
        }

        /// <summary>
        /// Shares with Nancy the application's DI container
        /// </summary>
        /// <returns>
        /// Container instance
        /// </returns>
        protected override IContainer GetApplicationContainer() {
            return Container;
        }
    }
}
