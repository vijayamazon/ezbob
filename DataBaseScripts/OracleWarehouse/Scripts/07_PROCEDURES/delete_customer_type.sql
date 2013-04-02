CREATE OR REPLACE PROCEDURE delete_customer_type
(pCustomerTypeId number)
 AS

  l_tablename varchar2(30);
  l_cnt       number;

  l_tablename_hf   varchar2(30);
  l_tablename_h     varchar2(30);
  l_tablename_h2hr   varchar2(30);

BEGIN

    select tablename,
           histfacttablename,
           historytablename,
           h2hrtablename
      into l_tablename, l_tablename_hf, l_tablename_h, l_tablename_h2hr
      from customertypes
     where id = pCustomerTypeId;

 execute immediate 'select count(*)  from '|| l_tablename into l_cnt;

  if l_cnt = 0 then

    execute immediate 'drop table ' || l_tablename_h2hr;
    execute immediate 'drop table ' || l_tablename_h;
    execute immediate 'drop table ' || l_tablename_hf;
    execute immediate 'drop table ' || l_tablename;

    delete from customertypehistoricalparams where customertypeid = pCustomerTypeId;
    delete from customertypeparams where customertypeid = pCustomerTypeId;
    delete from customertypes where Id = pCustomerTypeId;

  end if;
commit;
END;
/