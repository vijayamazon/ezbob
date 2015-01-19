IF object_id('FraudGetDetectionsParsedBank') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE FraudGetDetectionsParsedBank AS SELECT 1')
END
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE FraudGetDetectionsParsedBank
@CustomerID INT 
AS 
BEGIN

WITH t1 AS 
	(
		SELECT t.Id, t.transactionAmount, t.description, t.transactionDate, t.postDate, m.CustomerId
		FROM MP_YodleeOrderItemBankTransaction t
		INNER JOIN MP_YodleeOrderItem i ON t.OrderItemId=i.Id
		INNER JOIN MP_YodleeOrder o ON o.Id = i.OrderId
		INNER JOIN MP_CustomerMarketPlace m ON m.Id = o.CustomerMarketPlaceId 
		INNER JOIN Customer c ON c.Id = m.CustomerId
		WHERE c.Id = @CustomerID
		AND t.description NOT LIKE '%fee%' 
		AND t.description <> 'RETURNED D/D' 
		AND t.transactionAmount > 10
		AND m.DisplayName='ParsedBank'
	), t2 AS 
	(
		SELECT t.transactionAmount, t.description, t.transactionDate, t.postDate,t.Id, m.CustomerId
		FROM MP_YodleeOrderItemBankTransaction t
		INNER JOIN MP_YodleeOrderItem i ON t.OrderItemId=i.Id
		INNER JOIN MP_YodleeOrder o ON o.Id = i.OrderId
		INNER JOIN MP_CustomerMarketPlace m ON m.Id = o.CustomerMarketPlaceId 
		INNER JOIN Customer c ON c.Id = m.CustomerId 
		WHERE c.Id<>@CustomerID
		AND t.description NOT LIKE '%fee%' 
		AND t.description <> 'RETURNED D/D' 
		AND t.transactionAmount > 10
		AND m.DisplayName='ParsedBank'
	)
	SELECT t2.CustomerId CustomerId, count(*) MatchedTransactions
	FROM t1 INNER JOIN t2 ON t1.transactionAmount = t2.transactionAmount AND t1.description = t2.description AND
	(
	 (t1.transactionDate IS NOT NULL AND t2.transactionDate IS NOT NULL AND t1.transactionDate = t2.transactionDate) OR 
	 (t1.postDate IS NOT NULL AND t2.postDate IS NOT NULL AND t1.postDate = t2.postDate)
	) 
	WHERE t1.CustomerId <> t2.CustomerId
	GROUP BY t1.CustomerId, t2.CustomerId
	HAVING count(*) > 10
	
END
GO