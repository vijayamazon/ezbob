namespace Ezbob.Backend.Strategies.Investor {
    using System;
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.Facade.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.Facade.Implement;
    using Ezbob.Database;
    using StructureMap;

    public class FindInvestorForOffer : AStrategy {
		public FindInvestorForOffer(int customerID, long cashRequestID) {
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
		}//ctor

		public override string Name { get { return "FindInvestorForOffer"; } }

		public override void Execute() {
			//for alpha only one investor should be found and he is full funded (100%) of offer
			//for alpha the logic for choosing one investor: was the last who assigned for a loan

            var container = InitContainer(typeof(InvestorService));
            var investorService = container.GetInstance<IInvestorService>();
			try {
				KeyValuePair<int, decimal>? investorParametersList = investorService.GetMatchedInvestor(this.cashRequestID);

				if (investorParametersList.HasValue) {
					IsFound = true;
					DB.ExecuteNonQuery("I_OpenPlatformOfferSave", CommandSpecies.StoredProcedure,
						DB.CreateTableParameter("Tbl", new I_OpenPlatformOffer {
							CashRequestID = this.cashRequestID,
							InvestorID = investorParametersList.Value.Key,
							InvestmentPercent = investorParametersList.Value.Value
						})
						);
				} //if
			} catch (Exception ex) {
				Log.Error(ex, "Failed to find investor for offer {0} for customer {1}", this.cashRequestID, this.customerID);
				IsFound = false;
			}
		}//Execute

        protected IContainer InitContainer(Type scanAssemblyOfType) {
            Container container = new Container();
	        container.Configure(c => c.Scan(scanner => {
		        scanner.AssemblyContainingType(scanAssemblyOfType);
		        scanner.LookForRegistries();
	        }));

            return container;
        }//InitContainer
        
		public bool IsFound { get; private set; }

		private readonly int customerID;
		private readonly long cashRequestID;
	}//FindInvestorForOffer
}//ns
