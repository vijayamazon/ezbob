IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExposurePerMedalReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExposurePerMedalReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExposurePerMedalReport]
	@dateStart DateTime, 
	@dateEnd DateTime
AS
BEGIN

	SELECT dbo.Medals.Medal, ISNULL(pr.Processed, 0) as Processed, ISNULL(pr.ProcessedAmount, 0) as ProcessedAmount, 
			ISNULL(ag.Approved, 0) as Approved, ISNULL(ag.ApprovedAmount,0) as ApprovedAmount, 
			ISNULL(pr.Paid, 0) as Paid, ISNULL(pr.PaidAmount,0) as PaidAmount, 
			ISNULL(pr.Defaults, 0) as Defaults, ISNULL(pr.DefaultsAmount,0) as DefaultsAmount, 
			ISNULL(pr.Late30, 0) as Late30, ISNULL(pr.Late30Amount,0) as Late30Amount, 
			ISNULL(pr.Late60, 0) as Late60, ISNULL(pr.Late60Amount,0) as Late60Amount, 
			ISNULL(pr.Late90, 0) as Late90, ISNULL(pr.Late90Amount,0) as Late90Amount,
			ISNULL(pr.Exposure, 0) as Exposure, ISNULL(ol.OpenCreditLine, 0) as OpenCreditLine
			FROM 
			dbo.Medals LEFT OUTER JOIN  
		(SELECT UPPER(dbo.CashRequests.MedalType) as Medal, Count(dbo.CashRequests.SystemCalculatedSum) as Processed, Sum(dbo.CashRequests.SystemCalculatedSum) as ProcessedAmount,
		SUM(Paid) as Paid, SUM(PaidAmount) as PaidAmount, 
		SUM(Defaults) as Defaults, SUM(DefaultsAmount) as DefaultsAmount,
		SUM(Late30) as Late30, SUM(Late30Amount) as Late30Amount, 
		SUM(Late60) as Late60, SUM(Late60Amount) as Late60Amount, 
		SUM(Late90) as Late90, SUM(Late90Amount) as Late90Amount,
		SUM(Exposure) as Exposure
		FROM dbo.CashRequests LEFT OUTER JOIN GetLoanByRequestCashId() as lp ON dbo.CashRequests.Id = lp.RequestCashId
		Where dbo.CashRequests.SystemDecisionDate >= @dateStart
				AND dbo.CashRequests.UnderwriterDecisionDate <= @dateEnd
		GROUP BY UPPER(dbo.CashRequests.MedalType)) AS pr 
		ON UPPER (dbo.Medals.Medal) = UPPER(pr.Medal)
		LEFT OUTER JOIN GetApprovedGroupedByMedal(@dateStart, @dateEnd) AS ag ON pr.Medal = ag.Medal
		LEFT OUTER JOIN GetOpenCreditLineByMedal() AS ol ON UPPER(pr.Medal) = UPPER(ol.Medal)
END
GO
