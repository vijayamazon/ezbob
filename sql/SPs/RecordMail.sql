IF OBJECT_ID('RecordMail') IS NULL
	EXECUTE('CREATE PROCEDURE RecordMail AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RecordMail
@Filename NVARCHAR(3000),
@Body VARBINARY(MAX),
@Creation DATETIME,
@CustomerMail NVARCHAR(200),
@UserID INT,
@TemplateName NVARCHAR(500)
AS
BEGIN
	BEGIN TRANSACTION

	INSERT INTO Export_Results(
		FileName, BinaryBody, FileType,
		CreationDate, SourceTemplateId, ApplicationId,
		Status, StatusMode, NodeName, SignedDocumentId
	) VALUES (
		@Filename, @Body, 0,
		@Creation, NULL, -1,
		NULL, NULL, @TemplateName, NULL
	)

	DECLARE @erID INT = SCOPE_IDENTITY()

	INSERT INTO EzbobMailNodeAttachRelation (ExportId, ToField, UserID)
		VALUES (@erID, @CustomerMail, @UserID)

	COMMIT TRANSACTION
END
GO
