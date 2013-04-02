CREATE OR REPLACE PROCEDURE ResetAllSignals
AS
BEGIN
execute immediate 
    'update Signal set Status = 0 where IsExternal is null or IsExternal = 0 ';
END ResetAllSignals;
/
