SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetCustomerIncorporationDate') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerIncorporationDate AS SELECT 1')
GO

ALTER PROCEDURE GetCustomerIncorporationDate
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT IncorporationDate = dbo.udfGetCustomerIncorporationDate(@CustomerID, @Now)
END
GO
