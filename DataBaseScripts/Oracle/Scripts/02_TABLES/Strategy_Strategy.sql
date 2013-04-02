CREATE TABLE Strategy_Strategy (
       StrategyId             NUMBER NOT NULL,
       CurrentVersionId       NUMBER NULL,
       Name                   VARCHAR2(500) NOT NULL,
       Description            VARCHAR2(500) NULL,
       IsEmbeddingAllowed     SMALLINT NOT NULL,
       XML                    CLOB NOT NULL,
       IsDeleted              NUMBER DEFAULT 0 NOT NULL,
       UserId                 NUMBER NULL,
       AuthorId               NUMBER NOT NULL,
       State                  SMALLINT DEFAULT 0 NOT NULL,
       SubState               SMALLINT DEFAULT 1 NOT NULL,
       CreationDate           DATE DEFAULT CURRENT_TIMESTAMP NOT NULL,
       Icon                   BLOB NULL,
       ExecutionDuration      NUMBER NULL,
       LASTUPDATEDATE         DATE default CURRENT_TIMESTAMP NULL,
       StrategyType           VARCHAR2(255) NOT NULL,
       DISPLAYNAME            VARCHAR2(2048) NULL,
       TERMDATE               DATE NULL,
       SIGNEDDOCUMENT         CLOB NULL,
       IsMigrationSupported   NUMBER DEFAULT 0,
       SignedDocumentDelete   CLOB NULL,
       InDbFormat             BLOB NULL
);

COMMENT ON COLUMN Strategy_Strategy.StrategyId IS 'Strategy id';
COMMENT ON COLUMN Strategy_Strategy.CurrentVersionId IS 'version id';
COMMENT ON COLUMN Strategy_Strategy.Name IS 'name';
COMMENT ON COLUMN Strategy_Strategy.Description IS 'description';
COMMENT ON COLUMN Strategy_Strategy.IsEmbeddingAllowed IS 'is embedding allowed';
COMMENT ON COLUMN Strategy_Strategy.XML IS 'Strategy body';
COMMENT ON COLUMN Strategy_Strategy.IsDeleted IS '0-Active; >0 Deleted';
COMMENT ON COLUMN Strategy_Strategy.UserId IS 'Who modified the strategy last';
COMMENT ON COLUMN Strategy_Strategy.AuthorId IS 'Author';
COMMENT ON COLUMN Strategy_Strategy.State IS '0-Challange; 1-Champion';
COMMENT ON COLUMN Strategy_Strategy.SubState IS '0-Locked (Challenge); 1-Unlocked (Challenge); 2-Published (Champion); 3 - Suspended (Champion)';
COMMENT ON COLUMN Strategy_Strategy.CreationDate IS 'Creation date';
COMMENT ON COLUMN Strategy_Strategy.Icon IS 'Strategy Icon';
COMMENT ON COLUMN Strategy_Strategy.ExecutionDuration IS 'Strategy execute duration (TimeSpan)';
comment on column Strategy_Strategy.LASTUPDATEDATE is 'Last strategy update date';
comment on column Strategy_Strategy.StrategyType is 'Currently: Behavioral flag. 0 - Application strategy(default), 1 - Behavioral';

CREATE UNIQUE INDEX PK_Strategy_Strategy ON Strategy_Strategy
(
       StrategyId                     ASC
);


ALTER TABLE Strategy_Strategy
       ADD  ( CONSTRAINT PK_Strategy_Strategy PRIMARY KEY (StrategyId) ) ;