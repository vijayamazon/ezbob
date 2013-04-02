CREATE OR REPLACE FUNCTION GetPagingCursor
(
  pSQL in varchar2,
  pPageNumber in Number,
  pItemsPerPage in Number,
  pNeedTotalCount in out number
) return sys_refcursor
AS
  l_Cursor sys_refcursor;
  l_sql varchar2(5000);
  startIndex number;
  endIndex number;
BEGIN

  l_sql := pSQL;

  startIndex := pPageNumber * pItemsPerPage + 1;
  endIndex :=  (pPageNumber + 1) * pItemsPerPage;
  EXECUTE   IMMEDIATE 'select count(*) from ('||l_sql||') '
  into pNeedTotalCount;

  OPEN l_Cursor FOR 'Select * from (
select t1.*, ROWNUM rn from (
'|| l_sql ||'
) t1
) where rn between :startIndex and :endIndex' using
  startIndex, endIndex;

  return l_Cursor;

END;
/