﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezbob.Backend.Strategies.Investor
{
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Database;

    public class LoadInvestors : AStrategy 
    {
        
        public override string Name { get { return "LoadInvestors"; } }
        
        public override void Execute()
        {
           LoadFromDb();
          
           
        } //Execute
       

        private void  LoadFromDb()
        {
            try {
                Result = new List<OneInvestorModel>();

                DB.ForEachRowSafe(FillOneInvestor,"UwGridInvestors", CommandSpecies.StoredProcedure, new QueryParameter("WithTest", false));
                
            }
            catch(Exception ex) {
                Log.Warn(ex, "Fail to load investors");
                throw;
            }
        } //load From db

        private ActionResult FillOneInvestor(SafeReader sr, bool arg2) {

            Result.Add(new OneInvestorModel
            {
                InvestorID = sr["InvestorID"],
                InvestorType = sr["InvestorType"],
                InvestorTypeID = sr["InvestorTypeID"],
                CompanyName = sr["CompanyName"],
                IsActive = sr["IsActive"]
            });
            return ActionResult.Continue;
        } //FillOneInvestor



        public List<OneInvestorModel> Result { get; set; }
    }//end of LoadInvestors class
}
