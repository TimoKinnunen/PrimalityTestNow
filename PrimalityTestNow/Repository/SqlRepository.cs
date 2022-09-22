using PrimalityTestNow.Models;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrimalityTestNow.Repository
{
    public class SqlRepository
    {
        public SQLiteAsyncConnection db { get; private set; }

        public SqlRepository()
        {
            db = new SQLiteAsyncConnection(App.DatabasePath);

            Task.Run(async () => await CreateTableAsync()).Wait();
        }

        public async Task CreateTableAsync()
        {
            CreateTableResult createTableResult = await db.CreateTableAsync<Prime>();
            switch (createTableResult)
            {
                case CreateTableResult.Created:
                    break;
                case CreateTableResult.Migrated:
                    break;
                default:
                    break;
            }
        }

        public async Task<Prime> SearchPrimeAsync(string primeNumber)
        {
            return await db.Table<Prime>().FirstOrDefaultAsync(p => p.Primenumber == primeNumber).ConfigureAwait(false);
        }

        public async Task<int> UpsertPrimeAsync(Prime prime)
        {
            if (prime.Id == 0)
            {
                var added = await db.InsertAsync(prime).ConfigureAwait(false);
                return prime.Id;
            }
            else
            {
                return await db.UpdateAsync(prime).ConfigureAwait(false);
            }
        }

        public async Task<Prime> GetPrimeAsync(int id)
        {
            return await db.Table<Prime>().FirstOrDefaultAsync(p => p.Id == id).ConfigureAwait(false);
        }

        public async Task<List<Prime>> GetAllPrimesAsync()
        {
            return await db.Table<Prime>().OrderBy(p => p.Id).ToListAsync().ConfigureAwait(false);
        }

        public async Task<int> GetPrimesCountAsync()
        {
            return await db.Table<Prime>().CountAsync().ConfigureAwait(false);
        }

        public async Task<string> GetBiggestPrimenumberAsStringAsync()
        {
            // Reverse the order and take first, it is now the last item
            Prime primeNumberItem = await db.Table<Prime>().OrderByDescending(p => p.Id).FirstOrDefaultAsync().ConfigureAwait(false);
            if (primeNumberItem != null)
            {
                return primeNumberItem.Primenumber;
            }
            else
            {
                return "0";
            }
        }

        public async Task<Prime> GetBiggestPrimeAsync()
        {
            // Reverse the order and take first, it is now the last item
            return await db.Table<Prime>().OrderByDescending(p => p.Id).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<int> DeleteDatabaseTableAsync()
        {
            return await db.DropTableAsync<Prime>();
        }
    }
}
