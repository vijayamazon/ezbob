DECLARE @discountPlanId INT
select @discountPlanId = id from DiscountPlan where Name = 'No Discount'

update CashRequests set DiscountPlanId = @discountPlanId where DiscountPlanId is null