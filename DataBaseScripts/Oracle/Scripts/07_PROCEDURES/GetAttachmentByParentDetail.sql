create or replace Procedure GetAttachmentByParentDetail
(
      pDetailId in integer,
	   cur_OUT out sys_refcursor
)
AS
begin
  open cur_OUT for
	SELECT apat.value FROM Application_Attachment apat
	WHERE apat.DetailId = 
	(
		SELECT ad.DetailId
		FROM Application_Detail ad 
		INNER JOIN Application_DetailName dn ON ad.DetailNameId = dn.DetailNameId
		WHERE
			ad.ParentDetailId = pDetailId and
			dn.Name = 'FileBin'
	);
end;
/