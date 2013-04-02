CREATE OR REPLACE FUNCTION StrategyAreas_AddNew
(
pName VARCHAR2,
pDescription VARCHAR2
)
RETURN NUMBER
as
  l_id NUMBER;
begin
  select SEQ_StrategyArea.nextval into l_id from dual;

  INSERT INTO StrategyAreas
  (ID, Name, Description)
  VALUES
  (l_id, pName, pDescription);

return l_id;

end;
/