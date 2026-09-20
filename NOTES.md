# NOTES

**Background:** C# was new to me going into this task. I worked through it together with Claude, asking it to explain each piece of code as it was written, and that was enough to get past the parts I didn't understand on my own — I learned it while building this, rather than knowing it beforehand.

**What I'd do differently with more time:**
- The connection string is currently hardcoded to LocalDB. I'd make it configurable (e.g. an environment variable) so pointing at a different SQL Server doesn't require a code change.
- I only tested manually, by running the program with different inputs and checking the output myself. With more time I'd add automated tests, especially around the error-handling paths.
- While looking at the sample data, I noticed EquipId 9 has `TaskID = 8`, the same TaskID as EquipId 8 — every other row has `TaskID` equal to its own `EquipId`, so this is the only place two different devices share one. I wasn't sure whether this is a genuine data problem or an intentional grouping (e.g. a lift controller and its outfeed scanner sharing a task), so rather than guess, I decided not to implement a warning for it. With more time, I'd either confirm the intended relationship or add a check once I was sure it was actually wrong.

**What's unfinished / not happy with:**
- The TaskID duplicate check mentioned above.
