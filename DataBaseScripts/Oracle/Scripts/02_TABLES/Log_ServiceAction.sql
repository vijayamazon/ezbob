CREATE TABLE Log_ServiceAction (
       LogServiceActionId   NUMBER NOT NULL,
       Command              VARCHAR2(255) NULL,
       ApplicationId        NUMBER NULL,
       Request              CLOB NULL,
       Response             CLOB NULL,
       DateTime             DATE NOT NULL,
       UserHost             VARCHAR2(255) NULL
);

COMMENT ON COLUMN Log_ServiceAction.LogServiceActionId IS 'id';
COMMENT ON COLUMN Log_ServiceAction.Command IS 'Service Action. Namespace and method name';
COMMENT ON COLUMN Log_ServiceAction.ApplicationId IS 'Application id';
COMMENT ON COLUMN Log_ServiceAction.Request IS 'Soap Request';
COMMENT ON COLUMN Log_ServiceAction.Response IS 'Soap Response';
COMMENT ON COLUMN Log_ServiceAction.UserHost IS 'User host name or Ip address';
CREATE UNIQUE INDEX PK_Log_ServiceAction ON Log_ServiceAction
(
       LogServiceActionId             ASC
);


ALTER TABLE Log_ServiceAction
       ADD  ( CONSTRAINT PK_Log_ServiceAction PRIMARY KEY (
              LogServiceActionId) ) ;


