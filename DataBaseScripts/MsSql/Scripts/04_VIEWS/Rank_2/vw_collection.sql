IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_collection]'))
DROP VIEW [dbo].[vw_collection]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_collection]
AS
select distinct (CustomerID), ISNULL(MAX (MaxDelinquencyDays), 0) as MaxDelinquencyDays,
ISNULL(MAX (DateClose),0) as DateClosed
from vw_NotClose
group by CustomerID
GO
