# AI_NOTES

**1. What I asked the AI to do**
- Read through the task's four requirements myself first.
- Asked Claude to draft the C# console app in stages matching those requirements:
  - Connect to SQL Server and print all rows.
  - Filter by device ID via a command-line argument.
  - Add validation and error handling.
- Asked Claude to explain each piece of code as it was written, so I could understand it and edit/extend it myself later.
- Ran and tested every stage myself before moving on to the next one.
- Started learning C# only two days before this task, so most of the explanations were genuinely new to me, not a refresher.

**2. One thing it got wrong, and how I noticed**
- Claude had me test the "database unreachable" case by pointing the connection string at a fake SQL Server instance.
- It gave me that test before having me also test the "device ID doesn't exist" case.
- I ran `dotnet run -- 999` expecting the "no device found" message, but got a database connection error instead.
- I noticed because the output didn't match what we'd just discussed the code should do.
- The code itself was correct — the mistake was the order of the tests, since the connection was still broken from the previous step.
- Fixed by reverting the connection string, re-confirming the earlier tests passed, then re-breaking it only for the actual unreachable-database test at the end.

**3. What I changed myself**
- Added a column-aligned table output (beyond the original pipe-separated version).
- Added a third warning check for devices that are enabled but have no Trigger mode set.
- Mistake 1: overwrote the existing IP-address check instead of adding a separate one for Trigger, which silently broke a check that had been working.
  - Found this by testing a device I knew should trigger the old warning and noticing it had disappeared.
- Mistake 2: computed a truncated value for long text, but then printed the original untruncated value by mistake, so truncation never actually applied.
  - Found this by reading through the code with Claude line by line.
- Independently noticed, while reading the data, that EquipId 9 and EquipId 8 share the same TaskID, unlike every other row where TaskID matches EquipId.
  - Decided not to build a check for it since I wasn't sure it was an actual data error rather than intentional grouping — documented that reasoning in `NOTES.md` instead.
