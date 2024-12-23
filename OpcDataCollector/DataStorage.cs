using System;
using System.Collections.Generic;
using System.Data.SQLite;

public class DataStorage
{
    private readonly string connectionString;

    public DataStorage(string databaseFile)
    {
        connectionString = $"Data Source={databaseFile};Version=3;";
    }

    /// <summary>
    /// 初始化数据库，创建表结构
    /// </summary>
    public void InitializeDatabase()
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string query = @"CREATE TABLE IF NOT EXISTS DataLog (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                TagName TEXT NOT NULL,
                                Value REAL NOT NULL,
                                Timestamp DATETIME NOT NULL
                             )";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// 插入数据到数据库
    /// </summary>
    public void InsertData(string tagName, double value, DateTime timestamp)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string query = "INSERT INTO DataLog (TagName, Value, Timestamp) VALUES (@TagName, @Value, @Timestamp)";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TagName", tagName);
                command.Parameters.AddWithValue("@Value", value);
                command.Parameters.AddWithValue("@Timestamp", timestamp);
                command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// 查询指定时间范围内的数据
    /// </summary>
    public List<HistoricalData> QueryDataByTimeRange(DateTime startTime, DateTime endTime)
    {
        var result = new List<HistoricalData>();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT TagName, Value, Timestamp FROM DataLog WHERE Timestamp BETWEEN @StartTime AND @EndTime";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@EndTime", endTime);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new HistoricalData
                        {
                            TagName = reader.GetString(0),
                            Value = reader.GetDouble(1),
                            Timestamp = reader.GetDateTime(2)
                        });
                    }
                }
            }
        }

        return result;
    }
}

/// <summary>
/// 历史数据实体类
/// </summary>
public class HistoricalData
{
    public string TagName { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
}
