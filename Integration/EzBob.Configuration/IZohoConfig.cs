namespace EzBob.Configuration
{
    public interface IZohoConfig
    {
        string Token { get; }
        bool Enabled { get; }
    }
}