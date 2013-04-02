CREATE OR REPLACE PROCEDURE Dump_Application_Variables
(
  pDumpId IN NUMBER,
  pName VARCHAR2,
  pValue CLOB,
  pType VARCHAR2,
  pDirection IN INTEGER
)
AS
l_id number;
BEGIN    

  begin
     Select id into l_id from Application_VariablesDumpData
  where DumpId = pDumpId
    AND Name = pName;
  exception 
    when others then 
      Select seq_app_VariablesDumpData.Nextval into l_id from dual;

      insert into Application_VariablesDumpData
        (ID, DumpId, NAME, VALUE, TYPE, DIRECTION)
      values
        (l_id, pDumpId, pName, pValue, pType, pDirection);
    return;
  end;
  UPDATE Application_VariablesDumpData
     SET Value = pValue
   WHERE
      DumpId = pDumpId AND
      Name  = pName;

END;
/
