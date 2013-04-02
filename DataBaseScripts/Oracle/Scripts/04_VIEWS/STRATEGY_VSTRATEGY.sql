CREATE OR REPLACE VIEW STRATEGY_VSTRATEGY AS
SELECT ss.StrategyID,
       ss.CurrentVersionID,
       ss.Name,
       ss.Description,
       ss.Icon,
       ss.IsEmbeddingAllowed,
       ss.UserId,
       ss.AuthorId,
       ss.State,
       ss.SubState,
       ss.IsDeleted,
       ss.StrategyType,
       su.FullName,
       su1.FullName as AuthorName,
       ss.ExecutionDuration,
       ss.displayname,
       ss.termdate,
       (SELECT COUNT(strategyId) FROM Strategy_Publicrel
            WHERE strategyId = ss.StrategyID) as STRATEGYCOUNT
  FROM Strategy_Strategy ss LEFT
     OUTER JOIN Security_User su ON ss.UserId = su.UserId
     LEFT OUTER JOIN Security_User su1  ON ss.AuthorId = su1.UserId
/
