SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetCustomerCompanyType') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerCompanyType AS SELECT 1')
GO

ALTER PROCEDURE GetCustomerCompanyType
@CustomerID INT,
@Now DATETIME,
@TypeOfBusiness NVARCHAR(50) OUTPUT
AS
BEGIN
	SET @TypeOfBusiness = NULL

	DECLARE @CompanyID INT = dbo.udfGetCustomerCompanyID(@CustomerID, @Now)

	SELECT
		@TypeOfBusiness = c.TypeOfBusiness
	FROM
		Company c
	WHERE
		c.Id = @CompanyID
END
GO
