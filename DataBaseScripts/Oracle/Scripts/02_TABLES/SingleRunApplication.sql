CREATE TABLE SingleRunApplication
(
	ID                          NUMBER NOT NULL,
	ApplicationExecutionTypeId  NUMBER NOT NULL
);

ALTER TABLE SingleRunApplication ADD (
  CONSTRAINT SingleRunApplication_PK
 PRIMARY KEY
 (ID));
