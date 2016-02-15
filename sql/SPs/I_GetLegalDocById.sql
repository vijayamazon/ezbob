IF OBJECT_ID('I_GetLegalDocById')IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetLegalDocById AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE I_GetLegalDocById
@TemplateTypeID INT
AS
BEGIN
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
where lat.TemplateTypeID = @TemplateTypeID
END
GO