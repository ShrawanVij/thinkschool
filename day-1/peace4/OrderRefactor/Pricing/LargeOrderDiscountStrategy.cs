namespace OrderRefactor.Pricing;

public class LargeOrderDiscountStrategy : IPricingStrategy
{
    public decimal Apply(decimal total, PricingContext context)
    {
        return total > 1000 ? total - 50 : total;
    }
}