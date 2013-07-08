IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertPayPointData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertPayPointData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE InsertPayPointData
@_class      VARCHAR(300),
@acquirer    VARCHAR(300),
@amount      DECIMAL(18, 2),
@auth_code   VARCHAR(300),
@authorised  VARCHAR(300),
@card_type   VARCHAR(300),
@cid         VARCHAR(300),
@company_no  VARCHAR(300),
@country     VARCHAR(300),
@currency    VARCHAR(300),
@cv2avs      VARCHAR(300),
@date        DATETIME,
@deferred    VARCHAR(300),
@emvValue    VARCHAR(300),
@fraud_code  VARCHAR(300),
@FraudScore  VARCHAR(300),
@ip          VARCHAR(300),
@lastfive    VARCHAR(300),
@merchant_no VARCHAR(300),
@message     VARCHAR(300),
@MessageType VARCHAR(300),
@mid         VARCHAR(300),
@name        VARCHAR(300),
@options     VARCHAR(300),
@status      VARCHAR(300),
@tid         VARCHAR(300),
@trans_id    VARCHAR(300)
AS
	INSERT INTO PayPointBalance (
		acquirer, amount, auth_code, authorised, card_type, cid,
		_class, company_no, country, currency, cv2avs, date,
		deferred, emvValue, fraud_code, FraudScore,
		ip, lastfive, merchant_no, message, MessageType, mid,
		name, options, status, tid, trans_id
	)
	VALUES (
		@acquirer, @amount, @auth_code, @authorised, @card_type, @cid,
		@_class, @company_no, @country, @currency, @cv2avs, @date,
		@deferred, @emvValue, @fraud_code, @FraudScore,
		@ip, @lastfive, @merchant_no, @message, @MessageType, @mid,
		@name, @options, @status, @tid, @trans_id
	)
GO
