CREATE TABLE Export_OperationJournal
(
  Id              NUMBER                        NOT NULL,
  UserId          NUMBER                        NOT NULL,
  ActionDateTime  DATE                          NOT NULL,
  SignedDocument  CLOB,
  OperationType   VARCHAR2(6)                   NOT NULL,
  ContentType     VARCHAR2(10)                  NOT NULL,
  BinaryBody      BLOB                          NOT NULL,
  ContentName     VARCHAR2(255)                 NOT NULL,
  JournalType     VARCHAR2(50)                  NOT NULL
);

ALTER TABLE AP.Export_OperationJournal ADD (
  CONSTRAINT Export_OperationJournal_PK
 PRIMARY KEY
 (Id));