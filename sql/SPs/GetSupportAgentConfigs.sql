IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSupportAgentConfigs]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSupportAgentConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSupportAgentConfigs]
AS
BEGIN
	SELECT 
		CfgKey, 
		CfgValue 
	FROM 
		SupportAgentConfigs	
END
GO
