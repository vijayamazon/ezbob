CREATE OR REPLACE PROCEDURE add_dictionary_field

(pDictionaryId number, pDisplayName varchar2, pFieldType varchar2, pDefaultValue varchar2)

 AS

  l_tablename varchar2(30);
  l_fieldtype varchar2(20);
  l_field_number number;
  l_field_name varchar2(30);

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

 select SEQ_dict_attr.nextval into l_field_number from dual;

 l_field_name := 'field_'||l_field_number;

  select tablename
    into l_tablename
    from dictionaries
   where id = pDictionaryId;

if pDefaultValue is not null then
  execute immediate 'alter table ' || l_tablename || ' add ' || l_field_name || ' ' ||
                    l_fieldtype||' default '||pDefaultValue;
                    else
                    execute immediate 'alter table ' || l_tablename || ' add ' || l_field_name || ' ' ||
                    l_fieldtype;
                    
end if;
 



 insert into dictionaryparams
    (Id, DictionaryId, FieldName, DisplayName, FieldType, DefValue)
  values
    (l_field_number, pDictionaryId, l_field_name, pDisplayName, pFieldType, pDefaultValue );
commit;
END;
/
