CREATE TABLE EntityLink (
       Id          NUMBER         NOT NULL,
       SeriaId     NUMBER         NOT NULL,
       EntityType  VARCHAR2(100)  NOT NULL,
       EntityId    NUMBER         NOT NULL,
       UserId      NUMBER         NOT NULL,
       LinksDoc    CLOB           NOT NULL,
       SignedDoc   CLOB               NULL,
       ISDELETED   NUMBER             NULL,
       IsApproved  NUMBER             NULL
);

CREATE UNIQUE INDEX PK_EntityLink ON EntityLink
(
       Id                  ASC
);


ALTER TABLE EntityLink
       ADD  ( CONSTRAINT PK_EntityLink PRIMARY KEY (
              Id) ) ;

