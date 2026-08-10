# Round 1 - AI Refactoring Prompt

Refactor the pricing and discount logic in OrderService.cs using the Strategy Pattern.

The goal is to allow new pricing/discount rules to be added without modifying OrderService each time.

Current rules include:
- 10% discount when an item's quantity is greater than 10
- $50 discount when the order total is greater than 1000
- 5% VIP discount
- Coupon discount when a valid coupon is provided

Requirements:
- Keep the existing behavior.
- Do not over-engineer the solution.
- Create a simple strategy interface.
- Each pricing rule should be implemented as a separate strategy.
- OrderService should use the strategies instead of containing the discount rules directly.
- Use dependency injection.
- Keep the code asynchronous where it already is.
- Do not change unrelated order creation logic.
- Explain each file you create or modify.
- Show the proposed diff before applying changes.