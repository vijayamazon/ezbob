IF OBJECT_ID('BrokerDownloadCustomerFile') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerDownloadCustomerFile AS SELECT 1')
GO

ALTER PROCEDURE BrokerDownloadCustomerFile
@RefNum NVARCHAR(8),
@ContactEmail NVARCHAR(255),
@FileID INT,
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

	SELECT
		d.DocName AS FileName,
		d.BinaryBody AS FileContents
	FROM
		MP_AlertDocument d
		INNER JOIN Customer c
			ON d.CustomerId = c.Id
			AND c.RefNumber = @RefNum
			AND c.BrokerID = @BrokerID
	WHERE
		d.UserId = @BrokerID
		AND
		d.Id = @FileID
END
GO
