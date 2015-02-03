IF OBJECT_ID('RecordMail') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE RecordMail AS SELECT 1')
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[RecordMail] 
	(
	 @Filename NVARCHAR(3000),
	 @Body VARBINARY(MAX),
	 @Creation DATETIME,
	 @CustomerMail NVARCHAR(200),
	 @UserID INT
	)
	
AS
BEGIN
	INSERT INTO Export_Results
		(FileName, BinaryBody, FileType, CreationDate, SourceTemplateId, ApplicationId, Status, StatusMode, NodeName, SignedDocumentId)
	VALUES
		(@Filename, @Body, 0, @Creation, NULL, -1, NULL, NULL, 'No nodes - New flow', NULL)
	
	INSERT INTO EzbobMailNodeAttachRelation (ExportId, ToField, UserID) VALUES (@@Identity, @CustomerMail, @UserID)
END

GO
