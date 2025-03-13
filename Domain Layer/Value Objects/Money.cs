namespace StockMaster.Domain.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "USD") // Default currency: USD
        {
            if (amount < 0) throw new ArgumentException("Price must be greater than or equal to 0.");
            if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.");

            Amount = amount;
            Currency = currency;
        }

        // Ensure Value Objects compare by VALUE, not reference
        public override bool Equals(object obj)
        {
            if (obj is not Money other) return false;
            return Amount == other.Amount && Currency == other.Currency;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        public override string ToString()
        {
            return $"{Currency} {Amount:F2}";  // Example output: "USD 100.00"
        }
    }
}
