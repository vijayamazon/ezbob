IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetApprovalConfigs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetApprovalConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetApprovalConfigs] 
AS
BEGIN
	SELECT
		(SELECT convert(BIT, Value) FROM ConfigurationVariables WHERE Name = 'AutoApproveIsSilent') AS AutoApproveIsSilent,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'AutoApproveSilentTemplateName') AS AutoApproveSilentTemplateName,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'AutoApproveSilentToAddress') AS AutoApproveSilentToAddress
END
GO
