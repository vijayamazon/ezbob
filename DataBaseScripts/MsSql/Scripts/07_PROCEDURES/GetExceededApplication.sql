IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExceededApplication]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExceededApplication]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExceededApplication]
AS
BEGIN
select
   aa.applicationid
   ,(select ss.displayname from strategy_strategy ss where ss.strategyid = aa.strategyid) as strategy_name
   ,(select top 1 cpp.name from creditproduct_products cpp, creditproduct_strategyrel cps where cpp.id = cps.creditproductid and cps.strategyid = aa.strategyid) as creditproduct_name
   ,aa.appcounter
  from
   application_application aa
   ,strategy_node sn
  where
   (sn.executionduration is not null or sn.executionduration != 0)
   and aa.parentappid is null
  group by
    aa.applicationid
   ,aa.strategyid
   ,aa.appcounter
END;
GO
