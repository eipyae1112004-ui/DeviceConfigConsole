using Microsoft.Data.SqlClient;

const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ICAS_Test;Trusted_Connection=True;";

// 1. Validate the CLI argument before touching the database at all.
int? equipId = null;
if (args.Length > 0)
{
    if (!int.TryParse(args[0], out int parsedId))
    {
        Console.Error.WriteLine($"Error: '{args[0]}' is not a valid device ID. Please provide a whole number, e.g. DeviceCfg.exe 3");
        return 1;
    }
    equipId = parsedId;
}

string sql = "SELECT * FROM dbo.t_DeviceCfg";
if (equipId.HasValue)
{
    sql += " WHERE EquipId = @EquipId";
}

// 2. Anything that can fail talking to the database goes through this try/catch.
try
{
    using var connection = new SqlConnection(ConnectionString);
    connection.Open();

    using var command = new SqlCommand(sql, connection);
    if (equipId.HasValue)
    {
        command.Parameters.AddWithValue("@EquipId", equipId.Value);
    }

    using var reader = command.ExecuteReader();

    if (!reader.HasRows)
    {
        Console.Error.WriteLine(equipId.HasValue
            ? $"Error: no device found with EquipId {equipId.Value}."
            : "No devices found in dbo.t_DeviceCfg.");
        return 1;
    }

    // AFTER: Extract headers
    var headers = new string[reader.FieldCount];
    for (int i = 0; i < reader.FieldCount; i++)
    {
        headers[i] = reader.GetName(i);
    }

    var rows = new List<string[]>();
    var warnings = new List<string>();

    while (reader.Read())
    {
        var values = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
        {
            values[i] = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString() ?? "";
        }
        rows.Add(values);
        CheckForWarnings(reader, warnings);
    }

    // Calculate maximum column widths
    int[] colWidths = new int[headers.Length];
    for (int i = 0; i < headers.Length; i++)
    {
        colWidths[i] = headers[i].Length;
    }

    foreach (var row in rows)
    {
        for (int i = 0; i < row.Length; i++)
        {
            if (row[i].Length > colWidths[i])
            {
                colWidths[i] = row[i].Length;
            }
        }
    }

    // Print table
    PrintAlignedTable(headers, rows, colWidths);

    if (warnings.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("Warnings:");
        foreach (var warning in warnings)
        {
            Console.WriteLine($" - {warning}");
        }
    }
}
catch (SqlException ex)
{
    Console.Error.WriteLine("Error: could not read device configuration from the database.");
    Console.Error.WriteLine($"Details: {ex.Message}");
    return 1;
}

return 0;

static void PrintAlignedTable(string[] headers, List<string[]> rows, int[] colWidths)
{
    PrintRowFormatted(headers, colWidths);

    var separator = new string[headers.Length];
    for (int i = 0; i < headers.Length; i++)
    {
        separator[i] = new string('-', colWidths[i]);
    }
    Console.WriteLine(string.Join("-+-", separator));

    foreach (var row in rows)
    {
        PrintRowFormatted(row, colWidths);
    }
}

static void PrintRowFormatted(string[] values, int[] colWidths)
{
    var padded = new string[values.Length];
    for (int i = 0; i < values.Length; i++)
    {
        padded[i] = values[i].PadRight(colWidths[i]);
    }
    Console.WriteLine(string.Join(" | ", padded));
}

// 3. Flag rows whose data doesn't make sense, without aborting the rest of the listing.
static void CheckForWarnings(SqlDataReader reader, List<string> warnings)
{
    int equipId = reader.GetInt32(reader.GetOrdinal("EquipId"));
    string equipName = reader.GetString(reader.GetOrdinal("EquipName"));
    short enable = reader.GetInt16(reader.GetOrdinal("Enable"));
    bool ipIsNull = reader.IsDBNull(reader.GetOrdinal("IpAddress"));
    if (enable != 0 && ipIsNull)
    {
        warnings.Add($"EquipId {equipId} ({equipName}) is enabled but has no IP address — it cannot be reached on the network.");
    }

    bool triggerIsNull = reader.IsDBNull(reader.GetOrdinal("Trigger"));
    if (enable != 0 && triggerIsNull)
    {
        warnings.Add($"EquipId {equipId} ({equipName}) is enabled but has no Trigger mode — the program doesn't know when to read it.");
    }

    int slotOrdinal = reader.GetOrdinal("SlotIndex");
    if (!reader.IsDBNull(slotOrdinal) && reader.GetInt32(slotOrdinal) < 0)
    {
        int slotIndex = reader.GetInt32(slotOrdinal);
        warnings.Add($"EquipId {equipId} ({equipName}) has SlotIndex {slotIndex}, which is not a valid slot position.");
    }
}
