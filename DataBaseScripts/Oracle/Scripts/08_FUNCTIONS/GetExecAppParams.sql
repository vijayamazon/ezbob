CREATE OR REPLACE FUNCTION GetExecAppParams
 (
    pApplicationId IN NUMBER
 ) RETURN sys_refcursor
AS
  l_cur sys_refcursor;
BEGIN
-- Description:  Select all details for given ApplicationID
   open l_cur for
      SELECT
        STRATEGYENGINE_EXECUTIONSTATE.ID,
        STRATEGY_NODE.NODEID,
        STRATEGYENGINE_EXECUTIONSTATE.APPLICATIONID,
        APPLICATION_APPLICATION.CREATORUSERID,
        APPLICATION_APPLICATION.VERSION,
        STRATEGY_NODE.ISHARDREACTION
      FROM
        STRATEGY_NODE
        INNER JOIN STRATEGYENGINE_EXECUTIONSTATE
            ON STRATEGY_NODE.NODEID = STRATEGYENGINE_EXECUTIONSTATE.CURRENTNODEID
        INNER JOIN APPLICATION_APPLICATION
            ON STRATEGYENGINE_EXECUTIONSTATE.APPLICATIONID = APPLICATION_APPLICATION.APPLICATIONID
      WHERE
        STRATEGYENGINE_EXECUTIONSTATE.APPLICATIONID = pApplicationId;
   RETURN l_cur;
  
END GetExecAppParams;
/