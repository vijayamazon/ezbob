CREATE OR REPLACE FUNCTION Strategy_GetStrategies
(
 pEntityType in VARCHAR2
)
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN
  OPEN l_Cursor FOR
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
               where  p.strategyid = strategy_strategy.strategyid)from dual
              ) as "embcount",
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
                      AND EntityLink.EntityType = pEntityType AND (EntityLink.IsDeleted = 0 OR EntityLink.IsDeleted IS NULL)
          left outer join strategy_strategy dest on EntityLink.SeriaId = dest.strategyid
         where strategy_strategy.isdeleted = 0;

  return l_Cursor;

END;
/