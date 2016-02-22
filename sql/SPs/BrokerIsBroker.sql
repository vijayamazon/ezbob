SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BrokerIsBroker') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerIsBroker SELECT 1')
GO

ALTER PROCEDURE BrokerIsBroker
@ContactEmail NVARCHAR(255),
@UiOriginID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT

	SELECT
		@BrokerID = BrokerID
	FROM
		Broker
	WHERE
		LOWER(ContactEmail) = LOWER(@ContactEmail)
		AND
		@UiOriginID IS NOT NULL
		AND
		OriginID = @UiOriginID

	SELECT ISNULL(@BrokerID, 0) AS BrokerID
END
GO
