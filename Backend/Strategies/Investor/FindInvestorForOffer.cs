namespace Ezbob.Backend.Strategies.Investor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.Facade.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.Facade.Implement;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;
    using StructureMap;

    public class FindInvestorForOffer : AStrategy {
		public FindInvestorForOffer(int customerID, long cashRequestID) {
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
		}
		public override string Name { get { return "FindInvestorForOffer"; } }

		public override void Execute() {
			//todo implement find investor logic
			//todo for alpha only one investor should be found and he is full funded (100%) of offer
			//todo for alpha the logic for choosing one investor: was the last who assigned for a loan

			//var foundInvetsorID = DB.ExecuteScalar<int?>("SELECT TOP 1 InvestorID FROM I_Investor", CommandSpecies.Text);
			//if(!foundInvetsorID.HasValue) {
			//	IsFound = false;
			//	Log.Warn("No investors found in the system customer id: {0} cash request id: {1}", this.customerID, this.cashRequestID);
			//	return;
			//}

		    this.cashRequestID = 1;
            var container = InitContainer(typeof(InvestorService));
            var investorService = container.GetInstance<IInvestorService>();
		    //var filtered = investorsList.Where(x => x.IsActive);
            KeyValuePair<int,decimal>? investorParametersList = investorService.GetMatchedInvestor(this.cashRequestID);
                
                                        //x.InvestorSystemBalance.Max(m => m.InvestorSystemBalanceID) <= cashRequest.ManagerApprovedSum &&
                                        //systemParameters.DailyInvestmentAllowed - (cashRequest.ManagerApprovedSum + x.InvestorSystemBalance.Where(w => w.Timestamp.Date == DateTime.Today).Sum(y => Convert.ToDouble(y.TransactionAmount))) <= cashRequest.ManagerApprovedSum &&
                                        //systemParameters.GrageABudgets[cashRequest.Grade] <= cashRequest.ManagerApprovedSum);

            //DB.ExecuteNonQuery("I_OpenPlatformOfferSave", CommandSpecies.StoredProcedure,
            //    DB.CreateTableParameter("Tbl",
            //        new I_OpenPlatformOffer {
            //            CashRequestID = this.cashRequestID,
            //            InvestorID = foundInvetsorID.Value,
            //            InvestmentPercent = 1M //100% always in alpha
            //        })
            //    );



            if (investorParametersList !=null)
			    IsFound = true;
		}

        protected IContainer InitContainer(Type scanAssemblyOfType) {
            Container container = new Container();
            container.Configure(c => c.Scan(scanner => {
                scanner.AssemblyContainingType(scanAssemblyOfType);
                scanner.LookForRegistries();
            }));

            return container;
        }
        

		public bool IsFound { get; private set; }

		private readonly int customerID;
		private long cashRequestID;
	}
}
