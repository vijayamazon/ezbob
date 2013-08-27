IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateLoanStatusToLate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateLoanStatusToLate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLoanStatusToLate] 
(@LoanId int,
@CustomerId int,
@PaymentStatus varchar(50),
@LoanStatus varchar(50))

AS
BEGIN

UPDATE [dbo].[Loan]
  SET  [Status] = @LoanStatus,
		[PaymentStatus] = @PaymentStatus

WHERE Id = @LoanId

UPDATE [dbo].Customer
  SET  CreditResult = @LoanStatus,
		[IsWasLate] = 1	
		
WHERE Id = @CustomerId



SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
