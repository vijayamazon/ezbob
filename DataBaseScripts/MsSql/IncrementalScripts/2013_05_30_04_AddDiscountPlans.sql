DELETE FROM dbo.DiscountPlan WHERE Name = 'Simple Discount'

--insert new discount plans
INSERT INTO dbo.DiscountPlan
 (
 Name,ValuesStr,IsDefault
 )
VALUES
 (
 'New13','0,0,0,-10,-10,-10,-10,-10,-19,-19,-19,-19',0
 )
GO


INSERT INTO dbo.DiscountPlan
 (
 Name,ValuesStr,IsDefault
 )
VALUES
 (
 'Old13','0,0,0,-10,-10,-10,-10,-10,-10,-10,-10,-10', 0
 )
GO

-- update default loan type
UPDATE dbo.LoanType
SET IsDefault = 0
WHERE Id = 2
GO

UPDATE dbo.LoanType
SET IsDefault = 1
WHERE Id = 1
GO