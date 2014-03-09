using Scorto.Configuration;
using log4net.Appender;

namespace Log4NetCustomAppenders
{
    public class EzbobAdoNetAppender : AdoNetAppender
    {
        private readonly string _connectionString;

        public EzbobAdoNetAppender()
        {
            var configuration = ConfigurationRoot.GetConfiguration();
            _connectionString = configuration.DbLib.ConnectionString;
        }

        public new string ConnectionString
        {
            get { return base.ConnectionString; }
            set { base.ConnectionString = _connectionString; }
        }
    }
}