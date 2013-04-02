create or replace procedure Save_Application_Results
(
  pApplicationId IN NUMBER,
  pName VARCHAR2,
  pValue CLOB,
  pType VARCHAR2,
  pDirection IN INTEGER
)
as
  l_result_id number;
begin
   Select SEQ_Application_Result.Nextval into l_result_id from dual;

   insert into Application_Result
     (ID, APPLICATIONID, NAME, VALUE, TYPE, DIRECTION)
   values
     (l_result_id, pApplicationId, pName, pValue, pType, pDirection);
end;
/