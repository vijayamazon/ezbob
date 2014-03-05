IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetEkmConnectorConfigs]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetEkmConnectorConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetEkmConnectorConfigs]
AS
BEGIN
	SELECT 
		CfgKey, 
		CfgValue 
	FROM 
		EkmConnectorConfigs
END
GO
