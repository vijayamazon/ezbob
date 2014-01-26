IF OBJECT_ID('AV_GetCustomerBirthDate') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetCustomerBirthDate AS SELECT 1')
GO

ALTER PROCEDURE AV_GetCustomerBirthDate
@CustomerId INT
AS
BEGIN 
	SELECT DateOfBirth FROM Customer WHERE Id=@CustomerId
END 