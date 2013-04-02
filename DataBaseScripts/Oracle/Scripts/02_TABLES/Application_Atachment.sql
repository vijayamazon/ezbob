CREATE TABLE Application_Attachment (
       AttachmentId         NUMBER NOT NULL,
       DetailId             NUMBER NULL,
       Value                BLOB NOT NULL
);

COMMENT ON COLUMN Application_Attachment.AttachmentId IS 'Primary Key';
COMMENT ON COLUMN Application_Attachment.DetailId IS 'Reference to Application_Detail record';
COMMENT ON COLUMN Application_Attachment.Value IS 'Binary value';
CREATE UNIQUE INDEX PK_Application_Attachment ON Application_Attachment
(
       AttachmentId                   ASC
);


ALTER TABLE Application_Attachment
       ADD  ( CONSTRAINT PK_Application_Attachment PRIMARY KEY (
              AttachmentId) ) ;


create index i_app_attach_1 on application_attachment (detailid) ;