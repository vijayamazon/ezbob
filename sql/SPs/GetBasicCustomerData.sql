IF OBJECT_ID('GetBasicCustomerData') IS NULL
	EXECUTE('CREATE PROCEDURE GetBasicCustomerData AS SELECT 1')
GO

ALTER PROCEDURE GetBasicCustomerData
@CustomerId INT
AS
BEGIN
	DECLARE @NumOfLoans INT

	SELECT 
		@NumOfLoans = COUNT(1)
	FROM 
		Loan l
	WHERE 
		l.CustomerId = @CustomerId

	SELECT
		c.Id,
		c.FirstName,
		c.Surname,
		c.Fullname,
		c.Name AS Mail,
		c.IsOffline,
		@NumOfLoans AS NumOfLoans,
		c.RefNumber,
		c.MobilePhone,
		c.DaytimePhone,
		c.IsTest
	FROM
		Customer c
	WHERE
		c.Id = @CustomerId
END
GO