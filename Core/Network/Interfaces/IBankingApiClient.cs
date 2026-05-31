namespace BosesApp.Core.Network.Interfaces;

/// <summary>
/// Production banking API client interface — Phase 3 (Brankas / UnionBank).
///
/// SIMULATION: Register MockBrankasApiClient (already wired) — uses IBankApiClient.
/// PRODUCTION: Implement this interface against the real Brankas REST API.
///   Base URL: https://api.brankas.com/v1
///   Auth:     OAuth 2.0 Bearer token (see BrankasOAuth2Service — to be created)
///
/// NOTE: Type names here are prefixed with "Banking" to avoid collision with the
/// shared models in IBankApiClient.cs (BankAccount, BankTransaction, TransferRequest).
/// </summary>
public interface IBankingApiClient
{
    /// <summary>Authenticate with OAuth 2.0 access token.</summary>
    Task<bool> AuthenticateAsync(string accessToken);

    /// <summary>Get enhanced account details including account name and currency.</summary>
    Task<BankingAccount> GetAccountAsync(string accountId);

    /// <summary>Get real-time account balance.</summary>
    Task<decimal> GetBalanceAsync(string accountId);

    /// <summary>Get paginated transaction history.</summary>
    Task<IEnumerable<BankingTransaction>> GetTransactionHistoryAsync(string accountId, int limit = 10);

    /// <summary>Initiate a fund transfer; may return a pending OTP challenge.</summary>
    Task<BankingTransferResponse> InitiateTransferAsync(BankingTransferRequest request);

    /// <summary>Confirm a transfer with the one-time password sent to the account holder.</summary>
    Task<bool> ConfirmTransferAsync(string transferId, string otp);

    /// <summary>Health-check the upstream banking API.</summary>
    Task<bool> IsAvailableAsync();

    /// <summary>List all accounts linked to the authenticated user.</summary>
    Task<IEnumerable<BankingLinkedAccount>> GetLinkedAccountsAsync();
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

/// <summary>Transfer request payload for the production banking API.</summary>
public class BankingTransferRequest
{
    public string FromAccountId { get; set; } = string.Empty;
    public string ToAccountId { get; set; } = string.Empty;
    public string ToAccountNumber { get; set; } = string.Empty;
    public string ToAccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string BankCode { get; set; } = "UNIONBANK";
}

/// <summary>Transfer initiation response — may require OTP confirmation.</summary>
public class BankingTransferResponse
{
    public string TransferId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // PENDING, PROCESSING, SUCCESS, FAILED
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool RequiresOtp { get; set; }
}

/// <summary>A bank account linked to the authenticated user (production model).</summary>
public class BankingLinkedAccount
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // SAVINGS, CHECKING
    public decimal Balance { get; set; }
}

/// <summary>Enhanced bank account model returned by the production API.</summary>
public class BankingAccount
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

/// <summary>Enhanced transaction model returned by the production API.</summary>
public class BankingTransaction
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
