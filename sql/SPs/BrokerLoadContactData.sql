IF OBJECT_ID('BrokerLoadContactData') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadContactData AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadContactData
@BrokerID INT
AS
BEGIN
	SELECT
		BrokerID,
		ContactName,
		ContactEmail
	FROM
		Broker
	WHERE
		BrokerID = @BrokerID
END
GO

