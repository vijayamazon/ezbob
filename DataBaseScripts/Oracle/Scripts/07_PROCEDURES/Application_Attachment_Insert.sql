create or replace procedure Application_Attachment_Insert
(
  pValue IN BLOB,
  pDetailId IN NUMBER,
  pAttachmentId OUT NUMBER
)
as
 l_attach_id Number;
begin

   Select SEQ_app_attachment.Nextval into l_attach_id from dual;

   insert into Application_Attachment
     (AttachmentId, Value, DetailId)
   values
     (l_attach_id, pValue, pDetailId);

    pAttachmentId := l_attach_id;
end;
/