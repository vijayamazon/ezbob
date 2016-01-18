SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetCustomerCompanyID') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerCompanyID AS SELECT 1')
GO

ALTER PROCEDURE GetCustomerCompanyID
@CustomerID INT,
@Now DATETIME,
@CompanyID INT OUTPUT
AS
BEGIN
	SET @CompanyID = dbo.udfGetCustomerCompanyID(@CustomerID, @Now)
END
GO
