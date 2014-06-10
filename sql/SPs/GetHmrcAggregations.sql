
IF OBJECT_ID('GetHmrcAggregations') IS NULL
	EXECUTE('CREATE PROCEDURE GetHmrcAggregations AS SELECT 1')
GO


ALTER PROCEDURE [dbo].[GetHmrcAggregations] 
	(@CustomerId INT)
AS
BEGIN
	
	DECLARE @AnnualRevenues DECIMAL = (
		SELECT ISNULL(MAX(y.Revenues/y.totalDays * 365), -1) AS AnnualRevenues
		FROM
		(
			SELECT SUM(x.days) totalDays, x.mpId, x.Revenues
			FROM
			(
				SELECT ISNULL(DATEDIFF(DAY, p.DateFrom, p.DateTo),0) days, s.Revenues, s.CustomerMarketplaceID mpId 
				FROM MP_VatReturnSummary s INNER JOIN MP_VatReturnSummaryPeriods p ON p.SummaryID = s.SummaryID AND s.IsActive=1
				WHERE s.CustomerID=@CustomerId
			) x
		GROUP BY x.mpId, x.Revenues
		) y
	)
	
	DECLARE @QuarterRevenues DECIMAL = 
	(
		SELECT ISNULL(MAX(pp.Revenues / ISNULL(DATEDIFF(DAY, pp.DateFrom, pp.DateTo),0) * 90), -1) AS QuarterRevenues
		FROM MP_VatReturnSummaryPeriods pp INNER JOIN 
		(
			SELECT max(p.DateTo) lastPeriod, p.SummaryID FROM MP_VatReturnSummary s INNER JOIN MP_VatReturnSummaryPeriods p ON p.SummaryID = s.SummaryID AND s.IsActive=1
			WHERE s.CustomerID=@CustomerId
			GROUP BY p.SummaryID
		) x 
		ON pp.SummaryID = x.SummaryID AND pp.DateTo = x.lastPeriod
	)
   	
	SELECT @AnnualRevenues AS AnnualRevenues, @QuarterRevenues QuarterRevenues
	
END

GO
