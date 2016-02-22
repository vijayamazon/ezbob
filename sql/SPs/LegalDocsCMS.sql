IF OBJECT_ID('I_AddLegalDoc')IS NULL 
	EXECUTE('CREATE PROCEDURE I_AddLegalDoc AS SELECT 1')
GO

IF OBJECT_ID('I_ApproveLegalDoc')IS NULL 
	EXECUTE('CREATE PROCEDURE I_ApproveLegalDoc AS SELECT 1')
GO

IF OBJECT_ID('I_ReviewLegalDoc')IS NULL 
	EXECUTE('CREATE PROCEDURE I_ReviewLegalDoc AS SELECT 1')
GO

IF OBJECT_ID('I_GetLatestLegalDocs')IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetLatestLegalDocs AS SELECT 1')
GO

IF OBJECT_ID('I_GetLegalDocsPendingApproval')IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetLegalDocsPendingApproval AS SELECT 1')
GO


IF OBJECT_ID('I_DeleteLegalDocById')IS NULL 
	EXECUTE('CREATE PROCEDURE I_DeleteLegalDocById AS SELECT 1')
GO


IF OBJECT_ID('I_SaveLegalDoc')IS NULL 
	EXECUTE('CREATE PROCEDURE I_SaveLegalDoc AS SELECT 1')
GO


IF TYPE_ID('LoanAgreementTemplateType') IS NULL
	CREATE TYPE LoanAgreementTemplateType  AS TABLE
(
	Template nvarchar(max) NULL,
	IsUpdate bit NOT NULL,
	OriginID int NULL,
	IsRegulated bit NULL,
	ProductID int NULL,
	IsApproved bit NULL,
	IsReviewed bit NULL,
	ReleaseDate datetime NULL,
	TemplateTypeID int NULL
)
GO

ALTER PROCEDURE I_AddLegalDoc
@Tbl LoanAgreementTemplateType READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO LoanAgreementTemplate(
	Template,
	IsUpdate,
	OriginID,
	IsRegulated,
	ProductID,
	IsApproved,
	IsReviewed,
	ReleaseDate,
	TemplateTypeID
	)
SELECT
	Template,
	IsUpdate,
	OriginID,
	IsRegulated,
	ProductID,
	IsApproved,
	IsReviewed,
	ReleaseDate,
	TemplateTypeID
FROM @Tbl		

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO

ALTER PROCEDURE I_ApproveLegalDoc
@LoanAgreementTemplateId int
AS
BEGIN	
	UPDATE LoanAgreementTemplate
	SET IsApproved = 1 
	WHERE Id = @LoanAgreementTemplateId
END
GO

ALTER PROCEDURE I_ReviewLegalDoc
@LoanAgreementTemplateId int
AS
BEGIN	
	UPDATE LoanAgreementTemplate
	SET IsReviewed = 1 
	WHERE Id = @LoanAgreementTemplateId
END
GO

ALTER PROCEDURE I_GetLatestLegalDocs
AS
BEGIN
	;with selectedTemplates as((select  Max(lat.id) as tid , lat.TemplateTypeID  
							from LoanAgreementTemplate lat 
							where IsApproved = 1 and IsReviewed = 1
							group by lat.TemplateTypeID, lat.OriginID)
							union
							(select  lat.id as tid, lat.TemplateTypeID  
							from LoanAgreementTemplate lat 
							where IsApproved = 0 and IsReviewed = 1						
							)
							union
							(select  lat.id as tid, lat.TemplateTypeID  
							from LoanAgreementTemplate lat 
							where IsApproved = 0 and IsReviewed = 0
							))
			select 
			 lat.Id
			,lat.Template
			,lat.IsUpdate
			,lat.TemplateTypeID
			,lat.OriginID
			,lat.IsRegulated
			,lat.ProductID
			,lat.IsApproved
			,lat.IsReviewed
			,lat.ReleaseDate
			 from  LoanAgreementTemplate lat
	join selectedTemplates st
	on st.tid = lat.Id
END
GO


ALTER PROCEDURE I_GetLegalDocsPendingApproval
AS
BEGIN
	;with maxTemplates as (select  Max(lat.id) as  maxVersion, lat.TemplateTypeID  
						from LoanAgreementTemplate lat 
						where IsReviewed = 1
						group by lat.TemplateTypeID)
			select 
			 lat.Id
			,lat.Template
			,lat.IsUpdate
			,lat.TemplateTypeID
			,lat.OriginID
			,lat.IsRegulated
			,lat.ProductID
			,lat.IsApproved
			,lat.IsReviewed
			,lat.ReleaseDate
			 from  LoanAgreementTemplate lat
	join maxTemplates mt
	on mt.maxVersion = lat.Id
END
GO

AlTER PROCEDURE I_GetLegalDocById
@LoanAgreementTemplateId INT
AS
BEGIN
		SELECT  
			 lat.Id
			,lat.Template
			,lat.IsUpdate
			,lat.TemplateTypeID
			,lat.OriginID
			,lat.IsRegulated
			,lat.ProductID
			,lat.IsApproved
			,lat.IsReviewed
			,lat.ReleaseDate
		FROM 
			LoanAgreementTemplate lat
		WHERE 
			lat.id = @LoanAgreementTemplateId
END
GO

ALTER PROCEDURE I_DeleteLegalDocById
@LoanAgreementTemplateId INT
AS
BEGIN
	IF EXISTS (SELECT * from LoanAgreementTemplate WHERE IsApproved = 1 AND IsReviewed = 1 AND @LoanAgreementTemplateId = Id)
	BEGIN
		RETURN (SELECT 'Can not delete production template');
	END;
	DELETE FROM LoanAgreementTemplate
	WHERE Id = @LoanAgreementTemplateId
END
GO

ALTER PROCEDURE I_SaveLegalDoc
@LoanAgreementTemplateId INT,
@Template VARCHAR(max)
AS
BEGIN
	UPDATE
		LoanAgreementTemplate 
	SET 
		Template = @Template
	WHERE 
		Id = @LoanAgreementTemplateId
END
GO
