
ALTER PROCEDURE AV_GetYodleeRevenues
@CustomerMarketplaceId INT
AS
BEGIN

DECLARE @YodleeRevenues DECIMAL(18,4) = 0

DECLARE @IsParsedBank BIT = 0

DECLARE @MinPostDate DATETIME
DECLARE @MinTransDate DATETIME

DECLARE @MinDate DATETIME
DECLARE @MaxDate DATETIME

SELECT @IsParsedBank = CASE WHEN mp.DisplayName='ParsedBank' THEN 1 ELSE 0 END FROM MP_CustomerMarketPlace mp WHERE mp.Id=@CustomerMarketplaceId

IF @IsParsedBank = 0 
BEGIN
	;WITH trns AS (
	SELECT DISTINCT tr.srcElementId srcElementId, tr.Id  FROM MP_CustomerMarketPlace mp 
	INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
	INNER JOIN MP_YodleeOrderItem i ON i.OrderId = o.Id
	INNER JOIN MP_YodleeOrderItemBankTransaction tr ON tr.OrderItemId = i.Id
	INNER JOIN MP_YodleeGroup g ON g.Id = tr.EzbobCategory
	WHERE mp.Id=@CustomerMarketplaceId
	AND tr.transactionBaseType = 'credit'
	AND (tr.isSeidMod=0 OR tr.isSeidMod IS NULL)
	AND g.MainGroup = 'Revenues'
	)
	SELECT @YodleeRevenues = isnull(sum(tr.transactionAmount),0)
	FROM MP_YodleeOrderItemBankTransaction tr INNER JOIN trns ON tr.Id=trns.Id
		
	SELECT @MaxDate = max(i.asOfDate), @MinTransDate = min(tr.transactionDate), @MinPostDate = min(tr.postDate) FROM MP_CustomerMarketPlace mp 
	INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
	INNER JOIN MP_YodleeOrderItem i ON i.OrderId = o.Id
	LEFT JOIN MP_YodleeOrderItemBankTransaction tr ON tr.OrderItemId = i.Id
	WHERE mp.Id=@CustomerMarketplaceId
		
	SELECT @MinDate = CASE 
		WHEN @MinPostDate IS NOT NULL AND @MinTransDate IS NULL THEN @MinPostDate
		WHEN @MinPostDate IS NULL AND @MinTransDate IS NOT NULL THEN @MinTransDate
		WHEN @MinPostDate IS NULL AND @MinTransDate IS NULL THEN NULL
		WHEN @MinPostDate < @MinTransDate THEN @MinPostDate
		ELSE @MinTransDate END
	
END
ELSE
BEGIN
	;WITH lastOrder AS (
	SELECT max(o.Id) Id FROM MP_CustomerMarketPlace mp 
	INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
	WHERE mp.Id=@CustomerMarketplaceId AND mp.Disabled=0
	)
	SELECT @YodleeRevenues = isnull(sum(tr.transactionAmount),0)
	FROM MP_YodleeOrderItemBankTransaction tr 
	INNER JOIN MP_YodleeOrderItem i ON i.Id = tr.OrderItemId 
	INNER JOIN MP_YodleeOrder o ON o.Id = i.OrderId INNER JOIN lastOrder ON o.Id=lastOrder.Id
	INNER JOIN MP_YodleeGroup g ON g.Id = tr.EzbobCategory
	WHERE tr.transactionBaseType = 'credit'
	AND (tr.isSeidMod=0 OR tr.isSeidMod IS NULL)
	AND g.MainGroup = 'Revenues'
	
	;WITH lastOrder AS (
	SELECT max(o.Id) Id FROM MP_CustomerMarketPlace mp 
	INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
	WHERE mp.Id=@CustomerMarketplaceId AND mp.Disabled=0
	)
	SELECT @MinDate = min(tr.transactionDate), @MaxDate = max(i.asOfDate)
	FROM MP_YodleeOrderItemBankTransaction tr 
	INNER JOIN MP_YodleeOrderItem i ON i.Id = tr.OrderItemId 
	INNER JOIN MP_YodleeOrder o ON o.Id = i.OrderId INNER JOIN lastOrder ON o.Id=lastOrder.Id
	
END

SELECT @YodleeRevenues AS YodleeRevenues, @MinDate AS MinDate, @MaxDate AS MaxDate
END
GO