IF OBJECT_ID('BrokerLoadCustomerFiles') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadCustomerFiles AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadCustomerFiles
@RefNum NVARCHAR(8),
@ContactEmail NVARCHAR(255),
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
		d.Id AS FileID,
		d.DocName AS FileName,
		d.UploadDate,
		d.Description AS FileDescription
	FROM
		MP_AlertDocument d
		INNER JOIN Customer c
			ON d.CustomerId = c.Id
			AND c.RefNumber = @RefNum
			AND c.BrokerID = @BrokerID
	WHERE
		d.UserId = @BrokerID
	ORDER BY
		d.Description
END
GO
