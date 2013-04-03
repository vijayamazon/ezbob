IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[APPLICATION_VSDELINQ]'))
DROP VIEW [dbo].[APPLICATION_VSDELINQ]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[APPLICATION_VSDELINQ]
AS
select s.strategyid
     , a.applicationid
     , s.executionduration as executiontimelimit
     , a.creationdate as starttime
     , case
         when a.state = 2 or a.state = 3
         then a.lastupdatedate
         else null
       end as finishtime
     , case
         when a.state = 2 or a.state = 3
         then DATEDIFF(ss, a.lastupdatedate, a.creationdate)
         else DATEDIFF(ss, GETDATE(), a.creationdate)
       end as executiontime
     , case
         when a.state = 2 or a.state = 3
         then a.istimelimitexceeded
         else case
                when DATEADD( ss, ISNULL( s.executionduration, 1000000000), a.creationdate) < GETDATE() 
                then 1
                else 0
              end
       end as istimelimitexpiried
from application_application a
join strategy_strategy s on a.strategyid = s.strategyid
GO
