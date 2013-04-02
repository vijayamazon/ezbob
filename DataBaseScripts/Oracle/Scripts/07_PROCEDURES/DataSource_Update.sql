CREATE OR REPLACE Procedure DataSource_Update
(
  pId in number,
  pDescription in varchar2,
  pUserId in number,
  pSignedData in clob,
  pDocument in clob
)
AS
begin
    UPDATE DataSource_Sources
    SET 
      Description = pDescription,
      UserId = pUserId,
      Document = pDocument,
      SIGNEDDOCUMENT = pSignedData
    WHERE Id = pId;
end;
/