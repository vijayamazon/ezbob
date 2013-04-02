CREATE OR REPLACE PROCEDURE add_ref_table_partition
( pHistoryId IN  NUMBER, pPartitionName IN VARCHAR2, pReferenceTableName IN VARCHAR2)
AS
BEGIN
   execute immediate 
   'ALTER TABLE ' || pReferenceTableName || ' ADD PARTITION ' || pPartitionName || ' VALUES (' || pHistoryId || ')';
END add_ref_table_partition;
/
