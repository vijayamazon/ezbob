IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateExperianFirstIdToHandle]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateExperianFirstIdToHandle]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE UpdateExperianFirstIdToHandle
	@FirstIdToHandle VARCHAR(100)
AS
BEGIN
	UPDATE ExperianDataAgentConfigs SET CfgValue = @FirstIdToHandle WHERE CfgKey = 'FirstIdToHandle'
END
GO
