CREATE OR REPLACE PROCEDURE delete_customer_field
(pCustomerTypeId number, pFieldName varchar2 )
 AS
  l_tablename varchar2(30);
  l_colname varchar2(30);
BEGIN
   
  select tablename
    into l_tablename
    from customertypes
   where id = pCustomerTypeId;

  l_colname := upper(pFieldName);

  execute immediate 'alter table ' || l_tablename || ' drop column "' || l_colname || '"';

  delete from customertypeparams
  where CustomerTypeId = pCustomerTypeId and 
  upper(FieldName) = upper(pFieldName) ;
  commit;
END;
/
