CREATE OR REPLACE Procedure DataSource_SaveRequest
(
  pAppId in number,
  pRequest in CLOB,
  pReqId out number
)
AS
begin
  select seq_datasource_requests.nextval into pReqId from dual;
  INSERT INTO DataSource_Requests
           (RequestId, ApplicationId,Request)
     VALUES (pReqId, pAppId, pRequest);
     
end;
/