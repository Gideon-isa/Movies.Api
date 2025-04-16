using System.Data;
using Movies.Application.Models;
using Npgsql;

namespace Movies.Application.Database;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}

public class NpgsqlConnectionFactory : IDbConnectionFactory
{
   private readonly string _connectionString;

   public NpgsqlConnectionFactory(string connectionString)
   {
       _connectionString = connectionString;
   }
   
    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}