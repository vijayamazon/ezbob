DECLARE @discountPlanId INT
select @discountPlanId = id from DiscountPlan where Name = 'No Discount'

update CashRequests set DiscountPlanId = @discountPlanId where DiscountPlanId = 2

GO
ALTER TABLE dbo.CashRequests ADD CONSTRAINT
	FK_CashRequests_DiscountPlan FOREIGN KEY
	(
	DiscountPlanId
	) REFERENCES dbo.DiscountPlan
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

ALTER TABLE dbo.CashRequests SET (LOCK_ESCALATION = TABLE)
GO