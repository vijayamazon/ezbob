SET QUOTED_IDENTIFIER ON
GO

IF 0 = (SELECT is_identity FROM sys.columns WHERE object_id = OBJECT_ID('DecisionHistory') AND name = 'Id')
BEGIN
	BEGIN TRANSACTION

	BEGIN TRY
		ALTER TABLE DecisionHistoryRejectReason DROP CONSTRAINT FK_DecisionHistoryRejectReason_DecisionHistory
		ALTER TABLE DecisionHistory DROP CONSTRAINT PK_DecisionHistory

		EXECUTE sp_rename 'DecisionHistory', 'OldDecisionHistory'

		CREATE TABLE DecisionHistory (
			Id INT IDENTITY(1, 1) NOT NULL,
			Action NVARCHAR(50) NOT NULL,
			Date DATETIME NOT NULL,
			Comment NVARCHAR(2000) NULL,
			UnderwriterId INT NOT NULL,
			CustomerId INT NOT NULL,
			CashRequestId BIGINT NULL,
			LoanTypeId INT NULL,
			TimestampCounter ROWVERSION,
			CONSTRAINT PK_DecisionHistory PRIMARY KEY (Id),
			CONSTRAINT FK_DecisionHistory_Underwriter FOREIGN KEY (UnderwriterId) REFERENCES Security_User (UserId),
			CONSTRAINT FK_DecisionHistory_Customer FOREIGN KEY (CustomerId) REFERENCES Customer (Id),
			CONSTRAINT FK_DecisionHistory_CashRequest FOREIGN KEY (CashRequestId) REFERENCES CashRequests (Id),
			CONSTRAINT FK_DecisionHistory_LoanType FOREIGN KEY (LoanTypeId) REFERENCES LoanType (Id)
		)

		SET IDENTITY_INSERT DecisionHistory ON

		INSERT INTO DecisionHistory (
			Id,
			Action,
			Date,
			Comment,
			UnderwriterId,
			CustomerId,
			CashRequestId,
			LoanTypeId
		)
		SELECT
			Id,
			Action,
			Date,
			Comment,
			UnderwriterId,
			CustomerId,
			CashRequestId,
			LoanTypeId
		FROM
			OldDecisionHistory

		SET IDENTITY_INSERT DecisionHistory OFF

		ALTER TABLE DecisionHistoryRejectReason
			ADD CONSTRAINT FK_DecisionHistoryRejectReason_DecisionHistory FOREIGN KEY (DecisionHistoryId) REFERENCES DecisionHistory (Id)

		DROP TABLE OldDecisionHistory

		COMMIT TRANSACTION

		SELECT 'DecisionHistory has been updated.' AS Result
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION
	END CATCH
END
GO
