IF OBJECT_ID('NL_LoanFeesOldIDUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanFeesOldIDUpdate AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[NL_LoanFeesOldIDUpdate]
AS
BEGIN
	
	SET NOCOUNT ON;

    UPDATE
	   nlf
	SET
		nlf.[OldFeeID] = charges.Id
	FROM
		[dbo].[NL_LoanFees] nlf
	INNER JOIN
		(select ch.Id, f.LoanFeeID, f.LoanID, f.OldFeeID
	 from [dbo].[LoanCharges]ch join Loan l on l.Id=ch.LoanId join NL_Loans nl on nl.OldLoanID=l.Id join NL_LoanFees f on f.LoanID=nl.LoanID
	  where 	
	  ch.Amount=f.Amount and datediff(DAY, ch.Date, f.AssignTime)=0 and f.OldFeeID is null 
	  and ch.ConfigurationVariableId=(select dbo.udfNL_FeeTypeToConfVariable(f.LoanFeeTypeID))) as charges
	ON
		nlf.LoanFeeID = charges.LoanFeeID
	WHERE
		nlf.LoanID = charges.LoanID;
END



