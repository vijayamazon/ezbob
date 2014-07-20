IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIsCompanyDataUpdated]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetIsCompanyDataUpdated]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetIsCompanyDataUpdated] 
	(@CustomerId INT, @Today DATE)
AS
BEGIN
	DECLARE @LastUpdateTime DATE
	
	SELECT @LastUpdateTime = AnalyticsDate FROM CustomerAnalyticsCompany WHERE CustomerId = @CustomerId AND IsActive = 1
	IF @Today = @LastUpdateTime
		SELECT CAST (1 AS BIT) AS IsUpdated
	ELSE
		SELECT CAST (0 AS BIT) AS IsUpdated
END
GO
