using SQLite;

namespace PrimalityTestNow.Models
{
    [Table("Primenumbers")]
    public class Prime
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Primenumber { get; set; }
    }
}
