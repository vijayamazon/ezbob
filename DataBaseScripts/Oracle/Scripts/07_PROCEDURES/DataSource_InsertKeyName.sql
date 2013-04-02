CREATE OR REPLACE Procedure DataSource_InsertKeyName
(
	 pKeyName in varchar2
)
AS
keyId number;
begin
  BEGIN
    SELECT KeyNameId INTO keyId FROM DataSource_KeyFields
      WHERE KeyName = pKeyName;
    EXCEPTION WHEN NO_DATA_FOUND THEN  
      INSERT INTO DataSource_KeyFields(Keynameid, KeyName)
         VALUES(seq_datasource_keyfields.nextval, pKeyName);
  END;
end;
/