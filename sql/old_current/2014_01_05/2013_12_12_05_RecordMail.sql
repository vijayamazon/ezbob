IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.RecordMail') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.RecordMail
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.RecordMail 
 (
	@Filename NVARCHAR(3000),
	@Body VARBINARY(MAX),
	@Creation DATETIME,
	@CustomerMail NVARCHAR(200)
 )
AS
BEGIN
	INSERT INTO Export_Results
		(FileName, BinaryBody, FileType, CreationDate, SourceTemplateId, ApplicationId, Status, StatusMode, NodeName, SignedDocumentId)
	VALUES
		(@Filename, @Body, 0, @Creation, NULL, -1, NULL, NULL, 'No nodes - New flow', NULL)
	
	INSERT INTO EzbobMailNodeAttachRelation (ExportId, ToField) VALUES (@@Identity, @CustomerMail)
END
GO
