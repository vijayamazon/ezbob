IF OBJECT_ID('BrokerDeleteCustomerFiles') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerDeleteCustomerFiles AS SELECT 1')
GO

ALTER PROCEDURE BrokerDeleteCustomerFiles
@RefNum NVARCHAR(8),
@ContactEmail NVARCHAR(255),
@FileIDs IntList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @UserID INT
	DECLARE @BrokerID INT

	SELECT
		@BrokerID = b.BrokerID,
		@UserID = b.UserID
	FROM
		Broker b
	WHERE
		b.ContactEmail = @ContactEmail

	DELETE
		MP_AlertDocument
	FROM
		MP_AlertDocument d,
		Customer c,
		@FileIDs f
	WHERE
		d.UserId = @UserID
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
