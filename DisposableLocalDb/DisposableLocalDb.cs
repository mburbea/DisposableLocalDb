using Microsoft.Data.SqlClient;
using System;
using System.IO;
using System.Threading;

namespace DisposableLocalDb
{
    public sealed class DisposableLocalDb : IDisposable
    {
        private const string LocalDbString = @"Data Source=(localDb)\mssqllocaldb;Integrated Security=true;";
        private string _database;
        
        public string Database => _database ?? throw new ObjectDisposedException(nameof(DisposableLocalDb));
        public string ConnectionString => Database is string db ? LocalDbString + $"Intial Catalog={db}" : throw new ObjectDisposedException(nameof(DisposableLocalDb));

        public DisposableLocalDb(string database) 
        {
            _database = database;
            CreateDb();
        }

        private void CreateDb()
        {
            DeleteDb();
            using var conn = new SqlConnection(LocalDbString);
            conn.Open();
            using var cmd = new SqlCommand($@"create database symbols on (name='{Database}', fileName='{Path.GetTempFileName()}.mdf');", conn);
            cmd.ExecuteNonQuery();
        }

        private void DeleteDb(string db = null)
        {
            db ??= Database;
            using var conn = new SqlConnection(LocalDbString);
            conn.Open();
            using var cmd = new SqlCommand(@$"
select top 1 physical_name from sys.master_files where db_id('{db}')=database_id;
if db_id('{db}') is not null
begin
    alter database ${db} set single_user with rollback immediate
    exec sp_detach_db '{db}'
end", conn);

            if (cmd.ExecuteScalar() is string path)
            {
                path = path[..^4];
                TryDelete(path);
                TryDelete(path + ".mdf");
                TryDelete(path + "_log.ldf");
            }

            static void TryDelete(string p)
            {
                if (File.Exists(p))
                {
                    File.Delete(p);
                }
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _database, null) is string db)
            {
                DeleteDb(db);
            }
        }
    }
}
