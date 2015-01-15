DECLARE @TrailID BIGINT = 102590
--
DECLARE @OriginationTime NVARCHAR(16) = 'OriginationTime'
--
DECLARE @Now DATETIME
DECLARE @CustomerID INT
DECLARE @CashRequestID INT
--
SELECT
	@CustomerID = CustomerID,
	@CashRequestID = CashRequestID
FROM
	DecisionTrail
WHERE
	TrailID = @TrailID
--
SELECT
	@Now = UnderwriterDecisionDate
FROM
	CashRequests
WHERE
	Id = @CashRequestID
--
SELECT DISTINCT
	Now = @Now,
	CashRequestID = @CashRequestID,
	CustomerID = m.CustomerId,
	MpID = m.Id,
	MpType = t.Name,
	IsDisabled = m.Disabled,
	Created = m.Created,
	IsIncluded = CASE WHEN ISNULL(m.Disabled, 0) = 0 AND m.Created < @Now THEN 'yes' ELSE 'no' END
FROM
	MP_CustomerMarketPlace m
	INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	m.CustomerID = @CustomerID


SELECT * FROM MP_YodleeOrder WHERE CustomerMarketPlaceId = 16431

SELECT * FROM MP_YodleeOrderItem WHERE OrderId = 2003

