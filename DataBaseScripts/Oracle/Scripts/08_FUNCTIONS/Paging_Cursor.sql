create or replace function Paging_Cursor
     (pTables     varchar2,
      pPK         varchar2,
      pSort       varchar2,
      pPageNumber Number,
      pPageSize   Number,
      pFields      varchar default '*',
      pFilter      varchar2,
      pGroup      varchar2) return sys_refcursor
as
  l_cur_paging sys_refcursor;
  l_tbl_cur sys_refcursor;
  l_strFilter Varchar2(4000);
  l_strGroup Varchar2(4000);
  l_pageNumber Number;
  l_StartRowNumber Number;
  l_endrowNumber Number;
  l_pSort Varchar2(4000);
  l_tables Varchar2(4000);
  l_sql Varchar2(4000);

begin
  l_pageNumber := pPageNumber;

  if l_pageNumber < 1 then
   l_pageNumber := 1;
  end if;

  l_StartRowNumber := ((l_pageNumber * pPageSize) - pPageSize) + 1;
  l_endrowNumber := (l_pageNumber * pPageSize);



  if pFilter is not null then
    l_strFilter := ' WHERE ' || pFilter || ' ';
  else
    l_strFilter := '';
  end if;

  if pGroup is not null then
    l_strGroup := ' GROUP BY ' || pGroup || ' ';
  else
    l_strGroup := '';
  end if;

  if pSort is null then
    l_pSort := pPk;
  else
   l_pSort := pSort;
  end if;

--âRÿÿ?â÷àÿ??õ ÷ïæ<à-<? ý?-â³àà
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
          )' using pTables;

  fetch l_tbl_cur into l_tables;

  l_sql :=
    'Select '||pFields||' from
     (
       Select rownum as rn, a.*
       from
        (
           SELECT 0 as rnx '
              ||(case
                    when pFields is not null then
                      ','
                    else
                      null
                  end)
              ||pFields||' FROM '||
             l_tables || l_strFilter || ' ' || l_strGroup || ' ORDER BY ' || l_pSort
         ||' ) a '
     ||') where rn >= '||to_char(l_StartRowNumber)||' and rn <= '||to_char(l_EndRowNumber);

  open l_cur_paging for l_sql;

   return l_cur_paging;
end;
/
