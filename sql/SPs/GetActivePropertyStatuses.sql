IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetActivePropertyStatuses]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetActivePropertyStatuses]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetActivePropertyStatuses]
AS
BEGIN
	SELECT 
		CustomerPropertyStatusGroups.Title,
		CustomerPropertyStatuses.Description,
		CustomerPropertyStatuses.IsOwner ,
		CustomerPropertyStatuses.Id 
	FROM 
		CustomerPropertyStatuses, 
		CustomerPropertyStatusGroups
	WHERE
		CustomerPropertyStatuses.GroupId = CustomerPropertyStatusGroups.Id AND
		CustomerPropertyStatuses.IsActive = 1
	ORDER BY 
		CustomerPropertyStatusGroups.Priority ASC,
		CustomerPropertyStatuses.Id ASC
END
GO
