IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetStrategies]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_GetStrategies]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_GetStrategies]
 @pEntityType [nvarchar](100)
AS
BEGIN
  select strategy_strategy.strategyid,
	  strategy_strategy.name,
	  strategy_strategy.description,
	  strategy_strategy.isembeddingallowed,
	  strategy_strategy.userid,
	  strategy_strategy.authorid,
	  strategy_strategy.state,
	  strategy_strategy.substate,
	  strategy_strategy.creationdate,
	  strategy_strategy.executionduration,
	  strategy_strategy.lastupdatedate,
	  strategy_strategy.displayname,
	  strategy_strategy.strategytype,
	  strategy_strategy.termdate,
	  strategy_strategy.IsMigrationSupported,
	  (
	  select (select count(rel.strategyid) from strategy_embededrel rel
	  where rel.embstrategyid = strategy_strategy.strategyid) +
	  (select count(p.publicid) from  strategy_publicrel p
	   where  p.strategyid = strategy_strategy.strategyid)) as "embcount",
	  (select count(sc.id) from  Strategy_Schedule sc
	   where  sc.strategyid = strategy_strategy.strategyid) as "scheduledCount",
	  (select count(ts.id) from  TaskedStrategies ts
	   where  ts.strategyid = strategy_strategy.strategyid) as "taskedStrategiesCount",
	  (select username from security_user where userid=strategy_strategy.authorid) as "authorname",
	  (select username from security_user where userid=strategy_strategy.userid) as "username",
          case when strategy_strategy.IsMigrationSupported = 1 
                AND EntityLink.IsApproved = 1
            then EntityLink.SeriaId
            else null
          end as LinkedToStrategy 
	from strategy_strategy
      left outer join EntityLink on EntityLink.EntityId = strategy_strategy.strategyid
                  AND EntityLink.EntityType = @pEntityType AND (EntityLink.IsDeleted = 0 or EntityLink.IsDeleted IS null)
      left outer join strategy_strategy dest on EntityLink.SeriaId = dest.strategyid
	where strategy_strategy.isdeleted = 0;
END
GO
