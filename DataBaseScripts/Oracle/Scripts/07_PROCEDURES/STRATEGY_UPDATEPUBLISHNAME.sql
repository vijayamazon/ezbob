CREATE OR REPLACE PROCEDURE Strategy_UpdatePublishName
  (
    pId IN OUT number,
    pName IN varchar2,
    pIsStopped IN number
   )
AS
  nameId Number;
  l_id Number;
BEGIN

  if pId is not null then
      select COUNT(publicnameid) into l_id
      from strategy_publicname
      where upper(name) = upper(pName) 
        and publicnameid <> pId
        and (IsDeleted is null or IsDeleted = 0);

      if (l_id > 0) then raise_application_error(-20100,'dublicated_name');
      end if;
      
       update strategy_publicname
        set name = pName,
            isstopped = pIsStopped
      where publicnameid = pId;

else
      select COUNT(publicnameid) into l_id from strategy_publicname
      where upper(name) = upper(pName)
        and (IsDeleted is null or IsDeleted = 0);

      if (l_id > 0) then raise_application_error(-20100,'dublicated_name');
      end if;
      Select seq_app_public_name.nextval into nameId from dual;
      pId := nameId;
      
      insert into strategy_publicname
        (publicnameid, name)
      values
        (pId, pName);
  end if;

END;
/

