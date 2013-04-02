CREATE OR REPLACE FUNCTION StrategyTasks_AddNew
(
pAreaId NUMBER,
pName VARCHAR2,
pDescription VARCHAR2
)
RETURN NUMBER
as
  l_id NUMBER;
begin
  select SEQ_StrategyTask.nextval into l_id from dual;

  INSERT INTO StrategyTasks
  (ID, Name, Description, AreaID)
  VALUES
  (l_id, pName, pDescription, pAreaId);

return l_id;

end;
/