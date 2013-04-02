CREATE OR REPLACE PROCEDURE App_History_ExitEvent
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
 lastLockDate date;
 delta number;
 summTimeOfFlyAcc number;
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
     -- Add exit time
  insert into APPLICATION_HISTORY
    (apphistoryid, userid, securityapplicationid, actiondatetime,  actiontype,   applicationid, currentnodeid )
  values
    (seq_application_history.nextval,pUserId, pSecurityAppid, currentTime, 4,  pApplicationId,  currentNodeID );
    
    if pSecurityAppId = -1 then
           update application_nodetime
           set exittime = currentTime,
           timeoffly = (currentTime - comingtime)*24*3600,
           outagetimelockunlock = (currentTime - comingtime)*24*3600-Nvl(worktime, 0) - Nvl(firsttimeoutage, 0),
           exittype = 1
           where applicationId = pApplicationId and nodeid = currentNodeID
           and exittime is null;
           
           
           for c in (select id, securityapplicationid from application_historysumm where applicationId = pApplicationId and nodeid = currentNodeID)
           loop
             select sum(t.timeoffly) into summTimeOfFlyAcc from application_nodetime t
             where t.applicationid = pApplicationId and t.nodeid = currentNodeID and t.securityapplicationid = c.securityapplicationid;
             
             update application_historysumm
             set lastexittime = currentTime,
             absolutetimeofflynode = (currentTime - firstcomingtime)*24*3600,
             summoutagetimelockunlock = summTimeOfFlyAcc - Nvl(summworktime, 0) - Nvl(summfirsttimeoutage, 0),
             generaloutagetime = summTimeOfFlyAcc - Nvl(summworktime, 0),
             summtimeoffly = summTimeOfFlyAcc
             where id = c.id;
           
           end loop;
           

    end if;    
    

   -- get current summary row id
   BEGIN
        select id  into currentSummaryRowId
          from application_historysumm
         where applicationId = pApplicationId and securityapplicationid = pSecurityAppId
         and nodeid = currentNodeID;
    EXCEPTION
         WHEN no_data_found THEN
           return;--raise_application_error(-20000, 'Summary Id not found');
   END;
   -- get current node time row Id
   BEGIN
     select id, comingtime into currentNodeTimeId, comingTimeNode
     from application_nodetime
           where applicationId = pApplicationId and securityapplicationid = pSecurityAppId
           and nodeid = currentNodeID and exittime is null;
    EXCEPTION
         WHEN no_data_found THEN
           return;--raise_application_error(-20000, 'currentNodeTimeId not found');
    END;
       --get last lock
       select MAX(actiondatetime) into lastLockDate from application_history
       where applicationid = pApplicationId and actiontype=0 and userid = pUserId
       and currentnodeid= currentNodeID and securityapplicationid = pSecurityAppId;

       delta := (currentTime - lastLockDate)*24*3600;

       update application_nodetime
       set worktime =Nvl(worktime, 0) +delta,
       exittime = currentTime,
       timeoffly = (currentTime - comingTimeNode)*24*3600
       where id = currentNodeTimeId;

       update application_historysumm
       set summworktime = Nvl(summworktime, 0) +delta,
       lastexittime = currentTime,
       absolutetimeofflynode = (currentTime - firstcomingtime)*24*3600,
       summtimeoffly = Nvl(summtimeoffly, 0) + (currentTime - comingTimeNode)*24*3600
       where id = currentSummaryRowId;
       
END;
/

