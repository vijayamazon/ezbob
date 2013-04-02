CREATE OR REPLACE Procedure DataSource_SaveResponse
(
	pReqId in number,
	pResponse in clob
)
AS
begin
  INSERT INTO DataSource_Responses
           (RequestId,Response)
     VALUES
           (pReqId, pResponse);
     
end;
/