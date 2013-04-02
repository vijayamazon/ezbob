CREATE OR REPLACE FUNCTION StrategyTask_NameExist
(
pAreaId NUMBER,
pName varchar2
)
RETURN NUMBER
as
  l_count NUMBER;
begin

select 	count(*) into l_count 
from StrategyTasks
where 	AreaID = pAreaId
	and UPPER(Name) = UPPER(pName);
	
return l_count;

end;
/