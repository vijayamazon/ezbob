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
		@BrokerID = c.BrokerID
	FROM
		Customer c
	WHERE
		c.Id = @CustomerID
	
	------------------------------------------------------------------------------

	IF @BrokerID IS NULL
		SELECT '' AS ContactEmail
	ELSE
		SELECT
			b.ContactEmail
		FROM
			Broker b
		WHERE
			b.BrokerID = @BrokerID
END
GO
