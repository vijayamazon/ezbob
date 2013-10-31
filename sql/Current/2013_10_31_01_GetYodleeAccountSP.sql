IF OBJECT_ID('GetYodleeAccount') IS NULL
	EXECUTE('CREATE PROCEDURE GetYodleeAccount AS SELECT 1')
GO

ALTER PROCEDURE GetYodleeAccount
	@IsCustomerId BIT,
	@Id INT
AS
BEGIN
	IF @IsCustomerId = 1 
		SELECT Username, Password 
		FROM YodleeAccounts 
		WHERE CustomerId=@Id
	ELSE 
		SELECT Username, Password 
		FROM YodleeAccounts 
		WHERE Id=@Id
END
GO
 	

