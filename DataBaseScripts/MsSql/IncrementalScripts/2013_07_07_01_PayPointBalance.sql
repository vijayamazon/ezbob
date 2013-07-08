DELETE FROM
	PacnetAgentConfigs
WHERE
	CfgKey LIKE 'PayPoint%'
GO

INSERT INTO PacnetAgentConfigs (CfgKey, CfgValue) VALUES ('PayPointMid', 'orange06')
INSERT INTO PacnetAgentConfigs (CfgKey, CfgValue) VALUES ('PayPointVpnPassword', 'ezbob2012')
INSERT INTO PacnetAgentConfigs (CfgKey, CfgValue) VALUES ('PayPointRemotePassword', 'ezbob2012')
GO

IF OBJECT_ID ('dbo.PayPointBalance') IS NULL
BEGIN
	CREATE TABLE dbo.PayPointBalance (
		Id                 INT IDENTITY NOT NULL
	,	acquirer           VARCHAR(300)
	,	amount             DECIMAL(18, 2)
	,	auth_code          VARCHAR(300)
	,	authorised         VARCHAR(300)
	,	card_type          VARCHAR(300)
	,	cid                VARCHAR(300)
	,	_class             VARCHAR(300)
	,	company_no         VARCHAR(300)
	,	country            VARCHAR(300)
	,	currency           VARCHAR(300)
	,	cv2avs             VARCHAR(300)
	,	date               DATETIME -- (Mon, 8 Apr 2013 07:57:08)
	,	deferred           VARCHAR(300)
	,	emvValue           VARCHAR(300)
	,	fraud_code         VARCHAR(300)
	,	FraudScore         VARCHAR(300)
	,	ip                 VARCHAR(300)
	,	lastfive           VARCHAR(300)
	,	merchant_no        VARCHAR(300)
	,	message            VARCHAR(300)
	,	MessageType        VARCHAR(300)
	,	mid                VARCHAR(300)
	,	name               VARCHAR(300)
	,	options            VARCHAR(300)
	,	status             VARCHAR(300)
	,	tid                VARCHAR(300)
	,	trans_id           VARCHAR(300)
	,	CONSTRAINT PK_PayPointBalance PRIMARY KEY (Id)
	)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanTransaction') AND name = 'Reconciliation')
	ALTER TABLE LoanTransaction ADD Reconciliation VARCHAR(10) NOT NULL CONSTRAINT DF_LoanTran_Recon DEFAULT 'not tested'
GO

IF OBJECT_ID('DeleteOldPayPointBalanceData') IS NOT NULL
	DROP PROC DeleteOldPayPointBalanceData
GO

CREATE PROCEDURE DeleteOldPayPointBalanceData
@Date DATE
AS
BEGIN
	DELETE FROM
		PayPointBalance
	WHERE
		CONVERT(DATE, date) = @Date
	
	UPDATE LoanTransaction SET
		Reconciliation = 'not tested'
	WHERE
		CONVERT(DATE, PostDate) = @Date
		AND
		Type = 'PaypointTransaction'
END
GO

IF OBJECT_ID('InsertPayPointData') IS NOT NULL
	DROP PROC InsertPayPointData
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

IF OBJECT_ID('LoadPayPointBalanceColumns') IS NOT NULL
	DROP PROCEDURE LoadPayPointBalanceColumns
GO

CREATE PROCEDURE LoadPayPointBalanceColumns
AS
	SELECT
		name
	FROM
		syscolumns
	WHERE
		id = OBJECT_ID('PayPointBalance')
		AND
		name != 'Id'
	ORDER BY
		name
GO

