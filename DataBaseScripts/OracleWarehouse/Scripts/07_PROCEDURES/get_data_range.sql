CREATE OR REPLACE PROCEDURE get_data_range
( pDataSequenceName VARCHAR2, pSize IN  NUMBER, pMin  OUT NUMBER, pMax  OUT NUMBER)
AS
 l_size NUMBER;
BEGIN
   if pSize > 0 then
    execute immediate 'select ' || pDataSequenceName || '.Nextval from dual' into pMin;
    l_size := pSize;
    execute immediate 'ALTER SEQUENCE ' || pDataSequenceName || ' increment by ' || l_size;
    pMax := pMin + l_size;
   end if ;
END get_data_range;
/
