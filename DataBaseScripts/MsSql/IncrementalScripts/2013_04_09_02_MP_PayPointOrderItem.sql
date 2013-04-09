IF OBJECT_ID ('dbo.MP_PayPointOrderItem') IS NOT NULL
BEGIN
	PRINT 'MP_PayPointOrderItem exists'	
	DROP TABLE MP_PayPointOrderItem
END

CREATE TABLE dbo.MP_PayPointOrderItem
(
	Id                 INT IDENTITY NOT NULL
  , OrderId            INT
  , acquirer           VARCHAR(300)
  , amount             DECIMAL
  , auth_code          VARCHAR(300)
  , authorised         VARCHAR(300)
  , card_type          VARCHAR(300)
  , cid                VARCHAR(300)
  , classType          VARCHAR(300)
  , company_no         VARCHAR(300)
  , country            VARCHAR(300)
  , currency           VARCHAR(300)
  , cv2avs             VARCHAR(300)
  , date               DATETIME -- (Mon, 8 Apr 2013 07:57:08)
  , deferred           VARCHAR(300)
  , emvValue           VARCHAR(300)
  , ExpiryDate         DATETIME -- (06/15)
  , fraud_code         VARCHAR(300)
  , FraudScore         VARCHAR(300)
  , ip                 VARCHAR(300)
  , lastfive           VARCHAR(300)
  , merchant_no        VARCHAR(300)
  , message            VARCHAR(300)
  , MessageType        VARCHAR(300)
  , mid                VARCHAR(300)
  , name               VARCHAR(300)
  , options            VARCHAR(300)
  , start_date         DATETIME
  , status             VARCHAR(300)
  , tid                VARCHAR(300)
  , trans_id           VARCHAR(300)	  
  , CONSTRAINT PK_MP_PayPointOrderItem PRIMARY KEY (Id)
  , CONSTRAINT FK_MP_PayPointOrderItem_MP_PayPointOrder FOREIGN KEY (OrderId) REFERENCES dbo.MP_PayPointOrder (Id)
)
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
