IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCompanyCaisAccountsDataForAlerts]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCompanyCaisAccountsDataForAlerts]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCompanyCaisAccountsDataForAlerts]
	@CustomerId INT
AS
BEGIN
	DECLARE
		@ServiceLogID BIGINT
		
	SELECT TOP 1
		@ServiceLogID = MP_ServiceLog.Id
	FROM
		MP_ServiceLog,
		Company,
		Customer
	WHERE
		Customer.Id = @CustomerID AND
		Customer.CompanyId = Company.Id AND		
		CompanyRefNum = Company.ExperianRefNum
		AND
		ServiceType = 'E-SeriesLimitedData'
	ORDER BY
		InsertDate DESC,
		MP_ServiceLog.Id DESC

	SELECT
		CAISLastUpdatedDate, 
		AccountStatusLast12AccountStatuses 
	FROM 
		ExperianLtdDL97,
		ExperianLtd
	WHERE
		ExperianLtdDL97.ExperianLtdID = ExperianLtd.ExperianLtdID AND
		ServiceLogID = @ServiceLogID AND
		ExperianLtdDL97.AccountState = 'A'
END
GO
