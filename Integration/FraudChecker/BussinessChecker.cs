namespace FraudChecker {
	using System;
	using System.Collections.Generic;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Fraud;
	using NHibernate;
	using StructureMap;
	using Ezbob.Utils;
	using EzServiceAccessor;

	class BussinessChecker {
		public BussinessChecker(int customerID) {
			this.session = ObjectFactory.GetInstance<ISession>();
			this.fraudDetections = new List<FraudDetection>();
			this.customerID = customerID;
		} // constructor


		public List<FraudDetection> Decide() {
			log.Info("Starting special bussines rules system check for customerId {0}.", customerID);

			this.customer = this.session.Load<Customer>(this.customerID);

			if(this.customer==null)
				return this.fraudDetections;

			CompanySeniority();

			log.Info("Finishing special bussines rules system check for customerId {0}.", customerID);
			return this.fraudDetections;
		} // SpecialBussinesRulesSystemDecision

		private void CompanySeniority() {

			DateTime now = DateTime.UtcNow;
			int maxSeniorityYears = ConfigManager.CurrentValues.Instance.MaxSeniorityYears;

			bool finishedWizard = this.customer.WizardStep.TheLastOne;

			if (!finishedWizard)
				return;

			bool isLimited = this.customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited;

			if (isLimited) {
				this.incorporationDate = ObjectFactory.GetInstance<IEzServiceAccessor>().CheckLtdCompanyCache(1, this.customer.Company.ExperianRefNum).IncorporationDate;
			} 
			else {
				this.incorporationDate = ObjectFactory.GetInstance<IEzServiceAccessor>().GetNonLimitedData(1, this.customer.Company.ExperianRefNum).IncorporationDate;
			}

			this.companySeniority = MiscUtils.GetFullYears(this.incorporationDate ?? now);

			bool isBeyondMaxSeniority = this.companySeniority > maxSeniorityYears;
			
			if (isBeyondMaxSeniority)
				this.fraudDetections.Add(
					Helper.CreateDetection(
						"Customer CompanySeniority",
						this.customer,
						null,
						"Company Seniority Above " + maxSeniorityYears,
						null,this.companySeniority.ToString()
						)
					);
		}//CompanySeniority
		
		private readonly ISession session;
		private readonly List<FraudDetection> fraudDetections;
		private readonly int customerID;
		
		private int companySeniority;
		private Customer customer;
		private DateTime? incorporationDate;

		private static readonly ASafeLog log = new SafeILog(typeof(ExternalChecker));

	} // class BussinessChecker
} // namespace
