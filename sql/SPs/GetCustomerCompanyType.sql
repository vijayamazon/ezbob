SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetCustomerCompanyType') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerCompanyType AS SELECT 1')
GO

ALTER PROCEDURE GetCustomerCompanyType
@CompanyID INT,
@TypeOfBusinessName NVARCHAR(50) OUTPUT
AS
BEGIN
	SET @TypeOfBusinessName = NULL

	SELECT
		@TypeOfBusinessName = c.TypeOfBusiness
	FROM
		Company c
	WHERE
		c.Id = @CompanyID
END
GO
