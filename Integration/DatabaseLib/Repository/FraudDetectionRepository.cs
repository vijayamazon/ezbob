using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Fraud;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Repository
{
	using System;
	using System.ComponentModel;

	public class FraudDetectionRepository : NHibernateRepositoryBase<FraudDetection>
	{
		public FraudDetectionRepository(ISession session)
			: base(session)
		{
		}

		public IEnumerable<FraudDetection> GetByCustomerId(int customerId)
		{
			return GetAll().Where(x => x.CurrentCustomer.Id == customerId);
		}

		public IEnumerable<FraudDetection> GetLastDetections(int customerId, out DateTime? lastDateCheck)
		{
			var lastCheck = Session.Query<FraudRequest>().OrderByDescending(x=> x.CheckDate).FirstOrDefault(x => x.Customer.Id == customerId);
			if (lastCheck != null)
			{
				lastDateCheck = lastCheck.CheckDate;
				return lastCheck.FraudDetections.ToList();
			}

			lastDateCheck = null;
			return new List<FraudDetection>();

		}
	}
}
