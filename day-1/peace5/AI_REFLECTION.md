# AI Reflection

Claude got the main structure right by moving the large-order and VIP discount rules out of OrderService and into separate strategy classes. Dependency injection made the strategies available to the service without hard-coding the implementations. I also found that the strategy design needed a PricingContext because the VIP strategy needs customer information. The tests passing after the changes gave me confidence that the original behavior was preserved.

One bug I would have caught during review was the initial design problem where the pricing strategy interface did not have enough information for the VIP rule. Another issue was the missing assignment of `_pricingStrategies` in the OrderService constructor, which caused a NullReferenceException. Reading the diff and running the tests exposed these problems before the change was considered complete.

Copilot-style test suggestions saved time by giving me a starting point for validation tests. I added tests for negative quantity, zero quantity, and invalid product ID. These tests increased the test count from 4 to 7 and all seven passed.

A subtle issue was treating QuantityDiscountStrategy as if it belonged in the same strategy interface even though it operates at the individual item level. It returned the total unchanged and was ultimately removed instead of keeping an incomplete abstraction.

At 2 AM while debugging production, I would reach for Claude first for understanding and exploring the problem, but I would not blindly accept its changes. I would verify the proposed fix with tests and by reading the diff myself.