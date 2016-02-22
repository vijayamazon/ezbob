SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CreateCustomerRequestedLoan') IS NULL
	EXECUTE('CREATE PROCEDURE CreateCustomerRequestedLoan AS SELECT 1')
GO

ALTER PROCEDURE CreateCustomerRequestedLoan
@CustomerID INT,
@Created DATETIME,
@Amount INT,
@Term INT
AS
BEGIN
	INSERT INTO CustomerRequestedLoan (Created, CustomerId, Amount, Term)
		VALUES (@Created, @CustomerID, @Amount, @Term)
END
GO
