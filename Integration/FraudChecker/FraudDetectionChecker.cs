namespace FraudChecker {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Fraud;
	using NHibernate;
	using StructureMap;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using Iesi.Collections.Generic;

	public class FraudDetectionChecker {
		public FraudDetectionChecker() {
			this.session = ObjectFactory.GetInstance<ISession>();
			this.customerRepository = ObjectFactory.GetInstance<CustomerRepository>();
		} // constructor

		/// <summary>
		/// run fraud all checks
		/// </summary>
		/// <param name="customerId">Customer.Id for check</param>
		/// <param name="mode">Mode to check</param>
		/// <returns></returns>
		public bool Check(int customerId, FraudMode mode = FraudMode.FullCheck) {
			var startDate = DateTime.UtcNow;
			var detections = new List<FraudDetection>();
			detections.AddRange(new InternalChecker(customerId, mode).Decide());
			detections.AddRange(new ExternalChecker(customerId).Decide());
			detections.AddRange(new BussinessChecker(customerId).Decide());

			SaveToDb(detections, startDate, customerId);
			return detections.Any();
		} // Check

		private void SaveToDb(IList<FraudDetection> fraudDetections, DateTime startDate, int customerId) {
			try {
				var customer = this.customerRepository.Get(customerId);

				var req = new FraudRequest {
					CheckDate = startDate,
					Customer = customer,
					FraudDetections = new HashedSet<FraudDetection>()
				};

				var count = fraudDetections.Count;

				for (var i = 0; i < count; i++) {
					var fraud = fraudDetections[i];
					fraud.Concurrence = ConcurrencePrepare(fraud);
					fraud.FraudRequest = req;
					req.FraudDetections.Add(fraud);
				} // for

				this.session.Save(req);

				if (count != 0) {
					//All other statuses will be set manually. See documentation
					//object customer is CustomerProxy so we need customer from DB to update him
					customer.FraudStatus = FraudStatus.FraudSuspect;
				} else {
					if (customer.FraudStatus == FraudStatus.FraudSuspect)
						customer.FraudStatus = FraudStatus.Ok;
				} // if

				this.customerRepository.SaveOrUpdate(customer);
			} catch (Exception ex) {
				log.Error(
					ex,
					"Failed to save fraud detections for customer {0}, number of detections {1}.",
					customerId,
					fraudDetections.Count()
				);
			} // try
		} // SaveToDb

		private static string ConcurrencePrepare(FraudDetection val) {
			if (val.ExternalUser != null) {
				return string.Format("{0} {1} (id={2})",
					val.ExternalUser.FirstName,
					val.ExternalUser.LastName,
					val.ExternalUser.Id);
			} // if

			string fullname;
			int id;

			if (val.InternalCustomer == null) { //for own check as DOB<21
				fullname = val.CurrentCustomer.PersonalInfo.Fullname;
				id = val.CurrentCustomer.Id;
			} else {
				fullname = val.InternalCustomer.PersonalInfo != null ? val.InternalCustomer.PersonalInfo.Fullname : "-";
				id = val.InternalCustomer.Id;
			} // if

			return string.Format("{0} (id={1})", fullname, id);
		} // ConcurrencePrepare

		private static readonly ASafeLog log = new SafeILog(typeof(FraudDetectionChecker));
		private readonly ISession session;
		private readonly CustomerRepository customerRepository;
	} // class FraudDetectionChecker
} // namespace