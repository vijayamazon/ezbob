IF OBJECT_ID('GetBasicCustomerData') IS NULL
	EXECUTE('CREATE PROCEDURE GetBasicCustomerData AS SELECT 1')
GO

ALTER PROCEDURE GetBasicCustomerData
@CustomerId INT
AS
BEGIN
	DECLARE @NumOfLoans INT

	SELECT 
		@NumOfLoans = count(1) 
	FROM 
		Loan 
	WHERE 
		CustomerId = @CustomerId
		
	SELECT
		Id,
		FirstName,
		Surname,
		Fullname,
		Name AS Mail,
		IsOffline,
		@NumOfLoans AS NumOfLoans,
		RefNumber,
		MobilePhone,
		DaytimePhone
	FROM
		Customer
	WHERE
		Id = @CustomerId
END
GO
