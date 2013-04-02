CREATE OR REPLACE PROCEDURE add_dictionary_field_link

(pDictionaryId number, pDisplayName varchar2, pMasterDictionaryId number)

 AS

  l_tablename   varchar2(30);
  l_tablename_1 varchar2(30);
  l_field_number number;
  l_field_name varchar2(30);
  
  l_constraint_id number;

BEGIN

 select SEQ_dict_attr.nextval into l_field_number from dual;

 l_field_name := 'field_'||l_field_number;
 
  select tablename
    into l_tablename
    from dictionaries
   where id = pdictionaryId;

  execute immediate 'alter table ' || l_tablename || ' add ' || l_field_name || ' ' ||
                    'number';

  select tablename
    into l_tablename_1
    from dictionaries
   where id = pMasterDictionaryId;

  select SEQ_Dict_Constraints.nextval into l_constraint_id from dual;

  execute immediate 'alter table ' || l_tablename || ' add constraint ' ||
                    'FK_' || l_constraint_id ||
                    ' foreign key (' || l_field_name || ') references ' ||
                    l_tablename_1 || '(ID) on delete cascade ';

  insert into dictionaryparams
    (id, dictionaryId, FieldName,DisplayName, FieldType, IsDeleted, MasterDictionaryId)
  values
    (l_field_number,
     pdictionaryId,
     l_field_name,
     pDisplayName,
     'Number',
     null,
     pMasterDictionaryId);
commit;
END;
/
