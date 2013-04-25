IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNewCustomers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetNewCustomers]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE GetNewCustomers
AS
BEGIN
	SELECT 
		Customer.Id, 
		Customer.Name 
	FROM 
		Customer,
		SupportAgentConfigs
	WHERE
		SupportAgentConfigs.CfgKey = 'MaxCustomerId' AND
		CONVERT(INT, SupportAgentConfigs.CfgValue) < Customer.Id
	ORDER BY
		Customer.Id ASC
END
GO
