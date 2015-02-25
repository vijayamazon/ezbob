IF OBJECT_ID('GetCustomerIdByEmail') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerIdByEmail AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO



ALTER PROCEDURE [dbo].[GetCustomerIdByEmail]
@CustomerEmail NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Id  FROM Customer WHERE Name = LTRIM(RTRIM(@CustomerEmail))
	
END
