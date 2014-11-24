IF OBJECT_ID('AV_GetHmrcTurnoverForRejection') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetHmrcTurnoverForRejection AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE AV_GetHmrcTurnoverForRejection
@CustomerId INT
AS
BEGIN
	DECLARE @HmrcRevenueAnnualized DECIMAL(18,6)
	DECLARE @HmrcRevenueQuarter DECIMAL(18,6)
	
	SELECT @HmrcRevenueAnnualized=sum(AnnualizedTurnover) FROM MP_VatReturnSummary WHERE CustomerID=@CustomerId AND IsActive=1
	
	;WITH lastPeriod AS 
	(SELECT p.SummaryID, max(p.DateTo) DateTo FROM MP_VatReturnSummaryPeriods p INNER JOIN MP_VatReturnSummary s ON p.SummaryID = s.SummaryID
	WHERE s.IsActive=1 AND s.CustomerID=@CustomerId
	GROUP BY p.SummaryID)
	SELECT @HmrcRevenueQuarter = sum(sp.Revenues) FROM MP_VatReturnSummaryPeriods sp INNER JOIN lastPeriod ON sp.SummaryID=lastPeriod.SummaryID AND sp.DateTo=lastPeriod.DateTo
	
	SELECT @HmrcRevenueQuarter AS HmrcRevenueQuarter,@HmrcRevenueAnnualized AS HmrcRevenueAnnualized
END 
GO
