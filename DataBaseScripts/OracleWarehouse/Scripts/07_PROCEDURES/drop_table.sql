CREATE OR REPLACE PROCEDURE drop_table
(pTableName IN VARCHAR2)
AS
 cnt NUMBER;
BEGIN
   select count(*) into cnt from user_tables where upper(table_name) = upper(pTableName);
   if cnt > 0 then
      execute immediate 
      'drop table ' || pTableName;
   end if;
END drop_table;
/

