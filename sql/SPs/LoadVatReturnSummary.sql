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

	DECLARE @SummaryID BIGINT
	
	SELECT
		@SummaryID = s.SummaryID
	FROM
		MP_VatReturnSummary s
	WHERE
		s.CustomerMarketplaceID = @MarketplaceID
		AND
		s.IsActive = 1
	
	SELECT
		s.SummaryID,
		s.CustomerID,
		s.BusinessID,
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
		s.FreeCashFlow
	FROM
		MP_VatReturnSummary s
		INNER JOIN Business b ON s.BusinessID = b.Id
	WHERE
		s.SummaryID = @SummaryID
	
	SELECT
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
	WHERE
		p.SummaryID = @SummaryID
	ORDER BY
		p.DateFrom
END
GO
