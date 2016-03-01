begin
WITH cte_FeeOldIds (LoanFeeID, OldFeeID)
AS(
select f.LoanFeeID, ch.Id as OldFeeID
from NL_LoanFees f join NL_Loans nl on f.LoanID=nl.LoanID left join LoanCharges ch on nl.OldLoanID=ch.LoanId
where datediff(DAY, f.AssignTime, ch.Date)=0 and f.Amount=ch.Amount and [dbo].[udfNL_FeeTypeToConfVariable](f.LoanFeeTypeID)=ch.ConfigurationVariableId
)
--select * from cte_FeeOldIds;
	UPDATE NL_LoanFees SET NL_LoanFees.OldFeeID = cte_FeeOldIds.OldFeeID FROM NL_LoanFees INNER JOIN cte_FeeOldIds ON (NL_LoanFees.LoanFeeID = cte_FeeOldIds.LoanFeeID);
end;

-- to check
select f.Amount, f.AssignTime, f.CreatedTime, f.LoanFeeID, f.LoanFeeTypeID, f.LoanID, f.Notes, f.OldFeeID, 
nl.LoanID, nl.OldLoanID,
ch.Amount, ch.AmountPaid, ch.ConfigurationVariableId, ch.Date as ChargeDate, ch.Description, ch.Id, ch.State
from NL_LoanFees f join NL_Loans nl on f.LoanID=nl.LoanID left join LoanCharges ch on nl.OldLoanID=ch.LoanId
where datediff(DAY, f.AssignTime, ch.Date)=0 and f.Amount=ch.Amount and [dbo].[udfNL_FeeTypeToConfVariable](f.LoanFeeTypeID)=ch.ConfigurationVariableId