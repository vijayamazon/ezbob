IF OBJECT_ID('LoyaltyProgramActionTypes') IS NULL
BEGIN
	CREATE TABLE LoyaltyProgramActionTypes (
		ActionTypeID INT NOT NULL,
		ActionTypeName NVARCHAR(256) NOT NULL,
		CONSTRAINT PK_LoyaltyProgramActionTypes PRIMARY KEY (ActionTypeID),
		CONSTRAINT UNQ_LoyaltyProgramActionTypes UNIQUE (ActionTypeName),
		CONSTRAINT CHK_LoyaltyProgramActionTypes CHECK (ActionTypeID > 0 AND LTRIM(RTRIM(ActionTypeName)) != '')
	)

	INSERT INTO LoyaltyProgramActionTypes VALUES (1, 'Absolute number of points')
	INSERT INTO LoyaltyProgramActionTypes VALUES (2, 'Absolute number of points if parameter is positive')
	INSERT INTO LoyaltyProgramActionTypes VALUES (3, 'Multiplier for parameter')

	CREATE TABLE LoyaltyProgramActions (
		ActionID INT NOT NULL,
		ActionName NVARCHAR(20) NOT NULL,
		ActionDescription NVARCHAR(256) NOT NULL,
		Cost INT NOT NULL,
		ActionTypeID INT NOT NULL,
		CONSTRAINT PK_LoyaltyProgramActions PRIMARY KEY (ActionID),
		CONSTRAINT FK_LoyaltyProgramActionType FOREIGN KEY (ActionTypeID) REFERENCES LoyaltyProgramActionTypes (ActionTypeID),
		CONSTRAINT UNQ_LoyaltyProgramActionName UNIQUE (ActionName),
		CONSTRAINT CHK_LoyaltyProgramActions CHECK (ActionID > 0 AND LTRIM(RTRIM(ActionName)) != '' AND LTRIM(RTRIM(ActionDescription)) != '' AND Cost >= 0)
	)

	INSERT INTO LoyaltyProgramActions VALUES (1, 'SIGNUP',         'Customer: sign up',               500, 1)
	INSERT INTO LoyaltyProgramActions VALUES (2, 'LINKACCOUNT',    'Customer: link account',         1000, 1)
	INSERT INTO LoyaltyProgramActions VALUES (3, 'ACCOUNTCHECKED', 'Linked account checked',         4000, 2)
	INSERT INTO LoyaltyProgramActions VALUES (4, 'PERSONALINFO',   'Customer: personal info added', 10000, 1)
	INSERT INTO LoyaltyProgramActions VALUES (5, 'LOAN',           'Customer: loan taken',              1, 3)
	INSERT INTO LoyaltyProgramActions VALUES (6, 'REPAYMENT',      'Customer: on-time repayment',       1, 3)

	CREATE TABLE CustomerLoyaltyProgram (
		ID BIGINT NOT NULL IDENTITY,
		CustomerID INT NOT NULL,
		CustomerMarketPlaceID INT NULL,
		LoanID INT NULL,
		LoanScheduleID INT NULL,
		ActionID INT NOT NULL,
		ActionDate DATETIME NOT NULL CONSTRAINT DF_CustomerLoyaltyProgramDate DEFAULT GETDATE(),
		EarnedPoints NUMERIC(29, 0) NOT NULL,
		CONSTRAINT PK_CustomerLoyaltyProgram PRIMARY KEY (ID),
		CONSTRAINT FK_CustomerLoyaltyProgram FOREIGN KEY (CustomerID) REFERENCES Customer (Id),
		CONSTRAINT FK_CustomerLoyaltyProgramMP FOREIGN KEY (CustomerMarketPlaceID) REFERENCES MP_CustomerMarketPlace (Id),
		CONSTRAINT FK_CustomerLoyaltyProgramAction FOREIGN KEY (ActionID) REFERENCES LoyaltyProgramActions (ActionID),
		CONSTRAINT CHK_CustomerLoyaltyProgram CHECK (EarnedPoints >= 0)
	)
END
GO

IF OBJECT_ID('TR_CustomerSignupLoyalty') IS NOT NULL
	DROP TRIGGER TR_CustomerSignupLoyalty
GO

CREATE TRIGGER TR_CustomerSignupLoyalty
ON Customer
FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, ActionID, EarnedPoints)
	SELECT
		c.Id,
		a.ActionID,
		a.Cost
	FROM
		inserted c
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'SIGNUP'

	SET NOCOUNT OFF
END
GO

IF OBJECT_ID('TR_CustomerLinkAccountLoyalty') IS NOT NULL
	DROP TRIGGER TR_CustomerLinkAccountLoyalty
GO

CREATE TRIGGER TR_CustomerLinkAccountLoyalty
ON MP_CustomerMarketPlace
FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, CustomerMarketPlaceID, ActionID, EarnedPoints)
	SELECT
		c.CustomerId,
		c.Id,
		a.ActionID,
		a.Cost
	FROM
		inserted c
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'LINKACCOUNT'

	SET NOCOUNT OFF
END
GO

IF OBJECT_ID('TR_CustomerPersonalInfoLoyalty') IS NOT NULL
	DROP TRIGGER TR_CustomerPersonalInfoLoyalty
GO

CREATE TRIGGER TR_CustomerPersonalInfoLoyalty
ON Customer
FOR UPDATE
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, ActionID, EarnedPoints)
	SELECT
		c.Id,
		a.ActionID,
		a.Cost
	FROM
		inserted c
		INNER JOIN deleted d ON c.Id = d.Id AND c.IsSuccessfullyRegistered = 1 AND d.IsSuccessfullyRegistered = 0
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'PERSONALINFO'

	SET NOCOUNT OFF
END
GO

IF OBJECT_ID('CustomerLoyaltyProgramPoints') IS NOT NULL
	DROP VIEW CustomerLoyaltyProgramPoints
GO

CREATE VIEW CustomerLoyaltyProgramPoints
AS
SELECT
	CustomerID,
	SUM(EarnedPoints) AS EarnedPoints,
	MAX(ActionDate) AS LastActionDate
FROM
	CustomerLoyaltyProgram
GROUP BY
	CustomerID
GO

IF OBJECT_ID('LoyaltyProgramCheckedAccounts') IS NOT NULL
	DROP VIEW LoyaltyProgramCheckedAccounts
GO

CREATE VIEW LoyaltyProgramCheckedAccounts
AS
SELECT
	clp.CustomerMarketPlaceID
FROM
	CustomerLoyaltyProgram clp
	INNER JOIN LoyaltyProgramActions a ON clp.ActionID = a.ActionID
WHERE
	a.ActionName = 'ACCOUNTCHECKED'
GO

IF OBJECT_ID('TR_AccountCheckedLoyalty') IS NOT NULL
	DROP TRIGGER TR_AccountCheckedLoyalty
GO

CREATE TRIGGER TR_AccountCheckedLoyalty
ON MP_AnalyisisFunctionValues
FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	SELECT
		v.CustomerMarketPlaceId,
		MAX(
			(((((
				CAST(v.CountMonths AS BIGINT) * 10000 + CAST(YEAR(v.Updated) AS BIGINT)
			) * 100 + CAST(MONTH(v.Updated) AS BIGINT)
			) * 100 + CAST(DAY(v.Updated) AS BIGINT)
			) * 100 + CAST(DATEPART(hour, v.Updated) AS BIGINT)
			) * 100 + CAST(DATEPART(minute, v.Updated) AS BIGINT)
			) * 100 + CAST(DATEPART(second, v.Updated) AS BIGINT)
		) AS MaxMeasureCode
	INTO
		#m
	FROM
		inserted v
		INNER JOIN MP_AnalyisisFunction f ON f.Id = v.AnalyisisFunctionId AND f.Name = 'TotalSumOfOrders'
		LEFT JOIN LoyaltyProgramCheckedAccounts lp
			ON v.CustomerMarketPlaceId = lp.CustomerMarketPlaceID
	WHERE
		v.ValueFloat > 0
		AND
		lp.CustomerMarketPlaceID IS NULL
	GROUP BY
		v.CustomerMarketPlaceId

	INSERT INTO CustomerLoyaltyProgram (CustomerID, CustomerMarketPlaceID, ActionID, EarnedPoints)
	SELECT
		mp.CustomerId,
		#m.CustomerMarketPlaceId,
		a.ActionID,
		a.Cost
	FROM
		#m
		INNER JOIN MP_CustomerMarketPlace mp ON #m.CustomerMarketPlaceId = mp.Id
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'ACCOUNTCHECKED'
	
	DROP TABLE #m
	
	SET NOCOUNT OFF
END
GO

IF OBJECT_ID('TR_LoanTakenLoyalty') IS NOT NULL
	DROP TRIGGER TR_LoanTakenLoyalty
GO

CREATE TRIGGER TR_LoanTakenLoyalty
ON Loan
FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, LoanID, ActionID, EarnedPoints)
	SELECT
		CustomerId,
		Id,
		a.ActionID,
		a.Cost * l.LoanAmount
	FROM
		inserted l
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'LOAN'
	
	SET NOCOUNT OFF
END
GO
	
IF OBJECT_ID('TR_RepaymentLoyalty') IS NOT NULL
	DROP TRIGGER TR_RepaymentLoyalty
GO

CREATE TRIGGER TR_RepaymentLoyalty
ON LoanSchedule
FOR UPDATE
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, LoanID, LoanScheduleID, ActionID, EarnedPoints)
	SELECT
		l.CustomerId,
		l.Id,
		i.Id,
		a.ActionID,
		a.Cost * CAST(d.LoanRepayment - i.LoanRepayment AS NUMERIC(29, 0))
	FROM
		deleted d
		INNER JOIN inserted i ON d.Id = i.id
		INNER JOIN Loan l ON d.LoanId = l.Id
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'REPAYMENT'
	WHERE
		d.Status != 'Late'
		AND
		i.Status != 'Late'
	
	SET NOCOUNT OFF
END
GO