using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Fraud;
using NHibernate;
using StructureMap;

namespace FraudChecker {
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Backend.Models;
	using Iesi.Collections.Generic;
	using log4net;

	public class FraudDetectionChecker {
		private readonly ISession _session;
		private readonly InternalChecker _internalChecker;
		private readonly ExternalChecker _externalChecker;
		private readonly BussinessChecker _bussinessChecker;
		private readonly CustomerRepository _customerRepository;
		protected static ILog Log = LogManager.GetLogger(typeof(FraudDetectionChecker));

		public FraudDetectionChecker() {
			this._bussinessChecker = new BussinessChecker();
			this._internalChecker = new InternalChecker();
			this._externalChecker = new ExternalChecker();
			this._session = ObjectFactory.GetInstance<ISession>();
			this._customerRepository = ObjectFactory.GetInstance<CustomerRepository>();
		}

		/// <summary>
		/// run fraud all checks
		/// </summary>
		/// <param name="customerId">Customer.Id for check</param>
		/// <param name="mode">Mode to check</param>
		/// <returns></returns>
		public bool Check(int customerId, FraudMode mode = FraudMode.FullCheck) {
			var startDate = DateTime.UtcNow;
			var detections = new List<FraudDetection>();
			detections.AddRange(this._internalChecker.InternalSystemDecision(customerId, mode));
			detections.AddRange(this._externalChecker.ExternalSystemDecision(customerId));
			detections.AddRange(this._bussinessChecker.SpecialBussinesRulesSystemDecision(customerId));

			SaveToDb(detections, startDate, customerId);
			return detections.Any();
		}

		private void SaveToDb(IList<FraudDetection> fraudDetections, DateTime startDate, int customerId) {
			try {
				var customer = this._customerRepository.Get(customerId);
				var req = new FraudRequest {
					CheckDate = startDate,
					Customer = customer,
					FraudDetections = new HashedSet<FraudDetection>()
				};
				var count = fraudDetections.Count;
				for (var i = 0; i < count; i++) {
					var fraud = fraudDetections[i];
					fraud.Concurrence = Helper.ConcurrencePrepare(fraud);
					fraud.FraudRequest = req;
					req.FraudDetections.Add(fraud);

				}
				this._session.Save(req);

				if (count != 0) {
					//All other statuses will be set manually. See documentation
					//object customer is CustomerProxy so we need customer from DB to update him
					customer.FraudStatus = FraudStatus.FraudSuspect;
				} else {
					if (customer.FraudStatus == FraudStatus.FraudSuspect) {
						customer.FraudStatus = FraudStatus.Ok;
					}
				}

				this._customerRepository.SaveOrUpdate(customer);
			} catch (Exception ex) {
				Log.ErrorFormat("Failed to save fraud detections for customer {0} number of detections {1}\n{2}", customerId, fraudDetections.Count(), ex);
			}
		}
	}
}