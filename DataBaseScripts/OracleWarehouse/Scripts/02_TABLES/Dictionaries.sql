create table Dictionaries
(
  ID           NUMBER not null,
  DISPLAYNAME  VARCHAR2(255) not null,
  TABLENAME    VARCHAR2(30) not null,
  DESCRIPTION  VARCHAR2(1024),
  CREATIONTIME DATE,
  USERID       NUMBER,
  ISDELETED    VARCHAR2(1)
);
