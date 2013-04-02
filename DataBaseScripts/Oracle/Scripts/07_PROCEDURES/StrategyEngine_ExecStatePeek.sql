CREATE OR REPLACE PROCEDURE StrategyEngine_ExecStatePeek
(
    pApplicationId in Number,
    cur_OUT in out sys_refcursor
)
AS
 lId Number;
BEGIN

    SELECT MAX(ID) into lId
    FROM StrategyEngine_ExecutionState
    WHERE ApplicationId = pApplicationId;

    Open cur_OUT for
    SELECT s.Data, s.CurrentNodePostfix as CurrentNodeName, a.ExecutionPath as ExecutionPath, a.ExecutionPathBin as ExecutionPathBin
    FROM StrategyEngine_ExecutionState s 
          left outer join Strategy_Node n 
              on n.NodeId = s.CurrentNodeId
          INNER JOIN Application_Application a
              on s.ApplicationId = a.ApplicationId
    WHERE s.Id = lId;

END;
/
