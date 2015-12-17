SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanIdByOldLoanId') IS NOT NULL
	DROP PROCEDURE  NL_LoanIdByOldLoanId
GO

CREATE PROCEDURE [dbo].[NL_LoanIdByOldLoanId]
 @OldLoanID INT
 AS
 BEGIN
	 SET NOCOUNT ON;
	 declare @LoanID bigint;

	 set @LoanID = (SELECT TOP 1 LoanID FROM NL_loans WHERE OldLoanID=@OldLoanID);

	 if @LoanID is null
		select CAST(0 as bigint);
	else
		select @LoanID;

 END
