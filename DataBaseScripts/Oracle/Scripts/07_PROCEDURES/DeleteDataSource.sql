CREATE OR REPLACE PROCEDURE DeleteDataSource
(
 pDataSourceId IN number,
 pSignedData in clob
)
AS
BEGIN

  UPDATE DataSource_Sources
  SET 
     IsDeleted = Id,
     SignedDocumentDelete = pSignedData
  WHERE DisplayName = (SELECT DisplayName FROM
  DataSource_Sources WHERE  Id = pDataSourceId);

END;
/

