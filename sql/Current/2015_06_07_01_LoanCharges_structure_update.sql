SET QUOTED_IDENTIFIER ON
GO

IF 0 = (SELECT is_identity FROM sys.columns WHERE object_id = OBJECT_ID('LoanCharges') AND name = 'Id')
BEGIN
	BEGIN TRANSACTION

	BEGIN TRY
		ALTER TABLE LoanCharges DROP CONSTRAINT PK_LoanCharges

		EXECUTE sp_rename 'LoanCharges', 'OldLoanCharges'

		CREATE TABLE LoanCharges (
		   Id INT IDENTITY(1, 1) NOT NULL,
		   Amount DECIMAL(18,4) NOT NULL,
		   LoanId INT NOT NULL,
		   ConfigurationVariableId INT,
		   Date DATETIME,
		   -- InstallmentId INT NULL,
		   AmountPaid DECIMAL(18,4),
		   State VARCHAR(50),
		   Description TEXT,
		   TimestampCounter ROWVERSION,
		   CONSTRAINT PK_LoanCharges PRIMARY KEY(Id),
		   CONSTRAINT FK_LoanCharges_Loan FOREIGN KEY(LoanId) REFERENCES Loan(Id),
		   CONSTRAINT FK_LoanCharges_CfgValues FOREIGN KEY(ConfigurationVariableId) REFERENCES ConfigurationVariables(iD)
		)

		SET IDENTITY_INSERT LoanCharges ON

		INSERT INTO LoanCharges (
			Id,
			Amount,
			LoanId,
			ConfigurationVariableId,
			[Date],
			-- InstallmentId,
			AmountPaid,
			State,
			Description
		)
		SELECT
			Id,
			Amount,
			LoanId,
			ConfigurationVariableId,
			[Date],
			-- InstallmentId,
			AmountPaid,
			State,
			Description
		FROM
			OldLoanCharges

		SET IDENTITY_INSERT LoanCharges OFF

		DROP TABLE OldLoanCharges

		COMMIT TRANSACTION

		SELECT 'LoanCharges has been updated.' AS Result
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION
	END CATCH
END
GO
