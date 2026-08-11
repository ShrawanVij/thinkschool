# Why a Rich Quote Model?

The rich Quote model puts the business rules where they belong: inside the Quote entity. Previously, Quote was an anemic model containing only public properties, while validation was handled by the API endpoint. This meant another part of the application could create or modify a Quote without necessarily applying the same rules.

The rich model now guarantees that every Quote has a valid author and text when it is created. `Quote.Create(author, text)` validates the input and returns either a valid Quote or a domain error. The `Text` property also cannot be changed after creation because its setter is private. Soft deletion is represented explicitly through `IsDeleted` and `SoftDelete()` rather than requiring callers to manipulate the flag directly.

This prevents business rules from being duplicated across controllers, services, or other callers.

For example, with the anemic model, a future endpoint could accidentally do `quote.Text = ""` or assign an author longer than the allowed limit and save it to the database. The existing POST validation would not protect that code path. With the rich model, those invalid states are prevented by the entity itself.

The main benefit is not simply more validation. It is that the Quote aggregate protects its own invariants regardless of where it is used.