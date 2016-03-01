IF OBJECT_ID('NL_MigrationCRDecisionOffer') IS NULL
	EXECUTE('CREATE PROCEDURE NL_MigrationCRDecisionOffer AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE NL_MigrationCRDecisionOffer 	
AS
BEGIN
	
	SET NOCOUNT ON;

   select --top 20 
   Id from Loan l left join NL_Loans nl on nl.OldLoanID=l.Id and l.Id=null and l.Modified=0;

END
GO