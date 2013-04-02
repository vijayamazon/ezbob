CREATE TABLE AppStatus (
       Id 			NUMBER NOT NULL,
       Name 		VARCHAR2(128),
       Description 	VARCHAR2(1024)
);

alter table APPSTATUS
  add constraint PK_APPSTATUS primary key (ID);