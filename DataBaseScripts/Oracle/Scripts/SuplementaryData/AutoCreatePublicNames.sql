create or replace procedure AutocreatePublicnames
as
currentNameId number;
begin
  for c in (select strategyid, currentversionid, name, description, isembeddingallowed, xml, isdeleted, userid, authorid, state, substate, creationdate, icon, executionduration, lastupdatedate 
            from strategy_strategy
            where isdeleted = 0)
  loop
      select seq_app_public_name.nextval into currentNameId from dual;
      -- add public name
      insert into strategy_publicname
        (publicnameid, name)
      values
        (currentNameId, c.name);
      -- publish strategy 
      strategy_strategypublish(pstrategyid => c.strategyid,
                               ppublishnameid => currentNameId,
                               ppercent => 100);
  end loop;
  commit;
end ;