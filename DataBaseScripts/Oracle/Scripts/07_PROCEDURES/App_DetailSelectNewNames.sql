create or replace Procedure App_DetailSelectNewNames
  (pLastParameterId number, cur_OUT in out sys_refcursor)
as
begin
 open cur_OUT for
  select
    DetailNameID id,
    Name name
  from Application_DetailName
  where DetailNameID > pLastParameterId;
end;
/