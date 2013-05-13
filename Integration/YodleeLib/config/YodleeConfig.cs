namespace YodleeLib.config
{
    using StructureMap;

    public class YodleeConfig
    {
        public static IYodleeMarketPlaceConfig _Config = ObjectFactory.GetInstance<IYodleeMarketPlaceConfig>();  
    }
}
