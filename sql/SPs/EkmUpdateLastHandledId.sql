IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EkmUpdateLastHandledId]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[EkmUpdateLastHandledId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EkmUpdateLastHandledId] 
	(@LastHandledId VARCHAR(100))
AS
BEGIN
	UPDATE EkmConnectorConfigs SET CfgValue=@LastHandledId WHERE CfgKey='LastHandledId'
END
GO
