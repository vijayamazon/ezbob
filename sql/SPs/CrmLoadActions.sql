IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CrmLoadActions]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[CrmLoadActions]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CrmLoadActions]
AS
BEGIN
	SELECT
		Id AS ID,
		Name
	FROM
		CRMActions
	ORDER BY
		Id ASC
END
GO
