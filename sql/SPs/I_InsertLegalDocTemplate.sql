IF OBJECT_ID('I_InsertLegalDocTemplate')IS NULL 
	EXECUTE('CREATE PROCEDURE I_InsertLegalDocTemplate AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE I_InsertLegalDocTemplate
@Template nvarchar(max),
@IsUpdate bit,
@OriginID int,
@IsRegulated bit,
@ProductID int,
@IsApproved bit,
@IsReviewed bit,
@ReleaseDate datetime,
@TemplateTypeID int
AS
BEGIN
INSERT INTO LoanAgreementTemplate 
(Template
,IsUpdate
,OriginID
,IsRegulated
,ProductID
,IsApproved
,IsReviewed
,ReleaseDate
,TemplateTypeID)
values(@Template,
	   @IsUpdate,
	   @OriginID,
	   @IsRegulated,
	   @ProductID,
	   @IsApproved ,
	   @IsReviewed,
	   @ReleaseDate,
	   @TemplateTypeID) 
END
GO