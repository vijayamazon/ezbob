IF OBJECT_ID('AttachCustomerToBroker') IS NULL
	EXECUTE('CREATE PROCEDURE AttachCustomerToBroker AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE AttachCustomerToBroker
@CustomerID INT,
@ToBrokerID INT,
@UnderwriterID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	INSERT INTO CustomerBrokerHistory (CustomerID, FromBrokerID, ToBrokerID, EventTime, UnderwriterID)
	SELECT
		@CustomerID, BrokerID, @ToBrokerID, @Now, @UnderwriterID
	FROM
		Customer
	WHERE
		Id = @CustomerID

	------------------------------------------------------------------------------

	UPDATE Customer SET
		BrokerID = @ToBrokerID
	WHERE
		Id = @CustomerID

	------------------------------------------------------------------------------

	COMMIT TRANSACTION
END
GO
