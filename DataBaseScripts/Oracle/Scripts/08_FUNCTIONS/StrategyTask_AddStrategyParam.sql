CREATE OR REPLACE FUNCTION StrategyTask_AddStrategyParam
(
pTaskedStrategyId NUMBER,
pName VARCHAR2,
pDisplayName VARCHAR2,
pDescription VARCHAR2,
pTypeName VARCHAR2,
pInitialValue VARCHAR2,
pConstraint VARCHAR2
)
RETURN NUMBER
as
  l_id NUMBER;
begin
  select SEQ_TaskedStrategyParam.nextval into l_id from dual;

  INSERT INTO TaskedStrategyParams
  (ID, TSID, Name, DisplayName, Description, TypeName, InitialValue, ConstraintString)
  VALUES
  (l_id, pTaskedStrategyId, pName, pDisplayName, pDescription, pTypeName, pInitialValue, pConstraint);

return l_id;

end;
/