IF OBJECT_ID('EncryptMarketplaceSecurityData') IS NULL
	EXECUTE('CREATE PROCEDURE EncryptMarketplaceSecurityData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE EncryptMarketplaceSecurityData
@BatchID UNIQUEIDENTIFIER,
@MarketplaceID INT,
@BackupTime DATETIME,
@OldData VARBINARY(MAX),
@NewData VARBINARY(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO TMP_CustomerMarketplaces(BatchID, MarketplaceID, BackupTime, OldData, NewData)
		VALUES (@BatchID, @MarketplaceID, @BackupTime, @OldData, @NewData)
END
GO
