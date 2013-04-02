CREATE OR REPLACE Procedure VerifyServiceRegistration
(
  pKey IN VARCHAR2,
  pResult OUT VARCHAR2
) 
AS
 regKey VARCHAR2(255);
 l_ServiceRegistration_id Number;
BEGIN
  begin
    select "KEY" 
    into regKey
    from ServiceRegistration;
    if regKey = pKey then
      pResult := null;
    else
      pResult := regKey;
    end if;
  exception
    when no_data_found
    then
     begin
      Select SEQ_ServiceRegistration.nextval into l_ServiceRegistration_id from dual;
      INSERT INTO ServiceRegistration
        (Id
         ,"KEY")
      VALUES
        (l_ServiceRegistration_id
         ,pKey);
      pResult := null;
     end;
  end;

END;
/
