CREATE OR REPLACE PROCEDURE add_fact_table_partition
( pMax IN  NUMBER, pPartitionName IN VARCHAR2, pFactTableName IN VARCHAR2)
AS
BEGIN
   execute immediate 
   'ALTER TABLE ' || pFactTableName || ' ADD PARTITION ' || pPartitionName || ' VALUES LESS THAN (' || pMax || ')';
END add_fact_table_partition;
/
