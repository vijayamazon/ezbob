IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCompanyCaisDefaultDataForAlerts]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCompanyCaisDefaultDataForAlerts]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCompanyCaisDefaultDataForAlerts]
	@CustomerId INT
AS
BEGIN
	DECLARE 
		@NumOfCurrentDefaults INT,
		@NumOfSettledDefaults INT,
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
			
	-- Num of current defaults
	SELECT 
		@NumOfCurrentDefaults = COUNT(1) 
	FROM 
		ExperianLtdDL97,
		ExperianLtd 
	WHERE 
		AccountState = 'D' AND 
		SettlementDate IS NULL AND 
		ExperianLtdDL97.ExperianLtdID = ExperianLtd.ExperianLtdID AND
		ServiceLogID = @ServiceLogID

	-- Num of settled defaults
	SELECT 
		@NumOfSettledDefaults = COUNT(1) 
	FROM 
		ExperianLtdDL97,
		ExperianLtd 
	WHERE 
		AccountState = 'D' AND 
		SettlementDate IS NOT NULL AND 
		ExperianLtdDL97.ExperianLtdID = ExperianLtd.ExperianLtdID AND
		ServiceLogID = @ServiceLogID

	SELECT
		@NumOfCurrentDefaults AS NumOfCurrentDefaults,
		@NumOfSettledDefaults AS NumOfSettledDefaults
END
GO
