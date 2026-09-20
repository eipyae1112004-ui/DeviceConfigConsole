using Microsoft.Data.SqlClient;

const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ICAS_Test;Trusted_Connection=True;";

using var connection = new SqlConnection(ConnectionString);
connection.Open();

string sql = "SELECT * FROM dbo.t_DeviceCfg";
int? equipId = args.Length > 0 ? int.Parse(args[0]) : null;
if (equipId.HasValue)
{
    sql += " WHERE EquipId = @EquipId";
}

using var command = new SqlCommand(sql, connection);
if (equipId.HasValue)
{
    command.Parameters.AddWithValue("@EquipId", equipId.Value);
}

using var reader = command.ExecuteReader();

PrintHeader(reader);
while (reader.Read())
{
    PrintRow(reader);
}

static void PrintHeader(SqlDataReader reader)
{
    var headers = new string[reader.FieldCount];
    for (int i = 0; i < reader.FieldCount; i++)
    {
        headers[i] = reader.GetName(i);
    }
    Console.WriteLine(string.Join(" | ", headers));
}

static void PrintRow(SqlDataReader reader)
{
    var values = new string[reader.FieldCount];
    for (int i = 0; i < reader.FieldCount; i++)
    {
        values[i] = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString() ?? "";
    }
    Console.WriteLine(string.Join(" | ", values));
}
