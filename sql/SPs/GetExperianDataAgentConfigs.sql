IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianDataAgentConfigs]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExperianDataAgentConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExperianDataAgentConfigs]
AS
BEGIN
	SELECT 
		CfgKey, 
		CfgValue 
	FROM 
		ExperianDataAgentConfigs
END
GO
