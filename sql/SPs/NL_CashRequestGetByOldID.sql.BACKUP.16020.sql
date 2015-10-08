
IF OBJECT_ID('NL_CashRequestGetByOldID') IS NULL
	EXECUTE('CREATE PROCEDURE NL_CashRequestGetByOldID AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE NL_CashRequestGetByOldID
@OldCashRequestID BIGINT
AS
BEGIN
	declare @CashRequestID BIGINT;
	set  @CashRequestID = isnull((SELECT TOP 1 CashRequestID FROM NL_CashRequests WHERE OldCashRequestID= @OldCashRequestID), 0);
	select @CashRequestID;
END 	