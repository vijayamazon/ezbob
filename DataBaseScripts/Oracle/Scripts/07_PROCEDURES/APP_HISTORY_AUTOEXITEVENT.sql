CREATE OR REPLACE PROCEDURE App_History_AutoExitEvent
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
 comingTimeNode date;
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
     -- Add coming time
  insert into APPLICATION_HISTORY
    (apphistoryid, userid, securityapplicationid, actiondatetime,  actiontype,   applicationid, currentnodeid )
  values
    (seq_application_history.nextval,pUserId, pSecurityAppid, currentTime, 4,  pApplicationId,  currentNodeID );


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
     select id, comingtime into currentNodeTimeId, comingTimeNode
     from application_nodetime
           where applicationId = pApplicationId and securityapplicationid = pSecurityAppId
           and nodeid = currentNodeID and exittime is null;
    EXCEPTION
         WHEN no_data_found THEN
         raise_application_error(-20000, 'currentNodeTimeId not found');
    END;

       update application_nodetime
       set outagetimelockunlock =(currentTime - comingTimeNode)*24*3600,
       exittime = currentTime,
       timeoffly = (currentTime - comingTimeNode)*24*3600
       where id = currentNodeTimeId;

       update application_historysumm
       set lastexittime = currentTime,
       generaloutagetime = Nvl(generaloutagetime, 0) + (currentTime - comingTimeNode)*24*3600,
       absolutetimeofflynode = (currentTime - firstcomingtime)*24*3600,
       summtimeoffly = Nvl(summtimeoffly, 0) + (currentTime - comingTimeNode)*24*3600
       where id = currentSummaryRowId;

END;
/

