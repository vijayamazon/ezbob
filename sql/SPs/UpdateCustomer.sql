IF OBJECT_ID('UpdateCustomer') IS NOT NULL
	DROP PROCEDURE UpdateCustomer
GO

CREATE PROCEDURE [dbo].[UpdateCustomer] 
@CustomerId int,
@IsWasLate bit = null,
@LoanStatus varchar(50) = null
AS
BEGIN
	UPDATE [dbo].Customer 
	SET
	[CreditResult] = ISNULL(@LoanStatus, CreditResult),
	[IsWasLate] = ISNULL(@IsWasLate, IsWasLate)	
	WHERE [Id] = @CustomerId;			
END

