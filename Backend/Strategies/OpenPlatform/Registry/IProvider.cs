namespace Ezbob.Backend.Strategies.OpenPlatform.Registry
{
    public interface IProvider<out T> 
    {
        T GetNew();
    }
}
