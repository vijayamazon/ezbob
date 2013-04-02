CREATE OR REPLACE FUNCTION
GetTemporaryTableIndex
RETURN NUMBER
AS
  l_seq number;
BEGIN
  select SEQ_TemporaryTableIndex.nextval into l_seq from dual ;
  return l_seq;
commit;
END;
/
