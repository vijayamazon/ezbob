  --Modefied by Gorbach Alexey
  --Added exception handling in case when pCurrentNodeType is empty, i.e. we are processing embedded strategy
  --in this case we'll write null in table StrategyEngine_ExecutionState in field CurrentNodeId.
  --28.05.2008

CREATE OR REPLACE PROCEDURE StrategyEngine_ExecStatePush
 (
	 pExecutionState IN clob,
 	 pCurrentNodeType IN varchar2,
	 pCurrentNodePostfix IN varchar2,
	 pApplicationId IN Number,
 	 pExecutionPathBin IN BLOB
 )
AS
 l_CurrentNodeId Number;
 l_id Number;
BEGIN

  begin
    select Strategy_Node.NodeId into l_CurrentNodeId
    from   Strategy_Node 
           inner join Strategy_NodeStrategyRel on Strategy_Node.NodeId = Strategy_NodeStrategyRel.NodeId 
           inner join Application_Application on Strategy_NodeStrategyRel.StrategyId = Application_Application.StrategyId
     where Application_Application.ApplicationId = pApplicationId and Strategy_Node.Name||Strategy_Node.Guid = pCurrentNodeType;
  exception
     when no_data_found
     then
     l_CurrentNodeId := NULL;
  end;

  App_ExecutionPathUpdate(pApplicationId => pApplicationId, pExecutionPathBin => pExecutionPathBin);

  Select SEQ_SE_EXECUTIONSTATE.NEXTVAL into l_id from dual;

	INSERT INTO StrategyEngine_ExecutionState(Id, Data, ApplicationId,
                                CurrentNodeId, CurrentNodePostfix)
	VALUES(l_id, pExecutionState, pApplicationId,
                                l_CurrentNodeId, pCurrentNodePostfix);

END;

/
