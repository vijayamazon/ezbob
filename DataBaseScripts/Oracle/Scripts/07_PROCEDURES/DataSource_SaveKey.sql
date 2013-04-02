CREATE OR REPLACE Procedure DataSource_SaveKey
(
  pRequestId in number,
	pKeyNameId in number,
	pKeyValue in CLOB
)
AS
begin
  INSERT INTO DataSource_KeyData
           (KeyValueId,RequestId,KeyNameId,Value)
     VALUES
           (seq_datasource_keydata.nextval,pRequestId, pKeyNameId, pKeyValue);
end;
/