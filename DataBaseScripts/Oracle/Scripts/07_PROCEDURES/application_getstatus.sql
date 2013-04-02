CREATE OR REPLACE Procedure application_getstatus
(
   pApplicationId in number,
   cur_OUT in out sys_refcursor
)
AS
begin
  open cur_OUT for
       select signal.target,
       signal.label,
       signal.status,
       signal.starttime,
       signal.appspecific,
       strategyengine_executionstate.currentnodeid,
       strategy_node.name,
       strategyengine_executionstate.currentnodepostfix,
       application_application.executionPath,
       application_application.executionPathBin,
       application_application.lockedbyuserid,
       application_application.state,
       application_application.istimelimitexceeded,
       application_application.version,
       application_application.strategyid

       from application_application
        left outer join signal on
           application_application.applicationid = signal.applicationid 
        left outer join strategyengine_executionstate on 
           application_application.applicationid = strategyengine_executionstate.applicationid 
        left outer join strategy_node on 
           strategy_node.nodeid = strategyengine_executionstate.currentnodeid 

       where application_application.applicationid = pApplicationId;
end;
/