using System;
using EZBob.DatabaseLib.Model.Loans;

namespace EzBob.Models
{
    [Serializable]
    public class LoanChargesModel : LoanCharge
    {
        public static LoanChargesModel FromCharges(LoanCharge loanCharge)
        {
            return loanCharge != null
                       ? new LoanChargesModel
                             {
                                 Amount = loanCharge.Amount,
                                 ChargesType = ConfigurationVariableModel.FromConfigurationVariable(loanCharge.ChargesType),
                                 Date = loanCharge.Date,
                                 Id = loanCharge.Id,
                                 State = loanCharge.State ?? "Active",
                                 AmountPaid = loanCharge.AmountPaid,
                                 Description = loanCharge.GetDescription()
                             }
                       : null;
        }
    }
}