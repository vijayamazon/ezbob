namespace Ezbob.Backend.Strategies.OpenPlatform.Registry
{
    using StructureMap;

    public class Provider<T> : IProvider<T>
        where T : class, new()
    {
        private readonly IContext context;

        public Provider(IContext context)
        {
            this.context = context;
        }
        public T GetNew()
        {
            T instance = new T();
            this.context.BuildUp(instance);
            return instance;
        }
    }
}
