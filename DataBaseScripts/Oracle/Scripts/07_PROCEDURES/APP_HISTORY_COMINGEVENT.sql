CREATE OR REPLACE PROCEDURE App_History_ComingEvent

(pUserId        number,
 pSecurityAppId number,
 pApplicationId number)

 AS
   lCurrentNodeID number;
   currentDate date;
   currentSummaryId number;
BEGIN
   BEGIN
   currentDate := sysdate;

   SELECT currentnodeid
          INTO lCurrentNodeID
          FROM strategyengine_executionstate
         WHERE applicationId = pApplicationId;
    EXCEPTION
         WHEN no_data_found
         THEN lCurrentNodeID:=NULL;
   END;

  -- Add coming time
  insert into APPLICATION_HISTORY
    (apphistoryid, userid, securityapplicationid, actiondatetime,  actiontype,   applicationid, currentnodeid )
  values
    (seq_application_history.nextval,pUserId, pSecurityAppid, currentDate, 3,  pApplicationId,  lCurrentNodeID );
  -- Create new row history node time
  insert into application_nodetime
    (id, applicationid, nodeid, comingtime)
  values
    (seq_app_nodetime.nextval, pApplicationId, lCurrentNodeID, currentDate);
  -- if needed add new row to app history summ table
  BEGIN
    select id into currentSummaryId
      from application_historysumm
     where applicationid = pApplicationId and nodeid = lCurrentNodeID;
  EXCEPTION
         WHEN no_data_found
         THEN
         insert into application_historysumm
           (id, applicationid, nodeid, firstcomingtime)
         values
           (seq_app_historysumm.nextval, pApplicationId, lCurrentNodeID,  currentDate);
   END;


END;
/

