SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('FindBrokerID') IS NULL
	EXECUTE('CREATE PROCEDURE FindBrokerID AS SELECT 1')
GO

ALTER PROCEDURE FindBrokerID
@ContactEmail NVARCHAR(255),
@Origin INT,
@BrokerID INT OUTPUT
AS
BEGIN
	SET @BrokerID = NULL

	SELECT
		@BrokerID = BrokerID
	FROM
		Broker
	WHERE
		ContactEmail = @ContactEmail
		AND
		OriginID = @Origin
END
GO
