namespace TeamCityEngine
{
    using System;
    using System.Configuration;
    using TeamCityData;

    public class TeamCityManager
    {
        public static volatile TeamCityData instance;
        private static readonly object syncRoot = new Object();

        public static TeamCityData Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        var url = ConfigurationManager.AppSettings["teamCityUrl"];
                        var user = ConfigurationManager.AppSettings["teamCityUser"];
                        var password = ConfigurationManager.AppSettings["teamCityPassword"];
                        instance = new TeamCityData(url, false);
                        instance.Connect(user, password);
                    }
                }

                return instance;
            }
        }

    }
}