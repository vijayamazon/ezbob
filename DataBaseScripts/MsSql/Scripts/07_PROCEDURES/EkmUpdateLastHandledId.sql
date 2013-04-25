IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EkmUpdateLastHandledId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[EkmUpdateLastHandledId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE EkmUpdateLastHandledId
	@LastHandledId VARCHAR(100)
AS
BEGIN

	UPDATE EkmConnectorConfigs SET CfgValue=@LastHandledId WHERE CfgKey='LastHandledId'

END
GO
