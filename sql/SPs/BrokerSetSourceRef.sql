IF OBJECT_ID('BrokerSetSourceRef') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerSetSourceRef AS SELECT 1')
GO

ALTER PROCEDURE BrokerSetSourceRef
@BrokerID INT,
@SourceRef NVARCHAR(255)
AS
	SET NOCOUNT ON;

	UPDATE Broker SET
		SourceRef = @SourceRef
	WHERE
		BrokerID = @BrokerID
GO
