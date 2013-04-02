CREATE OR REPLACE FUNCTION TaskedStrategy_LabelExist
(
pTaskId NUMBER,
pLabel varchar2
)
RETURN NUMBER
as
  l_count NUMBER;
begin

select 	count(*) into l_count 
from TaskedStrategies
where 	TaskID = pTaskId
	and UPPER(Label) = UPPER(pLabel);
	
return l_count;

end;
/