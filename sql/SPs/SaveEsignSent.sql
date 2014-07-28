IF OBJECT_ID('SaveEsignSent') IS NULL
	EXECUTE('CREATE PROCEDURE SaveEsignSent AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SaveEsignSent
@CustomerID INT,
@TemplateID INT,
@DocumentKey NVARCHAR(255),
@SentToCustomer BIT,
@Now DATETIME,
@Directors IntList READONLY,
@ExperianDirectors IntList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @SignatureID BIGINT

	BEGIN TRANSACTION

	INSERT INTO Esignatures (CustomerID, EsignTemplateID, DocumentKey, SendDate, StatusID)
		VALUES (@CustomerID, @TemplateID, @DocumentKey, @Now, 0)

	SET @SignatureID = SCOPE_IDENTITY()

	IF @SentToCustomer = 1
		INSERT INTO Esigners(EsignatureID, DirectorID, StatusID)
			VALUES (@SignatureID, NULL, 0)

	INSERT INTO Esigners(EsignatureID, DirectorID, StatusID)
	SELECT
		@SignatureID,
		Value,
		0
	FROM
		@Directors

	INSERT INTO Esigners(EsignatureID, ExperianDirectorID, StatusID)
	SELECT
		@SignatureID,
		Value,
		0
	FROM
		@ExperianDirectors

	COMMIT TRANSACTION
END
GO
