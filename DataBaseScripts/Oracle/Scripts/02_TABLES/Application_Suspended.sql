CREATE TABLE Application_Suspended (
       ApplicationId              NUMBER                         NOT NULL,
       ExecutionState             CLOB                           NOT NULL,
       Postfix                    VARCHAR2(1000 BYTE),
       Target                     VARCHAR2(50)                   NOT NULL,
       Label                      VARCHAR2(250)                  NOT NULL,
       Message                    BLOB                           NOT NULL,
       AppSpecific                NUMBER                         NULL,
       "Date"                     DATE DEFAULT CURRENT_TIMESTAMP NOT NULL,
       ExecutionType              SMALLINT                       NULL,
       ExecutionPathCurrentItemId NUMBER                         NOT NULL
);

CREATE UNIQUE INDEX PK_Application_Suspended ON Application_Suspended
(
       ApplicationId                  ASC
);


ALTER TABLE Application_Suspended
       ADD  ( CONSTRAINT PK_Application_Suspended PRIMARY KEY (
              ApplicationId) ) ;

