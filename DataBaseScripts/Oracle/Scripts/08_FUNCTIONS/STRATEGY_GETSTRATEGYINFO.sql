CREATE OR REPLACE FUNCTION Strategy_GetStrategyInfo
(
  pStrategyName in varchar2,
	pIsEmbeddingAllowed in Number,
	pIsPublished in Number,
  pUserId in Number

) return sys_refcursor
AS
  l_Strategy sys_refcursor;
  l_isPublished Number;
  applicationInProgress Number;
BEGIN

  OPEN l_Strategy FOR SELECT
             s.StrategyId  AS StrategyId
             , s.displayname AS Name
             , s.Description AS Description
             , u.FullName AS Author
             , s.CreationDate AS PublishingDate
             , (select count(applicationid)  from application_application where
       strategyid = s.strategyid and state not in(2,3,0)) as ApplicationInProgress
             ,case s.SubState
                     when 0 /* Locked */ then case s.UserId
                                                                          when pUserId then (select count(applicationid) from application_application where
                                                                                       strategyid = s.strategyid and state not in(2,3,0))
                                                                          else 1
                                                                end
                     when 1 then (select count(applicationid) from application_application where
                                 strategyid = s.strategyid and state not in(2,3,0))
                     else 1
              end AS IsReadOnly
            , (select COUNT(publicid) from strategy_publicrel
where strategyid = s.strategyid) AS IsPublished
FROM Strategy_Strategy s JOIN Security_User u ON s.AuthorID = u.UserID
WHERE  s.displayname = pStrategyName and s.isdeleted = 0 and s.termdate is null;



  return l_Strategy;

END;
/

