IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateLastFoundCustomerId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateLastFoundCustomerId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE UpdateLastFoundCustomerId
	@LastFoundCustomerId VARCHAR(100)
AS
BEGIN
	UPDATE SupportAgentConfigs SET CfgValue=@LastFoundCustomerId WHERE CfgKey = 'MaxCustomerId'
END
GO
