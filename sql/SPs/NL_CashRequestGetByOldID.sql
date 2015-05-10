
IF OBJECT_ID('NL_CashRequestGetByOldID') IS NULL
	EXECUTE('CREATE PROCEDURE NL_CashRequestGetByOldID AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE NL_CashRequestGetByOldID
@OldCashRequestID BIGINT
AS
BEGIN
	SELECT TOP 1 isnull(cr.CashRequestID, 0) AS CashRequestID FROM NL_CashRequests cr WHERE OldCashRequestID=@OldCashRequestID
END 
GO
	