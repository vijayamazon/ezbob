CREATE TABLE ExclusiveApplication
(
  ID             NUMBER                         NOT NULL,
  APPLICATIONID  NUMBER                         NOT NULL
);


ALTER TABLE ExclusiveApplication ADD (
  CONSTRAINT ExclusiveApplication_PK
 PRIMARY KEY
 (ID));
