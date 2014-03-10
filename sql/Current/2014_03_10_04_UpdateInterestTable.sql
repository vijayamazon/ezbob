IF EXISTS(SELECT 1 FROM sys.columns WHERE [name] = N'LoanIntrestBase' AND [object_id] = OBJECT_ID(N'BasicInterestRate'))
BEGIN
    EXEC sp_RENAME 'BasicInterestRate.LoanIntrestBase' , 'LoanInterestBase', 'COLUMN'
END
GO

UPDATE BasicInterestRate SET LoanInterestBase = 0.06 WHERE FromScore = 0
UPDATE BasicInterestRate SET LoanInterestBase = 0.05 WHERE FromScore = 650
UPDATE BasicInterestRate SET LoanInterestBase = 0.04 WHERE FromScore = 850
UPDATE BasicInterestRate SET LoanInterestBase = 0.03 WHERE FromScore = 1000
UPDATE BasicInterestRate SET LoanInterestBase = 0.02 WHERE FromScore = 1100
GO
