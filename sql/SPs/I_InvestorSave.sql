SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorSave') IS NOT NULL
	DROP PROCEDURE I_InvestorSave
GO
IF OBJECT_ID('I_InvestorDetailsSave') IS NOT NULL
	DROP PROCEDURE I_InvestorDetailsSave
GO
IF OBJECT_ID('I_InvestorDetailsUpdate') IS NOT NULL
	DROP PROCEDURE I_InvestorDetailsUpdate
GO

IF TYPE_ID('I_InvestorList') IS NOT NULL
	DROP TYPE I_InvestorList
GO

CREATE TYPE I_InvestorList AS TABLE (
	
	[InvestorTypeID] INT NOT NULL,
	[Name] NVARCHAR(255) NULL,
	[IsActive] BIT NOT NULL,
	[Timestamp] DATETIME NOT NULL,
	[MonthlyFundingCapital] decimal(18, 6) NOT NULL,
	[FundingLimitForNotification] decimal(18, 6) NOT NULL,
	[FundsTransferDate] INT NOT NULL
	
)
GO

CREATE PROCEDURE I_InvestorSave
@Tbl I_InvestorList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	--Check if investor exists
	IF EXISTS (SELECT * FROM I_Investor i INNER JOIN @Tbl t ON i.Name=t.Name)
	BEGIN
		SELECT 0 AS ScopeID
	END

	INSERT INTO I_Investor (
		[InvestorTypeID],
		[Name],
		[IsActive],
		[Timestamp],
		[MonthlyFundingCapital],
		[FundingLimitForNotification],
		[FundsTransferDate]
	) SELECT
		[InvestorTypeID],
		[Name],
		[IsActive],
		[Timestamp],
		[MonthlyFundingCapital],
		[FundingLimitForNotification],
		[FundsTransferDate]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


CREATE PROCEDURE I_InvestorDetailsUpdate
@InvestorTypeID Int,
@IsActive bit,
@Name  nvarchar(255),
@Timestamp datetime,
@FundingLimitForNotification  decimal(18, 6),
@InvestorID Int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE 
		I_Investor
	SET 
		
		I_Investor.[Name] = @Name ,
		I_Investor.[InvestorTypeID] = @InvestorTypeID,
		I_Investor.[IsActive] = @IsActive,
		I_Investor.[Timestamp] = @Timestamp,
		I_Investor.[FundingLimitForNotification] = @FundingLimitForNotification
	FROM
		I_Investor b 
	where
		b.InvestorID = @InvestorID
END
GO




