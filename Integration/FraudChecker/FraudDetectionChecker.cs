using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Fraud;
using NHibernate;
using StructureMap;

namespace FraudChecker
{
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Backend.Models;
	using Iesi.Collections.Generic;

	public class FraudDetectionChecker
	{
		private readonly ISession _session;
		private readonly InternalChecker _internalChecker;
		private readonly ExternalChecker _externalChecker;
		private readonly BussinessChecker _bussinessChecker;
		private readonly CustomerRepository _customerRepository;
		public FraudDetectionChecker()
		{
			_bussinessChecker = new BussinessChecker();
			_internalChecker = new InternalChecker();
			_externalChecker = new ExternalChecker();
			_session = ObjectFactory.GetInstance<ISession>();
			_customerRepository = ObjectFactory.GetInstance<CustomerRepository>();
		}

		/// <summary>
		/// run fraud all checks
		/// </summary>
		/// <param name="customerId">Customer.Id for check</param>
		/// <param name="mode">Mode to check</param>
		/// <returns></returns>
		public bool Check(int customerId, FraudMode mode = FraudMode.FullCheck)
		{
			var startDate = DateTime.UtcNow;
			var detections = new List<FraudDetection>();
			detections.AddRange(_internalChecker.InternalSystemDecision(customerId, mode));
			detections.AddRange(_externalChecker.ExternalSystemDecision(customerId));
			detections.AddRange(_bussinessChecker.SpecialBussinesRulesSystemDecision(customerId));

			SaveToDb(detections, startDate, customerId);
			return detections.Any();
		}

		private void SaveToDb(IList<FraudDetection> fraudDetections, DateTime startDate, int customerId)
		{
			var req = new FraudRequest
				{
					CheckDate = startDate,
					Customer = _customerRepository.Get(customerId),
					FraudDetections = new HashedSet<FraudDetection>()
				};
			var count = fraudDetections.Count;
			for (var i = 0; i < count; i++)
			{
				var fraud = fraudDetections[i];
				fraud.Concurrence = Helper.ConcurrencePrepare(fraud);
				fraud.FraudRequest = req;
				req.FraudDetections.Add(fraud);

			}
			_session.Save(req);
			_session.Flush();
			_session.Clear();

			var currentCustomer = _session.Get<Customer>(customerId);
			if (count != 0)
			{
				//All other statuses will be set manually. See documentation
				//object customer is CustomerProxy so we need customer from DB to update him
				currentCustomer.FraudStatus = FraudStatus.FraudSuspect;
			}
			else
			{
				if (currentCustomer.FraudStatus == FraudStatus.FraudSuspect)
				{
					currentCustomer.FraudStatus = FraudStatus.Ok;
				}
			}

			_session.Save(currentCustomer);
			_session.Flush();

		}

	}

}