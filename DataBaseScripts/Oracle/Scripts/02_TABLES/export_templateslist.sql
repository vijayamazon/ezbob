-- Create table
create table EXPORT_TEMPLATESLIST
(
  ID           		NUMBER not null,
  FILENAME     		VARCHAR2(2086),
  DESCRIPTION  		VARCHAR2(1024),
  VARIABLESXML 		CLOB,
  UPLOADDATE   		DATE default sysdate,
  ISDELETED    		NUMBER,
  BINARYBODY    	BLOB,
  EXCEPTIONTYPE		NUMBER,
  USERID        	NUMBER,
  DeleterUserId 	NUMBER NULL,
  DISPLAYNAME		VARCHAR2(2800),
  TERMINATIONDATE	DATE,
  SIGNEDDOCUMENT	CLOB NULL,
  DELSIGNEDDOCUMENT CLOB NULL
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table EXPORT_TEMPLATESLIST
  add constraint PK_TEMPLATEID primary key (ID);
 
alter table EXPORT_TEMPLATESLIST
  add constraint IX_TEMPLATENAME unique (FILENAME, ISDELETED);
  
