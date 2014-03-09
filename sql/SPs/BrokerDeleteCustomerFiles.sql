IF TYPE_ID('IntList') IS NULL
	CREATE TYPE IntList AS TABLE (Value INT NULL)
GO

IF OBJECT_ID('BrokerDeleteCustomerFiles') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerDeleteCustomerFiles AS SELECT 1')
GO

ALTER PROCEDURE BrokerDeleteCustomerFiles
@CustomerID INT,
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
		c.Id = @CustomerId
		AND
		c.BrokerId = @BrokerID
		AND
		d.Id = f.Value
END
GO
