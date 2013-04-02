CREATE TABLE Application_VariablesDump
(
  ID             NUMBER                         NOT NULL,
  APPLICATIONID  NUMBER                         NOT NULL,
  NAME           VARCHAR2(255),
  LastUpdateDate DATE NULL
);


ALTER TABLE Application_VariablesDump ADD (
  CONSTRAINT Application_VariablesDump_PK
 PRIMARY KEY
 (ID));
