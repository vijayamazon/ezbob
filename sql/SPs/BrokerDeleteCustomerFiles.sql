IF OBJECT_ID('BrokerDeleteCustomerFiles') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerDeleteCustomerFiles AS SELECT 1')
GO

ALTER PROCEDURE BrokerDeleteCustomerFiles
@RefNum NVARCHAR(8),
@ContactEmail NVARCHAR(255),
@FileIDs IntList READONLY,
@Origin INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT

	SELECT
		@BrokerID = b.BrokerID
	FROM
		Broker b
	WHERE
		b.ContactEmail = @ContactEmail
		AND
		b.OriginID = @Origin

	DELETE
		MP_AlertDocument
	FROM
		MP_AlertDocument d,
		Customer c,
		@FileIDs f
	WHERE
		d.UserId = @BrokerID
		AND
		d.CustomerId = c.Id
		AND
		c.RefNumber = @RefNum
		AND
		c.BrokerId = @BrokerID
		AND
		d.Id = f.Value
END
GO
