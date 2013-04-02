CREATE TABLE Log_Strategy (
       LogStrategyId        NUMBER NOT NULL,
       StrategyId           NUMBER NULL,
       Name                 VARCHAR2(500) NULL,
       Description          VARCHAR2(500) NULL,
       IsEmbeddingAllowed   NUMBER NULL,
       XML                  CLOB NULL,
       AuthorId             NUMBER NULL,
       State                SMALLINT NULL,
       SubState             SMALLINT NULL,
       CreationDate         DATE NULL
);

CREATE UNIQUE INDEX PK_Log_Strategy ON Log_Strategy
(
       LogStrategyId                  ASC
);


ALTER TABLE Log_Strategy
       ADD  ( CONSTRAINT PK_Log_Strategy PRIMARY KEY (LogStrategyId) ) ;


