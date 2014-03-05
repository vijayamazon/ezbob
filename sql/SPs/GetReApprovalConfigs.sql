IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetReApprovalConfigs]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetReApprovalConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetReApprovalConfigs]
AS
BEGIN
	SELECT
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'AutoReApproveMaxNumOfOutstandingLoans') AS AutoReApproveMaxNumOfOutstandingLoans
END
GO
