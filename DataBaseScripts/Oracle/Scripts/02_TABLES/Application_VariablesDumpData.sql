CREATE TABLE Application_VariablesDumpData
(
  ID             NUMBER                         NOT NULL,
  DUMPID         NUMBER                         NOT NULL,
  NAME           VARCHAR2(255)                  NOT NULL,
  VALUE          CLOB,
  TYPE           VARCHAR2(20)                   NOT NULL,
  DIRECTION      INTEGER                        NOT NULL
);


ALTER TABLE Application_VariablesDumpData ADD (
  CONSTRAINT App_VariablesDumpData_PK
 PRIMARY KEY
 (ID));
