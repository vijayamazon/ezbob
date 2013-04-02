CREATE TABLE Application_ExecutionType (
       Id               NUMBER NOT NULL,
       ApplicationId    NUMBER NOT NULL,
       ItemId           NUMBER NOT NULL,
       ExecutionType    SMALLINT NULL
);

CREATE UNIQUE INDEX PK_Application_ExecutionType ON Application_ExecutionType
(
       Id                  ASC
);


ALTER TABLE Application_ExecutionType
       ADD  ( CONSTRAINT PK_Application_ExecutionType PRIMARY KEY (
              Id) ) ;

