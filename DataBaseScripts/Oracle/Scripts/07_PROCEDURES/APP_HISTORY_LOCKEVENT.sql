CREATE OR REPLACE PROCEDURE App_History_LockEvent
(
 pUserId number,
 pSecurityAppId number,
 pApplicationId number
)

AS
 currentTime date;
 comingTime date;
 currentNodeID NUMBER;
 currentSummaryRowId NUMBER;
 currentNodeTimeId NUMBER;
 lastLockDate date;
 lastUnlockDate date;
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

   BEGIN
   
     -- get current node time
     select id, comingtime  into currentNodeTimeId, comingTime
     from application_nodetime
           where applicationId = pApplicationId  and (nodeid = currentNodeID or (nodeid is null and currentNodeID is null))
           and firsttimeoutage is null and exittime is null;
    -- get current summary row id
     BEGIN
          select id  into currentSummaryRowId
            from application_historysumm
           where applicationId = pApplicationId and (nodeid = currentNodeID or (nodeid is null and currentNodeID is null));
      EXCEPTION
           WHEN no_data_found THEN
           raise_application_error(-20000, 'Summary Id not found for first outage update');
     END;
    
    delta := (currentTime - comingTime)*24*3600;
    
    update application_nodetime
       set firsttimeoutage = delta,
       userid = pUserId,
       securityapplicationid = pSecurityAppId
       where id = currentNodeTimeId;
    update application_historysumm
       set summfirsttimeoutage = Nvl(summfirsttimeoutage, 0)+delta,
       generaloutagetime = Nvl(generaloutagetime, 0) + delta,
       securityapplicationid = pSecurityAppId
       where id = currentSummaryRowId;

    EXCEPTION
         WHEN no_data_found THEN
         BEGIN
           select id into currentNodeTimeId from application_nodetime
                where applicationId = pApplicationId and securityapplicationid = pSecurityAppId
                and (nodeid = currentNodeID or (nodeid is null and currentNodeID is null)) and userid = pUserId and exittime is null;
         EXCEPTION --autoexit
           WHEN no_data_found THEN
           select max(id) into currentNodeTimeId from application_nodetime
           where applicationId = pApplicationId
           and (nodeid = currentNodeID or (nodeid is null and currentNodeID is null)) and exittime is null;
         END;
         -- get current summary row id
         BEGIN
              select id  into currentSummaryRowId
                from application_historysumm
               where applicationId = pApplicationId and securityapplicationid = pSecurityAppId
               and (nodeid = currentNodeID or (nodeid is null and currentNodeID is null));
          EXCEPTION
               WHEN no_data_found THEN -- autoexit
               select max(id)  into currentSummaryRowId
                from application_historysumm
               where applicationId = pApplicationId
               and (nodeid = currentNodeID or (nodeid is null and currentNodeID is null));
         END;
         --get last lock replace null with min date value, used ugly code, needed refactoring
         select Nvl(MAX(actiondatetime), sysdate-1000000) into lastLockDate from application_history
         where applicationid = pApplicationId and actiontype=0 and userid = pUserId
        and currentnodeid= currentNodeID and securityapplicationid = pSecurityAppId;
        --get last unlock
        select Nvl(MAX(actiondatetime), sysdate-1000000) into lastUnlockDate from application_history
        where applicationid = pApplicationId and actiontype=1 and userid = pUserId
        and currentnodeid= currentNodeID and securityapplicationid = pSecurityAppId;

        if lastLockDate > lastUnlockDate then
           delta := (currentTime - lastLockDate)*24*3600;
        else
           delta := (currentTime - lastUnlockDate)*24*3600;
        end if;

       update application_nodetime
       set outagetimelockunlock = Nvl(outagetimelockunlock, 0)+delta
       where id = currentNodeTimeId;

       update application_historysumm
       set  summoutagetimelockunlock = Nvl(summoutagetimelockunlock, 0)+delta,
       generaloutagetime = Nvl(generaloutagetime, 0) + delta
       where id = currentSummaryRowId;
   END;

END;
/

