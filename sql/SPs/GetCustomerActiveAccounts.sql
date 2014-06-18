IF OBJECT_ID('GetCustomerActiveAccounts') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerActiveAccounts AS SELECT 1')
GO

ALTER PROCEDURE GetCustomerActiveAccounts
@CustomerId INT
AS
BEGIN	
	SELECT
		LastUpdateDate,
		StatusCode1,
		StatusCode2,
		StatusCode3,
		StatusCode4,
		StatusCode5,
		StatusCode6,
		StatusCode7,
		StatusCode8,
		StatusCode9,
		StatusCode10,
		StatusCode11,
		StatusCode12
	FROM
		FinancialAccounts
	WHERE
		CustomerId = @CustomerId
END
GO
