IF OBJECT_ID('AV_GetYodleeRevenuesQuarter') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetYodleeRevenuesQuarter AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE AV_GetYodleeRevenuesQuarter
@CustomerMarketplaceId INT
AS
BEGIN

DECLARE @YodleeRevenues DECIMAL(18,4) = 0

DECLARE @IsParsedBank BIT = 0
DECLARE @MaxDate DATETIME

SELECT @IsParsedBank = CASE WHEN mp.DisplayName='ParsedBank' THEN 1 ELSE 0 END FROM MP_CustomerMarketPlace mp WHERE mp.Id=@CustomerMarketplaceId

IF @IsParsedBank = 0 
BEGIN
	SELECT @MaxDate = max(i.asOfDate)
	FROM MP_CustomerMarketPlace mp 
	INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
	INNER JOIN MP_YodleeOrderItem i ON i.OrderId = o.Id
	LEFT JOIN MP_YodleeOrderItemBankTransaction tr ON tr.OrderItemId = i.Id
	WHERE mp.Id=@CustomerMarketplaceId
	
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
	AND (dateadd(day, 90, tr.transactionDate) > @MaxDate OR dateadd(day, 90, tr.postDate) > @MaxDate)
	)
	SELECT @YodleeRevenues = isnull(sum(tr.transactionAmount),0)
	FROM MP_YodleeOrderItemBankTransaction tr INNER JOIN trns ON tr.Id=trns.Id
END
ELSE
BEGIN
	;WITH lastOrder AS (
	SELECT max(o.Id) Id FROM MP_CustomerMarketPlace mp 
	INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
	WHERE mp.Id=@CustomerMarketplaceId AND mp.Disabled=0
	)
	SELECT @MaxDate = max(i.asOfDate)
	FROM MP_YodleeOrderItemBankTransaction tr 
	INNER JOIN MP_YodleeOrderItem i ON i.Id = tr.OrderItemId 
	INNER JOIN MP_YodleeOrder o ON o.Id = i.OrderId INNER JOIN lastOrder ON o.Id=lastOrder.Id
	
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
	AND (dateadd(day, 90, tr.transactionDate) > @MaxDate OR dateadd(day, 90, tr.postDate) > @MaxDate)
	
	
	
END

SELECT @YodleeRevenues AS YodleeRevenues
END
GO

