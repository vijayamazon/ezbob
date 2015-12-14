namespace Ezbob.Backend.Strategies.NewLoan.Collection
{
    using System;
    using DbConstants;
    using Ezbob.Backend.Models;
    using Ezbob.Database;
    using Ezbob.Backend.ModelsWithDB;

    /// <summary>
    /// Late Loan Cured
    /// </summary>
    public class LateLoanCured : AStrategy
    {
        private DateTime now;
        public override string Name { get { return "Late Loan Cured"; } }

        public override void Execute(){
            this.now = DateTime.UtcNow;
            try {
                this.now = DateTime.UtcNow;
                //-----------Change status to enabled for cured loans--------------------------------
                DB.ForEachRowSafe((sr, bRowsetStart) =>
                {
                    int customerID = sr["CustomerID"];
                    int loanID = sr["LoanID"];
                    int loanHistoryID = sr["LoanHistoryID"];
                    try{
                        HandleCuredLoan(customerID, loanID, loanHistoryID);
                    }
                    catch (Exception ex){
                        Log.Error(ex, "Failed to handle cured loan for customer {0}", customerID);
                    }
                    return ActionResult.Continue;
                }, "NL_CuredLoansGet", CommandSpecies.StoredProcedure);
            } catch (Exception ex) {
                NL_AddLog(LogType.Error, "Strategy Faild", null, null, ex.ToString(), ex.StackTrace);
            }
        }//Execute

        private void HandleCuredLoan(int customerID, int loanID, long loanHistoryID){
            //TODO Don't delete will be uncomment on fully NL activation.
            var collectionChangeStatus = new LateCustomerStatusChange(customerID, loanID, CollectionStatusNames.Enabled, CollectionType.Cured,this.now);
            collectionChangeStatus.Execute();
            new AddCollectionLog(new CollectionLog(){
                LoanID = loanID,
                TimeStamp = this.now,
                Type = collectionChangeStatus.Type.ToString(),
                CustomerID = customerID,
                LoanHistoryID = loanHistoryID,
                Comments = string.Empty,
                Method = CollectionMethod.ChangeStatus.ToString()
            }).Execute();
        }//HandleCuredLoan        

    }// class CollectionCuredLoans
} // namespace
