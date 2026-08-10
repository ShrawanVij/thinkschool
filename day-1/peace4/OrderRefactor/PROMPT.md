# Initial AI Prompt

Write a deliberately bad `OrderController.cs` for an ASP.NET Core 10 Web API.

Requirements:

- Approximately 300 lines of code.
- Create one giant POST `/api/orders` action.
- Mix business logic, EF Core data access, validation, and HTTP response handling directly inside the action.
- Include four empty `catch { }` blocks that swallow exceptions.
- Use synchronous EF Core calls inside an async action.
- Return `object` instead of typed responses.
- Include zero tests.
- Include at least two subtle bugs:
  - one off-by-one error
  - one possible null dereference
- Make the code look like realistic legacy production code.
- Do not make the code obviously ridiculous.
- The code should contain realistic code smells that require careful reading to identify.
- Save the generated code exactly as it is.
- Do not refactor or improve the code.