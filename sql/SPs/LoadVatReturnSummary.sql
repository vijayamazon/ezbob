IF OBJECT_ID('LoadVatReturnSummary') IS NULL
	EXECUTE('CREATE PROCEDURE LoadVatReturnSummary AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadVatReturnSummary
@MarketplaceID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		s.SummaryID
	INTO
		#s
	FROM
		MP_VatReturnSummary s
	WHERE
		s.CustomerMarketplaceID = @MarketplaceID
		AND
		s.IsActive = 1
	
	SELECT
		s.BusinessID,
		s.SummaryID,
		s.CustomerID,
		b.RegistrationNo,
		b.Name AS BusinessName,
		b.Address AS BusinessAddress,
		s.CreationDate,
		s.SalariesMultiplier,
		s.CustomerMarketplaceID,
		s.Currency AS CurrencyCode,
		s.PctOfAnnualRevenues,
		s.Revenues,
		s.Opex,
		s.TotalValueAdded,
		s.PctOfRevenues,
		s.Salaries,
		s.Tax,
		s.Ebida,
		s.PctOfAnnual,
		s.ActualLoanRepayment,
		s.FreeCashFlow,
		s.AnnualizedTurnover,
		s.AnnualizedValueAdded,
		s.AnnualizedFreeCashFlow
	FROM
		MP_VatReturnSummary s
		INNER JOIN Business b ON s.BusinessID = b.Id
		INNER JOIN #s ON s.SummaryID = #s.SummaryID
	
	SELECT
		p.SummaryID,
		p.SummaryPeriodID,
		p.DateFrom,
		p.DateTo,
		p.PctOfAnnualRevenues,
		p.Revenues,
		p.Opex,
		p.TotalValueAdded,
		p.PctOfRevenues,
		p.Salaries,
		p.Tax,
		p.Ebida,
		p.PctOfAnnual,
		p.ActualLoanRepayment,
		p.FreeCashFlow
	FROM
		MP_VatReturnSummaryPeriods p
		INNER JOIN #s ON p.SummaryID = #s.SummaryID
	ORDER BY
		p.DateFrom

	DROP TABLE #s
END
GO
