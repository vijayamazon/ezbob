create table DICTIONARYPARAMS
(
  ID                 NUMBER not null,
  DICTIONARYID       NUMBER not null,
  FIELDNAME          VARCHAR2(30) not null,
  DISPLAYNAME        VARCHAR2(255) not null,
  FIELDTYPE          VARCHAR2(30) not null,
  ISDELETED          VARCHAR2(1),
  MASTERDICTIONARYID NUMBER,
  DEFVALUE           VARCHAR2(100)
);
