using EZBob.DatabaseLib.Model.Loans;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class DiscountPlanModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static DiscountPlanModel Create(DiscountPlan discountPlan)
        {
            return new DiscountPlanModel{Id = discountPlan.Id, Name = discountPlan.NameWithPercents};
        }
    }
}