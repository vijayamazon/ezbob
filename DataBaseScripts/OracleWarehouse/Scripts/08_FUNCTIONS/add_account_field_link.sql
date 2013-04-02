CREATE OR REPLACE FUNCTION add_account_field_link
(pAccountTypeId number, pFieldName varchar2, pDictionaryId number)
return number
 AS
  l_tablename   varchar2(30);
  l_tablename_dict varchar2(30);
  l_id number;
  l_colname varchar2(30);
  
  l_constraint_id number;
BEGIN

  select tablename
    into l_tablename
    from accounttypes
   where id = paccountTypeId;

  select tablename
    into l_tablename_dict
    from dictionaries
   where id = pDictionaryId;

  l_colname := upper(pFieldName);

  execute immediate 'alter table ' || l_tablename || ' add "' || l_colname || '" ' ||
                    'number';

  select SEQ_Dict_Constraints.nextval into l_constraint_id from dual;

  execute immediate 'alter table ' || l_tablename || ' add constraint ' ||
                    'FK_' || l_constraint_id ||
                    ' foreign key ("' || l_colname || '") references ' ||
                    l_tablename_dict || '(ID) on delete cascade ';

  select SEQ_account_type_attr.nextval into l_id from dual;
  
  insert into accounttypeparams
    (id, accountTypeId, FieldName, FieldType, DictionaryId)
  values
    (l_id, paccountTypeId, pFieldName, 'Number', pDictionaryId);
  
  return l_id;
commit;
END;
/
