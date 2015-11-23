
IF OBJECT_ID('BigintList') IS NULL
CREATE TYPE [dbo].[BigintList] AS TABLE(
    [Item] BIGINT NOT NULL
);
GO

IF OBJECT_ID('NL_PaidAmountsReset') IS NULL
	EXECUTE('CREATE PROCEDURE NL_PaidAmountsReset AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_PaidAmountsReset]			
	@PaymentIds BigintList READONLY
AS
BEGIN
	SET NOCOUNT ON;	

	UPDATE [NL_LoanSchedulePayments] SET [PrincipalPaid] = 0, [InterestPaid] = 0 WHERE [PaymentID] in (@PaymentIds) ;
	UPDATE [NL_LoanFeePayments] SET [Amount] = 0 WHERE [PaymentID] in (@PaymentIds) ;
		
END