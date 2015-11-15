IF OBJECT_ID('NL_UpdateLoanStatusToLate') IS NOT NULL
	DROP PROCEDURE NL_UpdateLoanStatusToLate
GO

CREATE PROCEDURE [dbo].[NL_UpdateLoanStatusToLate] 
@LoanId int,
@CustomerId int,
@PaymentStatus varchar(50),
@LoanStatus varchar(50)
AS
BEGIN
	UPDATE [dbo].[NL_Loans]
	  SET  LoanStatusID = (select LoanStatusID from nl_loanstatuses where loanstatus = @LoanStatus)
		WHERE LoanID = @LoanId
	UPDATE [dbo].Customer
	  SET  CreditResult = @LoanStatus,
			[IsWasLate] = 1		
	WHERE Id = @CustomerId
	UPDATE [dbo].[NL_Payments]
	  SET  [PaymentStatusID] = (select LoanStatusID from nl_loanstatuses where loanstatus = @PaymentStatus)
	  where LoanID = @LoanId
SET NOCOUNT ON;
SELECT @@IDENTITY
END