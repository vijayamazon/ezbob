CREATE OR REPLACE FUNCTION add_customer_hist_field_link
(pCustomerTypeId number, pFieldName varchar2, pDictionaryId number)
return number
 AS
  l_tablename  varchar2(30);
  l_tablename_dict varchar2(30);
  l_id number;
  l_colname varchar2(30);
  l_constraint_id number;

BEGIN

  select HistFactTableName
    into l_tablename
    from customertypes
   where id = pCustomerTypeId;

  select tablename
    into l_tablename_dict
    from dictionaries
   where id = pDictionaryId;

  l_colname := upper(pFieldName);

  execute immediate 'alter table ' || l_tablename || ' add "' || l_colname || '" ' ||
                    'number';

  select SEQ_customer_type_ha.nextval into l_id from dual;

  insert into customertypehistoricalparams
    (id, CustomerTypeId, FieldName, FieldType, DictionaryId)
  values
    (l_id, pCustomerTypeId, pFieldName, 'Number', pDictionaryId);

  return l_id;
commit;
END;
/
