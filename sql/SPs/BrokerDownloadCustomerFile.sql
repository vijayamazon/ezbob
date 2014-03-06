IF OBJECT_ID('BrokerDownloadCustomerFile') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerDownloadCustomerFile AS SELECT 1')
GO

ALTER PROCEDURE BrokerDownloadCustomerFile
@CustomerID INT,
@ContactEmail NVARCHAR(255),
@FileID INT
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

	SELECT
		d.DocName AS FileName,
		d.BinaryBody AS FileContents
	FROM
		MP_AlertDocument d
		INNER JOIN Customer c
			ON d.CustomerId = c.Id
			AND c.Id = @CustomerID
			AND c.BrokerID = @BrokerID
	WHERE
		d.UserId = @UserID
		AND
		d.Id = @FileID
END
GO
