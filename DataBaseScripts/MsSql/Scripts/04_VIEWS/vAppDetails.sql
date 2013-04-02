IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vAppDetails]'))
DROP VIEW [dbo].[vAppDetails]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vAppDetails]
AS
SELECT ad.*, dn.Name
  FROM [dbo].[Application_Detail] ad
  inner join [dbo].[Application_DetailName] dn on dn.DetailNameId = ad.DetailNameId
GO
