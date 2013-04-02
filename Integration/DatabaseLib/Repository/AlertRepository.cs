using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;
using log4net;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public class AlertRepository : NHibernateRepositoryBase<MP_Alert>
	{
        static readonly ILog Log = LogManager.GetLogger(typeof(AlertRepository));
        public AlertRepository(ISession session)
            : base(session)
        {
        }

        public IQueryable<MP_Alert> GetAlertsByCustomer(int customerId, bool showPassed)
        {
            var qAlerts = (from a in GetAll() where (a.Customer.Id == customerId) select a);
            var maxStratDate = qAlerts.Max(x => x.StrategyStartedDate);
            qAlerts = showPassed ? qAlerts.Where(a => a.AlertSeverity == AlertsSeverity.Passed && a.StrategyStartedDate == maxStratDate) : qAlerts.Where(a => a.AlertSeverity != AlertsSeverity.Passed && a.StrategyStartedDate == maxStratDate);
            return qAlerts;
        }

	    //-----------------------------------------------------------------------------------
        public void AddAlert(int customerId, string type, string text, AlertsSeverity severity, DateTime? strategyStartedDate, int? directorId = null)
		{
            var customer = _session.Load<Customer>(customerId);
		    foreach (var alertData in text.Split(new []{'#'}, StringSplitOptions.RemoveEmptyEntries))
		    {
		        var tokens = alertData.Split(new [] {"||"}, StringSplitOptions.RemoveEmptyEntries);
                
                var alert = new MP_Alert
		        {
                    Customer = customer,
                    AlertType = type,
                    AlertText = tokens[0],
                    AlertSeverity = severity,
                    Status = "-",
                    DirectorId = directorId,
                    StrategyStartedDate = strategyStartedDate
		        };

                if (tokens.Length == 2)
                {
                    alert.ActionToMake = tokens[1];
                }
                SaveOrUpdate(alert);
		    }
		}
        
        public List<MP_Alert> GetByCustomerId(int customerId)
        {
            var alerts = GetAll().Where(a => a.Customer.Id == customerId).ToList();

            var maxDate = alerts.OrderByDescending(a => a.StrategyStartedDate)
                                    .Select(a => a.StrategyStartedDate)
                                    .FirstOrDefault();
            
            return maxDate == null ? 
                new List<MP_Alert>() : 
                (from alert in alerts where alert.StrategyStartedDate == maxDate select alert).ToList();
        }
	}
}