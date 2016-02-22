SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetCustomerCompanyID') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerCompanyID AS SELECT 1')
GO

ALTER PROCEDURE GetCustomerCompanyID
@CustomerID INT,
@Now DATETIME,
@CompanyID INT OUTPUT,
@TypeOfBusiness NVARCHAR(50) OUTPUT
AS
BEGIN
	SET @CompanyID = dbo.udfGetCustomerCompanyID(@CustomerID, @Now)

	SELECT @TypeOfBusiness = TypeOfBusiness FROM Company WHERE Id = @CompanyID
END
GO
