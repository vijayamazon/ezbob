IF OBJECT_ID('BrokerIsBroker') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerIsBroker AS SELECT 1')
GO

ALTER PROCEDURE BrokerIsBroker
@ContactEmail NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT

	SELECT
		@BrokerID = BrokerID
	FROM
		Broker
	WHERE
		ContactEmail = @ContactEmail

	SELECT ISNULL(@BrokerID, 0) AS BrokerID
END
GO
