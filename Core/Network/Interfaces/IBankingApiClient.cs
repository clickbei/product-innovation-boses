namespace BosesApp.Core.Network.Interfaces;

/// <summary>
/// Interface for production banking API client
/// Replaces mock implementation with real Brankas Open Banking API
/// </summary>
public interface IBankingApiClient
{
    /// <summary>
    /// Authenticate with OAuth 2.0 token
    /// </summary>
    Task<bool> AuthenticateAsync(string accessToken);

    /// <summary>
    /// Get real account information
    /// </summary>
    Task<BankAccount> GetAccountAsync(string accountId);

    /// <summary>
    /// Get real account balance
    /// </summary>
    Task<decimal> GetBalanceAsync(string accountId);

    /// <summary>
    /// Get real transaction history
    /// </summary>
    Task<IEnumerable<BankTransaction>> GetTransactionHistoryAsync(string accountId, int limit = 10);

    /// <summary>
    /// Initiate a real money transfer
    /// </summary>
    Task<TransferResponse> InitiateTransferAsync(TransferRequest request);

    /// <summary>
    /// Confirm a transfer with OTP
    /// </summary>
    Task<bool> ConfirmTransferAsync(string transferId, string otp);

    /// <summary>
    /// Check if service is available
    /// </summary>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Get linked accounts for user
    /// </summary>
    Task<IEnumerable<LinkedAccount>> GetLinkedAccountsAsync();
}

/// <summary>
/// Request model for money transfer
/// </summary>
public class TransferRequest
{
    public string FromAccountId { get; set; } = string.Empty;
    public string ToAccountId { get; set; } = string.Empty;
    public string ToAccountNumber { get; set; } = string.Empty;
    public string ToAccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string BankCode { get; set; } = "UNIONBANK"; // Default to UnionBank
}

/// <summary>
/// Response model for transfer initiation
/// </summary>
public class TransferResponse
{
    public string TransferId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // PENDING, PROCESSING, SUCCESS, FAILED
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool RequiresOtp { get; set; }
}

/// <summary>
/// Linked account information
/// </summary>
public class LinkedAccount
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // SAVINGS, CHECKING
    public decimal Balance { get; set; }
}

/// <summary>
/// Bank account model (enhanced)
/// </summary>
public class BankAccount
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "PHP";
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Bank transaction model (enhanced)
/// </summary>
public class BankTransaction
{
    public string TransactionId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // DEBIT, CREDIT
    public decimal Balance { get; set; }
    public string Status { get; set; } = "COMPLETED"; // PENDING, COMPLETED, FAILED
    public string ReferenceNumber { get; set; } = string.Empty;
}
