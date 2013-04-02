CREATE TABLE APPLICATION_RESULT
(
  ID             NUMBER                         NOT NULL,
  APPLICATIONID  NUMBER                         NOT NULL,
  NAME           VARCHAR2(255)                  NOT NULL,
  VALUE          CLOB,
  TYPE           VARCHAR2(20)                   NOT NULL,
  DIRECTION      INTEGER                        NOT NULL
);


ALTER TABLE APPLICATION_RESULT ADD (
  CONSTRAINT APPLICATION_RESULT_PK
 PRIMARY KEY
 (ID));
