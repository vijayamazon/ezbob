SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetYodleeRevenues') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetYodleeRevenues AS SELECT 1')
GO

ALTER PROCEDURE AV_GetYodleeRevenues
@CustomerMarketplaceId INT
AS
BEGIN

DECLARE @YodleeRevenues DECIMAL(18,4) = 0

;WITH trns AS (
SELECT DISTINCT tr.bankTransactionId bankTransactionId  FROM MP_CustomerMarketPlace mp 
INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
INNER JOIN MP_YodleeOrderItem i ON i.OrderId = o.Id
INNER JOIN MP_YodleeOrderItemBankTransaction tr ON tr.OrderItemId = i.Id
INNER JOIN MP_YodleeGroup g ON g.Id = tr.EzbobCategory
WHERE mp.Id=@CustomerMarketplaceId
AND mp.Disabled=0
AND tr.transactionBaseType = 'credit'
AND (tr.isSeidMod=0 OR tr.isSeidMod IS NULL)
AND g.MainGroup = 'Revenues'
)

SELECT @YodleeRevenues = isnull(sum(tr.transactionAmount),0) FROM MP_YodleeOrderItemBankTransaction tr INNER JOIN trns ON tr.bankTransactionId=trns.bankTransactionId

DECLARE @MinPostDate DATETIME
DECLARE @MinTransDate DATETIME

DECLARE @MinDate DATETIME
DECLARE @MaxDate DATETIME

SELECT @MaxDate = max(i.asOfDate), @MinTransDate = min(tr.transactionDate), @MinPostDate = min(tr.postDate) FROM MP_CustomerMarketPlace mp 
INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = mp.Id
INNER JOIN MP_YodleeOrderItem i ON i.OrderId = o.Id
INNER JOIN MP_YodleeOrderItemBankTransaction tr ON tr.OrderItemId = i.Id
INNER JOIN MP_YodleeGroup g ON g.Id = tr.EzbobCategory
WHERE mp.Id=@CustomerMarketplaceId
AND mp.Disabled=0

SELECT @MinDate = CASE 
	WHEN @MinPostDate IS NOT NULL AND @MinTransDate IS NULL THEN @MinPostDate
	WHEN @MinPostDate IS NULL AND @MinTransDate IS NOT NULL THEN @MinTransDate
	WHEN @MinPostDate IS NULL AND @MinTransDate IS NULL THEN NULL
	WHEN @MinPostDate < @MinTransDate THEN @MinPostDate
	ELSE @MinTransDate END

SELECT @YodleeRevenues AS YodleeRevenues, @MinDate AS MinDate, @MaxDate AS MaxDate
	
END
GO