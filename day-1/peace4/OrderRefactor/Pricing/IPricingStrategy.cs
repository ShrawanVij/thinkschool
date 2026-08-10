namespace OrderRefactor.Pricing;

public interface IPricingStrategy
{
    decimal Apply(decimal total, PricingContext context);
}