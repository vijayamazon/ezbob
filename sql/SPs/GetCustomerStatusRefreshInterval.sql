IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerStatusRefreshInterval]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomerStatusRefreshInterval]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomerStatusRefreshInterval]
AS
BEGIN
	SELECT 
		CONVERT(INT, Value) AS RefreshInterval
	FROM 
		ConfigurationVariables 
	WHERE 
		Name = 'CustomerStateRefreshInterval'
END
GO
