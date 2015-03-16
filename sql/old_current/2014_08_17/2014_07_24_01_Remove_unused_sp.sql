IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNonLimitedCompanyDashboardDetails]') AND TYPE IN (N'P', N'PC'))
	DROP PROCEDURE [dbo].[GetNonLimitedCompanyDashboardDetails]
GO
