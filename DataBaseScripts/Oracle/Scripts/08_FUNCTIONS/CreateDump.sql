CREATE OR REPLACE FUNCTION CreateDump
(
   pApplicationId IN NUMBER,
   pName IN varchar2
) return number
AS
l_id number;
BEGIN

  begin
     SELECT id into l_id from Application_VariablesDump
     WHERE ApplicationId = pApplicationId
       AND Name = pName;
     UPDATE Application_VariablesDump
        SET LastUpdateDate = sysdate
      WHERE id = l_id;
  exception
    when others then
      Select seq_app_VariablesDump.Nextval into l_id from dual;

      INSERT INTO Application_VariablesDump
         ( Id
         , ApplicationId
         , Name
         , LastUpdateDate)
      VALUES
         ( l_id
         , pApplicationId
         , pName
         , sysdate);
  end;

  return l_id;
END;
/
