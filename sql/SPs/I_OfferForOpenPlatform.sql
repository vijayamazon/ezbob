
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_OfferForOpenPlatform') IS NULL
	EXECUTE('CREATE PROCEDURE I_OfferForOpenPlatform AS SELECT 1')
GO

ALTER PROCEDURE I_OfferForOpenPlatform
@CashRequestID BIGINT
AS
BEGIN
	DECLARE @IsForOpenPlatform BIT = 0

	SELECT
		@IsForOpenPlatform = CAST((CASE WHEN pst.FundingTypeID IS NULL THEN 0 ELSE 1 END) AS BIT)
	FROM 
		CashRequests cr 
		INNER JOIN I_ProductSubType pst ON pst.ProductSubTypeID = cr.ProductSubTypeID
	WHERE
		cr.Id = @CashRequestID

	SELECT ISNULL(@IsForOpenPlatform, 0) AS IsForOpenPlatform
END
GO
