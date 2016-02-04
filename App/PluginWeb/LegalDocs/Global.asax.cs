namespace LegalDocs {
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using ConfigManager;
    using Ezbob.Database;
    using Ezbob.Database.Pool;
    using Ezbob.Logger;
    using LegalDocs.Code.Filters;

    public class MvcApplication : HttpApplication {
        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            InitOnStart();
        }

        private static bool _isInitialized = false;
        private static SafeILog log;
        private static SafeILog Log {
            get {
                if (log == null)
                    log = new SafeILog(typeof(MvcApplication));

                return log;
            } // get
        } // Log


        private static void RegisterGlobalFilters(GlobalFilterCollection filters) {

            filters.Add(
                new LegalDocsAuthorizeAttribute(),
                1
            );

        } // RegisterGlobalFilters

        private void InitOnStart() {
            Init();

            if (_isInitialized)
                return;

            lock (this) {
                if (_isInitialized)
                    return;
                try {
                    new Log4Net().Init();
                    Log.NotifyStart();
                    var db = DbConnectionGenerator.Get(Log);
                    CurrentValues.Init(
                        db,
                        Log,
                        oLimitations => oLimitations.UpdateWebSiteCfg("Ezbob.Web")
                    );

                    DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
                    AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);
                    RegisterGlobalFilters(GlobalFilters.Filters);
                }
                catch (Exception ex) {
                    Log.Error(ex);
                    throw;
                } // try
                _isInitialized = true;
            } // lock
        } // InitOnStart
    }
}
