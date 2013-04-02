create or replace procedure UpdateExportSigningId(
    pSignedDocumentId IN NUMBER,
    pId IN NUMBER
)
as
begin
   UPDATE Export_Results
   SET SignedDocumentId = pSignedDocumentId
   WHERE Id = pId;
end;
/
