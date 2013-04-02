CREATE OR REPLACE FUNCTION StrategyArea_NameExist
(
pName varchar2
)
RETURN NUMBER
as
  l_count NUMBER;
begin

select 	count(*) into l_count 
from StrategyAreas
where UPPER(Name) = UPPER(pName);
	
return l_count;

end;
/