CREATE OR REPLACE PROCEDURE UpdateSignalsSelectingStatus
AS
BEGIN
execute immediate 
    'update Signal set Status = 1 where Status=-1';
END UpdateSignalsSelectingStatus;
/
