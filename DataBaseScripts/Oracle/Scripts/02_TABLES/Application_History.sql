CREATE TABLE Application_History (
       AppHistoryId         NUMBER NOT NULL,
       UserId               NUMBER NULL,
       SecurityApplicationId NUMBER NULL,
       ActionDateTime       DATE NULL,
       ActionType           NUMBER NULL,
       ApplicationId        NUMBER NULL,
       CurrentNodeID        NUMBER
);

CREATE UNIQUE INDEX XPKApplication_History ON Application_History
(
       AppHistoryId                   ASC
);

CREATE INDEX XIF1Application_History ON Application_History
(
       UserId                         ASC
);

CREATE INDEX XIF2Application_History ON Application_History
(
       SecurityApplicationId          ASC
);

CREATE INDEX XIF3Application_History ON Application_History
(
       ApplicationId                  ASC
);


ALTER TABLE Application_History
       ADD  ( CONSTRAINT XPKApplication_History PRIMARY KEY (
              AppHistoryId) ) ;


comment on column APPLICATION_HISTORY.ACTIONTYPE
  is '0 - Lock, 1 - UnLock; Coming time -3; ExitTime -4';                              