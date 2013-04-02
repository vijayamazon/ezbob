CREATE TABLE Signal
(
  Id                  NUMBER                         NOT NULL,
  Target              VARCHAR2(50)                   NOT NULL,
  label               VARCHAR2(250)                  NOT NULL,
  Status              NUMBER                         NOT NULL,
  StartTime           DATE DEFAULT CURRENT_TIMESTAMP NOT NULL,
  AppSpecific         NUMBER                         NOT NULL,
  ApplicationId       NUMBER                         NOT NULL,
  Priority            NUMBER                         NULL,
  ExecutionType       SMALLINT                       NULL,
  Message             BLOB                           NOT NULL,
  OwnerApplicationId  NUMBER                         NULL,
  IsExternal          NUMBER                         NULL
);

ALTER TABLE Signal 
       ADD ( CONSTRAINT PK_Signal PRIMARY KEY
              (Id)
           );

create index I_Signal_IsExternal on Signal (ISEXTERNAL) ;
