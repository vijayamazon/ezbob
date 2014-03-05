IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UwGridLoans]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UwGridLoans]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UwGridLoans] 
	(@WithTest BIT)
AS
BEGIN
	SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS ApplyDate,
	c.GreetingMailSentDate AS RegDate,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	w.WizardStepTypeDescription AS WizardStep,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	c.FirstLoanDate,
	c.LastLoanDate,
	c.LastLoanAmount,
	c.AmountTaken,
	c.TotalPrincipalRepaid,	
	(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance,
	(
		SELECT TOP 1 s.[Date]
		FROM [LoanSchedule] s LEFT JOIN [loan] l ON l.[Id] = s.[LoanId]
		WHERE l.[CustomerId] = c.Id AND s.[Status] IN ('StillToPay','Late')
		ORDER BY s.[Date]
	) AS NextRepaymentDate,
	c.Status
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR c.IsTest = 0
	)
	AND
	c.CreditResult IS NOT NULL
ORDER BY
	c.Id DESC,
	t.Id
END
GO
