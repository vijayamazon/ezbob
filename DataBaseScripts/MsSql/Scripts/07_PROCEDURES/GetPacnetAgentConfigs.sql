IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPacnetAgentConfigs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPacnetAgentConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE GetPacnetAgentConfigs
AS
BEGIN	
	SELECT 
		CfgKey, 
		CfgValue 
	FROM 
		PacnetAgentConfigs	
END
GO
