IF OBJECT_ID('BrokerLoadAddressForCustomerMailCC') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadAddressForCustomerMailCC AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadAddressForCustomerMailCC
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @BrokerID INT

	------------------------------------------------------------------------------

	SELECT
		@BrokerID = bl.BrokerID
	FROM
		BrokerLeads bl
		INNER JOIN BrokerLeadAddModes m ON bl.BrokerLeadAddModeID = m.BrokerLeadAddModeID
	WHERE
		CustomerID = @CustomerID
		AND
		m.SendEmailOnCreate = 0
	
	------------------------------------------------------------------------------

	IF @BrokerID IS NULL
		SELECT '' AS ContactEmail
	ELSE
		SELECT
			ContactEmail
		FROM
			Broker
		WHERE
			BrokerID = @BrokerID
END
GO
