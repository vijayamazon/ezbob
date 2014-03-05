IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptLoanDateScoreNDSPCII]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptLoanDateScoreNDSPCII]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptLoanDateScoreNDSPCII]
AS
BEGIN
	SELECT
		c.Id AS CustomerID,
		e.InsertDate,
		e.ServiceType,
		e.ResponseData
	FROM
		Customer c
		LEFT JOIN MP_ServiceLog e
			ON c.Id = e.CustomerId
			AND e.ServiceType IN ('Consumer Request', 'E-SeriesLimitedData', 'E-SeriesNonLimitedData')
			AND e.ResponseData IS NOT NULL
	WHERE
		c.IsTest = 0
	ORDER BY
		c.Id,
		e.InsertDate,
		e.ServiceType
END
GO
