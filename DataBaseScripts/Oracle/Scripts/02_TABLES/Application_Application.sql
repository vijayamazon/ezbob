CREATE TABLE Application_Application (
       ApplicationId        NUMBER NOT NULL,
       CreationDate         DATE DEFAULT CURRENT_TIMESTAMP NOT NULL,
       CreatorUserId        NUMBER NOT NULL,
       StrategyId           NUMBER NOT NULL,
       LockedByUserId       NUMBER NULL,
       State                NUMBER DEFAULT 0 NOT NULL,
       LastUpdateDate       DATE NULL,
       Version              NUMBER DEFAULT 0 NOT NULL,
       IsTimeLimitExceeded  SMALLINT DEFAULT 0 NOT NULL,
       ErrorMsg             VARCHAR2(4000) NULL,
       Param1               VARCHAR2(512) NULL,
       Param2               VARCHAR2(512) NULL,
       Param3               VARCHAR2(512) NULL,
       Param4               VARCHAR2(512) NULL,
       Param5               VARCHAR2(512) NULL,
       Param6               VARCHAR2(512) NULL,
       Param7               VARCHAR2(512) NULL,
       Param8               VARCHAR2(512) NULL,
       Param9               VARCHAR2(512) NULL,
       PARENTAPPID          NUMBER NULL,
       CHILDCOUNT           NUMBER NULL,
       APPCOUNTER           NUMBER NULL,
       ExecutionPath        CLOB NULL,
       ExecutionPathBin     BLOB NULL
);

COMMENT ON TABLE Application_Application IS 'Each row represents a customer application.';
COMMENT ON COLUMN Application_Application.ApplicationId IS 'Primary Key';
COMMENT ON COLUMN Application_Application.CreationDate IS 'Application creation date';
COMMENT ON COLUMN Application_Application.CreatorUserId IS 'User identifier who create an application';
COMMENT ON COLUMN Application_Application.StrategyId IS 'Application strategy identifier';
COMMENT ON COLUMN Application_Application.LockedByUserId IS 'Who was locked the application';
COMMENT ON COLUMN Application_Application.State IS 'Application State. 0-need process by SE; 1-need process by node. 2-strategy has been finished without errors; 3-strategy has been finished with errors.';
COMMENT ON COLUMN Application_Application.LastUpdateDate IS 'When changes have been applied';
COMMENT ON COLUMN Application_Application.IsTimeLimitExceeded IS 'Whether time limit exceeded or not';
COMMENT ON COLUMN Application_Application.ErrorMsg IS 'Error message';
COMMENT ON COLUMN Application_Application.Param1 IS 'Custom parameter 1';
COMMENT ON COLUMN Application_Application.Param2 IS 'Custom parameter 2';
COMMENT ON COLUMN Application_Application.Param3 IS 'Custom parameter 3';
COMMENT ON COLUMN Application_Application.Param4 IS 'Custom parameter 4';
COMMENT ON COLUMN Application_Application.Param5 IS 'Custom parameter 5';
COMMENT ON COLUMN Application_Application.Param6 IS 'Custom parameter 6';
COMMENT ON COLUMN Application_Application.Param7 IS 'Custom parameter 7';
COMMENT ON COLUMN Application_Application.Param8 IS 'Custom parameter 8';
COMMENT ON COLUMN Application_Application.Param9 IS 'Custom parameter 9';
COMMENT ON COLUMN APPLICATION_APPLICATION.PARENTAPPID IS 'Parent application. If null, no parent.';
COMMENT ON COLUMN APPLICATION_APPLICATION.CHILDCOUNT IS 'Number of child applications. If null or 0, no child appplications.';
comment on column APPLICATION_APPLICATION.APPCOUNTER IS 'Application real counter';
CREATE UNIQUE INDEX PK_Application_Application ON Application_Application
(
       ApplicationId                  ASC
);


ALTER TABLE Application_Application
       ADD  ( CONSTRAINT PK_Application_Application PRIMARY KEY (
              ApplicationId) ) ;

