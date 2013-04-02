CREATE TABLE Security_Session (
       UserId               NUMBER NOT NULL,
       AppId                NUMBER NOT NULL,
       State                SMALLINT DEFAULT 0 NOT NULL,
       SessionId            VARCHAR2(32) NOT NULL,
       CreationDate         DATE DEFAULT CURRENT_TIMESTAMP NOT NULL,
       LastAccessTime       DATE DEFAULT CURRENT_TIMESTAMP NOT NULL,
	   HostAddress			VARCHAR2(1024) NULL
);

COMMENT ON COLUMN Security_Session.UserId IS 'user id';
COMMENT ON COLUMN Security_Session.AppId IS 'Application id';
COMMENT ON COLUMN Security_Session.State IS '0-Session Is inactive; 1-Session is Active';
COMMENT ON COLUMN Security_Session.SessionId IS 'User session id';
COMMENT ON COLUMN Security_Session.CreationDate IS 'Session creation date';
CREATE UNIQUE INDEX PK_Security_Session ON Security_Session
(
       SessionId                         ASC
);

