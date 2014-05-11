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
		c.IsTest,
		a.Postcode,
		a.Town AS City
	FROM
		Customer c LEFT JOIN CustomerAddress a ON c.Id=a.CustomerId
	WHERE
		c.Id = @CustomerId
	AND
		a.addressType=1	
END
GO
