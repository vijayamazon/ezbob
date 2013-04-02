CREATE TABLE Strategy_ScheduleParam (
       Id                   NUMBER NOT NULL,
       StrategyScheduleId   NUMBER NOT NULL,
       CurrentVersionId     NUMBER NULL,
       Name                 VARCHAR2(100) NOT NULL,
       Description          CLOB NULL,
       Data                 CLOB NOT NULL,
       Deleted              NUMBER NULL,
       UserId               NUMBER NULL,
       CreationDate         DATE DEFAULT CURRENT_TIMESTAMP NOT NULL,
       SignedDocument       CLOB NULL
);

CREATE UNIQUE INDEX PK_Strategy_ScheduleParam ON Strategy_ScheduleParam
(
       Id                    ASC
);


ALTER TABLE Strategy_ScheduleParam
       ADD  ( CONSTRAINT PK_Strategy_ScheduleParam PRIMARY KEY (
              Id) ) ;


