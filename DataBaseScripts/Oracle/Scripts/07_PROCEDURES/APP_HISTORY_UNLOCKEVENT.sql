CREATE OR REPLACE PROCEDURE App_History_UnlockEvent
(
 pUserId number,
 pSecurityAppId number,
 pApplicationId number
)

AS
 currentTime date;
 currentNodeID NUMBER;
 currentSummaryRowId NUMBER;
 currentNodeTimeId NUMBER;
 lastLockDate date;
 delta number;
BEGIN
   currentTime := sysdate;
   BEGIN
   --get current node id
   SELECT currentnodeid
          INTO currentNodeID
          FROM strategyengine_executionstate
         WHERE applicationId = pApplicationId;
    EXCEPTION
         WHEN no_data_found
         THEN currentNodeID:=NULL;
   END;
   -- get current summary row id
   BEGIN
        select id  into currentSummaryRowId
          from application_historysumm
         where applicationId = pApplicationId and securityapplicationid = pSecurityAppId
         and nodeid = currentNodeID;
    EXCEPTION
         WHEN no_data_found THEN
         raise_application_error(-20000, 'Summary Id not found');
   END;
   -- get current node time row Id
   BEGIN
     select id into currentNodeTimeId
     from application_nodetime
           where applicationId = pApplicationId and securityapplicationid = pSecurityAppId
           and nodeid = currentNodeID and exittime is null;
    EXCEPTION
         WHEN no_data_found THEN
         raise_application_error(-20000, 'currentNodeTimeId not found');
    END;
       --get last lock
       select MAX(actiondatetime) into lastLockDate from application_history
       where applicationid = pApplicationId and actiontype=0 and userid = pUserId
       and currentnodeid= currentNodeID and securityapplicationid = pSecurityAppId;

       delta := (currentTime - lastLockDate)*24*3600;

       update application_nodetime
       set worktime =Nvl(worktime, 0) +delta
       where id = currentNodeTimeId;

       update application_historysumm
       set summworktime  = Nvl(summworktime, 0) +delta
       where id = currentSummaryRowId;

END;
/

