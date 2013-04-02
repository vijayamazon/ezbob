CREATE OR REPLACE PROCEDURE App_History_Insert
-- Created by A.Grechko
  -- Date 04.02.08

  --Modefied by I.Borzenkov
  --Added IsTimeLimitExceeded and CurrentNodeID storing into APPLICATION_HISTORY table
  --15.04.2008

  --Modefied by V.Moshenok
  --Removed IsTimeLimitExceeded from APPLICATION_HISTORY table
  --22.05.2008

(pUserId        number,
 pSecurityAppId number,
 pActionType    number,
 pApplicationId number)

 AS
   lCurrentNodeID NUMBER;
BEGIN
   BEGIN
   SELECT currentnodeid
          INTO lCurrentNodeID
          FROM strategyengine_executionstate
         WHERE applicationId = pApplicationId;
    EXCEPTION
         WHEN no_data_found
         THEN lCurrentNodeID:=NULL;
   END;

  insert into APPLICATION_HISTORY
    (apphistoryid,
     userid,
     securityapplicationid,
     actiondatetime,
     actiontype,
     applicationid,
     currentnodeid
   )
  values
    (seq_application_history.nextval,
     pUserId,
     pSecurityAppid,
     sysdate,
     pActionType,
     pApplicationId,
     lCurrentNodeID
   );

END;
/
