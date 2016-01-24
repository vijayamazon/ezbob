IF OBJECT_ID('I_GetLegalDocs')IS NULL 
	EXECUTE('CREATE PROCEDURE I_GetLegalDocs AS SELECT 1')
GO

ALTER PROCEDURE I_GetLegalDocs
@OriginID INT,
@IsRegulated BIT,
@ProductSubTypeID INT
AS
BEGIN

IF (@ProductSubTypeID = 0)
BEGIN
	DECLARE @ProductID int
	SET @ProductID = 3
	IF (@OriginID = 1) OR (@OriginID = 2)  SET @ProductID = 1

	SET @ProductSubTypeID = (select top 1 pst.ProductSubTypeID from I_ProductSubType pst
								join I_ProductType pt
								on pt.ProductTypeID = pst.ProductTypeID
								join I_Product p
								on p.ProductID = pt.ProductID
								where p.ProductID = @ProductID)
END

;with maxTemplates as (select  Max(lat.id) as  maxVersion, lat.TemplateTypeID  
					from LoanAgreementTemplate lat 
					where IsApproved = 1 and IsReviewed = 1
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
join I_ProductType pt
on lat.ProductID = pt.ProductID
join I_ProductSubType pst
on pst.ProductTypeID = pt.ProductTypeID
where lat.OriginID = @OriginID AND
lat.IsRegulated = @IsRegulated AND
pst.ProductSubTypeID = @ProductSubTypeID
END
GO


