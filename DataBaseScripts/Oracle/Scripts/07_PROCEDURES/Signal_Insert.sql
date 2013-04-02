CREATE OR REPLACE PROCEDURE Signal_Insert
  (
    pTarget IN VARCHAR2,
    pLabel  IN VARCHAR2,
    pAppSpecific IN NUMBER,
    pApplicationId IN NUMBER,
    pPriority IN NUMBER,
    pOwnerAppId IN NUMBER,
    pMessage IN BLOB
   )
AS
 l_id Number;
 l_executionType Number;
BEGIN
  Select SEQ_Signal.NEXTVAL into l_id from dual;

  BEGIN
  
	  select ExecutionType into l_executionType
	  from Application_ExecutionType
	  where ApplicationId = pApplicationId;
  EXCEPTION
         WHEN no_data_found
         THEN l_executionType:=NULL;
   END;
   
  insert into Signal
    (Id, Target, Label, Status, AppSpecific, ApplicationId, Priority, OwnerApplicationId, Message, ExecutionType)
  values
    (l_id, pTarget, pLabel, 0, pAppSpecific, pApplicationId, pPriority, pOwnerAppId, pMessage, l_executionType);

END Signal_Insert;
/
