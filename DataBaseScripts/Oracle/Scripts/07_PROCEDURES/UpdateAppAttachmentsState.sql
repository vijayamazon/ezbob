create or replace procedure UpdateAppAttachmentsState(pApplicationId IN NUMBER)
as
begin
  UPDATE Application_Detail
  SET  ValueStr = 'Saved'
  WHERE Application_Detail.DetailId IN (
  SELECT Application_Detail_1.DetailId
  FROM Application_DetailName INNER JOIN
       Application_Detail Application_Detail_1 ON Application_DetailName.DetailNameId = Application_Detail_1.DetailNameId INNER JOIN
       Application_Attachment INNER JOIN
       Application_Detail ON Application_Attachment.DetailId = Application_Detail.DetailId ON 
       Application_Detail_1.ParentDetailId = Application_Detail.ParentDetailId
  WHERE Application_DetailName.Name = 'State'
    AND dbms_lob.SUBSTR(Application_Detail_1.ValueStr, 3, 1) = 'New'
    AND Application_Detail_1.ApplicationId = pApplicationId
    AND Application_Detail.ApplicationId = pApplicationId
    );
end;
/
