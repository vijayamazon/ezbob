IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateExperianFirstIdToHandle]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateExperianFirstIdToHandle]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateExperianFirstIdToHandle] 
	(@FirstIdToHandle VARCHAR(100))
AS
BEGIN
	UPDATE ExperianDataAgentConfigs SET CfgValue = @FirstIdToHandle WHERE CfgKey = 'FirstIdToHandle'
END
GO
