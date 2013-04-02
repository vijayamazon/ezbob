CREATE TABLE EmailAccount (
	 Id      NUMBER not null,
	 IsDeleted    NUMBER null,
	 Type     VARCHAR2(256) not null,
	 Name     VARCHAR2(1024) not null,
	 Description   VARCHAR2(1024) null,
	 EmailFrom    VARCHAR2(1024) null,
	 ServerAddress   VARCHAR2(1024) null,
	 SmtpServerAddress  VARCHAR2(1024) null,
	 Port     NUMBER NULL,
	 UserName   VARCHAR2 (1024) null,
	 Password   VARCHAR2 (1024) null,
	 EncryptionType  VARCHAR2 (64) null,
	 RequireAuthenication NUMBER null,
	 SIGNEDDOCUMENT       CLOB NULL,
	 SignedDocumentDelete CLOB NULL,
	 TERMINATIONDATE      DATE,
         CreatorUserId        NUMBER NULL,
         STARTDATE            DATE default sysdate
);

CREATE UNIQUE INDEX PK_EmailAccount ON EmailAccount
(
       ID                  ASC
);

ALTER TABLE EmailAccount ADD ( CONSTRAINT PK_EmailAccount PRIMARY KEY (ID) ) ;
