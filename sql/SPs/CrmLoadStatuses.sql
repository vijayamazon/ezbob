IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CrmLoadStatuses]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[CrmLoadStatuses]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CrmLoadStatuses]
AS
BEGIN
	SELECT
		Id AS ID,
		Name
	FROM
		CRMActions
END
GO
