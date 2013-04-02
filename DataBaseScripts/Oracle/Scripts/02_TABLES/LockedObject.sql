CREATE TABLE LockedObject (
       Id             NUMBER         NOT NULL,
       Type           VARCHAR2(100)  NOT NULL,
       TimeoutPeriod  NUMBER         NOT NULL,
       LastUpdateTime DATE           NOT NULL
);

CREATE UNIQUE INDEX PK_LockedObject ON LockedObject
(
       Id                  ASC
);


ALTER TABLE LockedObject
       ADD  ( CONSTRAINT PK_LockedObject PRIMARY KEY (
              Id) ) ;

