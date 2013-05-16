DECLARE @TableName sysname
DECLARE @DefaultConstraint sysname
DECLARE @DropStmt NVARCHAR(500)

SET @TableName = 'CashRequests'

IF EXISTS (SELECT * FROM syscolumns WHERE name = 'IsLoanTypeSelectionAllowed' AND id = OBJECT_ID(@TableName))
BEGIN
	SELECT @DefaultConstraint = NAME FROM sys.default_constraints WHERE parent_object_id = object_ID(@TableName)
	SET @DropStmt = 'ALTER TABLE ' + @TableName + ' DROP CONSTRAINT ' + @DefaultConstraint

	EXEC(@DropStmt)

	ALTER TABLE CashRequests ALTER COLUMN IsLoanTypeSelectionAllowed INT

	ALTER TABLE CashRequests ADD CONSTRAINT DF_CashRequests_IsLoanType DEFAULT 0 FOR IsLoanTypeSelectionAllowed
END
ELSE BEGIN
	ALTER TABLE CashRequests ADD IsLoanTypeSelectionAllowed INT NOT NULL DEFAULT (0)
END

SET @TableName = 'Customer'

IF EXISTS (SELECT * FROM syscolumns WHERE name = 'IsLoanTypeSelectionAllowed' AND id = OBJECT_ID(@TableName))
BEGIN
	SELECT @DefaultConstraint = NAME FROM sys.default_constraints WHERE parent_object_id = object_ID(@TableName)
	SET @DropStmt = 'ALTER TABLE ' + @TableName + ' DROP CONSTRAINT ' + @DefaultConstraint

	EXEC(@DropStmt)

	ALTER TABLE Customer ALTER COLUMN IsLoanTypeSelectionAllowed INT

	ALTER TABLE Customer ADD CONSTRAINT DF_Customer_IsLoanType DEFAULT 0 FOR IsLoanTypeSelectionAllowed
END
ELSE BEGIN
	ALTER TABLE Customer ADD IsLoanTypeSelectionAllowed INT NOT NULL DEFAULT (0)
END
GO
