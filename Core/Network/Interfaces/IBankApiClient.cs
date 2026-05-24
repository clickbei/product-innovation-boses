namespace BosesApp.Core.Network.Interfaces;

/// <summary>
/// Open Banking API client interface
/// Simulates integration with aggregators like Brankas or UnionBank Sandbox
/// Production-ready design with realistic async patterns
/// </summary>
public interface IBankApiClient
{
    /// <summary>
    /// Get account balance
    /// </summary>
    Task<BankAccountBalance> GetBalanceAsync(string accountId);

    /// <summary>
    /// Get transaction history
    /// </summary>
    Task<IEnumerable<BankTransaction>> GetTransactionsAsync(string accountId, int limit = 10);

    /// <summary>
    /// Initiate a fund transfer
    /// </summary>
    Task<TransferResult> TransferFundsAsync(TransferRequest request);

    /// <summary>
    /// Get list of linked accounts
    /// </summary>
    Task<IEnumerable<BankAccount>> GetLinkedAccountsAsync(int userId);

    /// <summary>
    /// Verify account ownership
    /// </summary>
    Task<bool> VerifyAccountAsync(string accountId);

    /// <summary>
    /// Enable simulation mode (no real API calls)
    /// </summary>
    bool SimulationMode { get; set; }
}

public class BankAccountBalance
{
    public string AccountId { get; set; } = string.Empty;
    public decimal AvailableBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "PHP";
    public DateTime AsOfDate { get; set; }
}

public class BankTransaction
{
    public string TransactionId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // DEBIT, CREDIT
    public decimal Balance { get; set; }
}

public class BankAccount
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // SAVINGS, CHECKING
    public decimal Balance { get; set; }
}

public class TransferRequest
{
    public string FromAccountId { get; set; } = string.Empty;
    public string ToAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
}

public class TransferResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public decimal NewBalance { get; set; }
}
