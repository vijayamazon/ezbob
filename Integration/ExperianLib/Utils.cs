using System;
using System.Text;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using ExperianLib.IdIdentityHub;
using StructureMap;
using log4net;

namespace ExperianLib
{
    public class Utils
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Utils));
        //-----------------------------------------------------------------------------------
        public static MP_ServiceLog WriteLog<TX, TY>(TX input, TY output, string type, int customerId)where TX : class where TY : class
        {
            return WriteLog(XSerializer.Serialize(input), XSerializer.Serialize(output), type, customerId);
        }

        //-----------------------------------------------------------------------------------
        public static MP_ServiceLog WriteLog(string input, string output, string type, int customerId)
        {
            var logEntry = new MP_ServiceLog
                               {
                                   InsertDate = DateTime.Now,
                                   RequestData = input,
                                   ResponseData = output,
                                   ServiceType = type,
                               };
            try
            {
                var repoLog = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ServiceLog>>();
                var customerRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<Customer>>();

                logEntry.Customer = customerRepo.Get(customerId);

                Log.DebugFormat("Input data was: {0}", logEntry.RequestData);
                Log.DebugFormat("Output data was: {0}", logEntry.ResponseData);
                repoLog.SaveOrUpdate(logEntry);
            }
            catch (Exception)
            {
            }

            return logEntry;
        }
        //-----------------------------------------------------------------------------------
        public static void TryRead(Action a, string key, StringBuilder errors)
        {
            try
            {
                a();
            }
            catch
            {
                errors.AppendLine("Can`t read value for: " + key);
            }
        }

        public static void TryRead(Func<object> action)
        {
            try
            {
                action();
            }
            catch
            {
                
            }
        }
    }
}
