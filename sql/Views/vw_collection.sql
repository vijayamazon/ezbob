IF OBJECT_ID (N'dbo.vw_collection') IS NOT NULL
	DROP VIEW dbo.vw_collection
GO

CREATE VIEW [dbo].[vw_collection]
AS
select distinct (CustomerID), ISNULL(MAX (MaxDelinquencyDays), 0) as MaxDelinquencyDays,
ISNULL(MAX (DateClose),0) as DateClosed
from vw_NotClose
group by CustomerID

GO

