CREATE OR REPLACE FUNCTION add_customer_field
(pCustomerTypeId number, pFieldName varchar2, pFieldType varchar2)
return number
 AS
  l_tablename varchar2(30);
  l_fieldtype varchar2(20);
  l_id number;
  l_colname varchar2(30);
BEGIN

  select decode(pFieldType,
                'DateTime',
                'Date',
                'Number',
                'Number',
                'String',
                'varchar2(255)',
                pFieldType)
    into l_fieldtype
    from dual;

  select tablename
    into l_tablename
    from customertypes
   where id = pCustomerTypeId;

  l_colname := upper(pFieldName);

  execute immediate 'alter table ' || l_tablename || ' add "' || l_colname || '" ' ||
                    l_fieldtype;

  select SEQ_customer_type_attr.nextval into l_id from dual;

  insert into customertypeparams
    (id, CustomerTypeId, FieldName, FieldType)
  values
    (l_id, pCustomerTypeId, pFieldName, pFieldType);

  return l_id;

commit;
END;
/
