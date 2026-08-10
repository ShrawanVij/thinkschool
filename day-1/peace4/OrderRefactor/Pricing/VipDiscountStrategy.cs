namespace OrderRefactor.Pricing;

public class VipDiscountStrategy : IPricingStrategy
{
    public decimal Apply(decimal total, PricingContext context)
    {
        return context.IsVip ? total * 0.95m : total;
    }
}