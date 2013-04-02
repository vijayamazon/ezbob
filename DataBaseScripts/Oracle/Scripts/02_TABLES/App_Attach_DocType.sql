CREATE TABLE App_Attach_DocType
(
  AttachmentTypeId Number NOT NULL,
  AttachmentType varchar2(1024) NOT NULL,
  AttachmentGroup varchar2(1024) NULL
);

CREATE UNIQUE INDEX PK_App_AttachDocType ON App_Attach_DocType
(
  AttachmentTypeId ASC
);


ALTER TABLE App_Attach_DocType
       ADD  ( CONSTRAINT PK_App_AttachDType PRIMARY KEY 
               (AttachmentTypeId) 
            ) ;
