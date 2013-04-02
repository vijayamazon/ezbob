CREATE OR REPLACE PROCEDURE temp_table_partition_exchange
( pPartitionName IN VARCHAR2, pTableName IN VARCHAR2, pBaseTableName IN VARCHAR2)
AS
BEGIN
   execute immediate '
ALTER TABLE ' || pBaseTableName ||
' EXCHANGE PARTITION ' || pPartitionName ||
' WITH TABLE ' || pTableName ||
' WITHOUT VALIDATION
UPDATE GLOBAL INDEXES';
END temp_table_partition_exchange;
/
