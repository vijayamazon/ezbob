create or replace function PAGING_CURSOR_COUNT
(
  pTableName IN Varchar2,
  pCondition IN Varchar2
) return number
as
  l_cur sys_refcursor;
  l_tbl_cur sys_refcursor;
  l_tables Varchar2(4000);
  l_value Number;
begin
  open l_tbl_cur for
   'select substr(extract(xmlagg(xmlelement("X", '',''||table_name)), ''X/text()'').getstringval(), 2) table_names
     from
        (
        Select
           NVL(a.OBJECT_NAME, ''TABLE(''||n.obj_name||'')'') table_name
        from all_objects a
          inner join
            (select owner from user_constraints
            where rownum = 1) o
            on a.owner = o.owner and (a.object_type = ''TABLE'' or a.object_type = ''VIEW'')
          right outer join
            ( Select UPPER(COLUMN_VALUE) as obj_name from TABLE(str2tbl(:tables)) ) n
          on n.obj_name = a.object_name
          )' using pTableName;

  fetch l_tbl_cur into l_tables;

  open l_cur for
   ' SELECT count(*) FROM '||l_tables||' WHERE  '|| NVL(pCondition, '1=1');
   fetch l_cur into l_value;

   return l_value;
end;
/