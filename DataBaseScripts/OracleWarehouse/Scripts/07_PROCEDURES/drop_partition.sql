CREATE OR REPLACE PROCEDURE drop_partition
( pPartitionName IN VARCHAR2, pTableName IN VARCHAR2)
AS
cnt number;
BEGIN

   select count(*) into cnt from user_tab_partitions 
   where upper(table_name) = upper(pTableName)
         and upper(partition_name) = upper(pPartitionName);
   if cnt > 0 then
      execute immediate
              'ALTER TABLE ' || pTableName || ' DROP PARTITION ' || pPartitionName;
   end if;
END drop_partition;
/