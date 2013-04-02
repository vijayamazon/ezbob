CREATE OR REPLACE FUNCTION Application_DetailInsert
 (
  pApplicationId  IN NUMBER,
  pDetailNameID   IN NUMBER,
  pParentDetailId IN NUMBER,
  pValueStr       IN CLOB,
  pIsBinary       IN NUMBER
 ) return number
AS
 l_DetailNameID NUMBER;
 l_detailId Number;
 l_seq_dtil_name_id Number;
BEGIN
  Select seq_app_detail.nextval into l_detailId from dual;
  INSERT INTO Application_Detail
         (
          DetailId
         ,ApplicationId
         ,DetailNameID
         ,ParentDetailId
         ,ValueStr
         ,ValueNum
         ,ValueDateTime
         ,IsBinary)
     VALUES
         ( l_detailId
         , pApplicationId
         , pDetailNameID
         , pParentDetailId
         , pValueStr
         , null
         , null
         , pIsBinary);

   return l_detailId;
END Application_DetailInsert;
/