IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMainStrategyStallerData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMainStrategyStallerData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMainStrategyStallerData] 
	(@CustomerId INT)
AS
BEGIN
	SELECT		
		ExperianRefNum,
		CAST(CASE WHEN LastStartedMainStrategyEndTime IS NULL THEN 0 ELSE 1 END AS BIT) AS MainStrategyExecutedBefore,
		Company.TypeOfBusiness,
		Name AS CustomerEmail
	FROM
		Customer,
		Company
	WHERE
		Customer.Id = @CustomerId AND
		Customer.CompanyId = Company.Id
END
GO
