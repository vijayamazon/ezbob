CREATE OR REPLACE PROCEDURE StrategyArea_RemoveArea
(
pAreaId NUMBER
)
as
begin

  DELETE FROM StrategyAreas where ID = pAreaId;

end;
/