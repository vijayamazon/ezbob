IF OBJECT_ID (N'dbo.vAppDetails') IS NOT NULL
	DROP VIEW dbo.vAppDetails
GO

CREATE VIEW [dbo].[vAppDetails]
AS
SELECT ad.*, dn.Name
  FROM [dbo].[Application_Detail] ad
  inner join [dbo].[Application_DetailName] dn on dn.DetailNameId = ad.DetailNameId

GO

