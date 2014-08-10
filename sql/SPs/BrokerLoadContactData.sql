IF OBJECT_ID('BrokerLoadContactData') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadContactData AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadContactData
@BrokerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		BrokerID,
		FirmName,
		ContactName,
		ContactEmail,
		UserID
	FROM
		Broker
	WHERE
		BrokerID = @BrokerID
END
GO
