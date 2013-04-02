CREATE OR REPLACE FUNCTION add_account_hist_field
(pAccountTypeId number, pFieldName varchar2, pFieldType varchar2)
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

  select HistFactTableName
    into l_tablename
    from accounttypes
   where id = pAccountTypeId;

  l_colname := upper(pFieldName);

  execute immediate 'alter table ' || l_tablename || ' add "' || l_colname || '" ' ||
                    l_fieldtype;

  select SEQ_account_type_ha.nextval into l_id from dual;

  insert into accounttypehistoricalparams
    (id, AccountTypeId, FieldName, FieldType)
  values
    (l_id, pAccountTypeId, pFieldName, pFieldType);
  
  return l_id;
commit;
END;
/
