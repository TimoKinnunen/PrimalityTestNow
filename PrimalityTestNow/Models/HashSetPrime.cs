using System.Numerics;

namespace PrimalityTestNow.Models
{
    public class HashSetPrime
    {
        public HashSetPrime() { }

        public HashSetPrime(int id, BigInteger primeNumber, BigInteger forwardX)
        {
            Id = id;
            Primenumber = primeNumber;
            ForwardX = forwardX;
        }

        public int Id { get; set; }

        public BigInteger Primenumber { get; set; }

        public BigInteger ForwardX { get; set; }
    }
}
