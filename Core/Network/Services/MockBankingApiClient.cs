using BosesApp.Core.Network.Interfaces;
using System.Diagnostics;

namespace BosesApp.Core.Network.Services;

/// <summary>
/// Phase 3 — Mock implementation of the production IBankingApiClient (Brankas / UnionBank).
///
/// SIMULATION (always active for demo — this class IS the mock):
///   Returns realistic Philippine banking data with simulated network latency.
///   All amounts are in PHP. No real API calls are made.
///
/// PRODUCTION PATH:
///   Replace this class with BrankasApiClient that calls:
///     Base URL : https://api.brankas.com/v1
///     Auth     : OAuth 2.0 Bearer token via BrankasOAuth2Service
///   Registration: https://developer.brankas.com
///
///   Or replace with UnionBankApiClient for direct bank integration:
///     Base URL : https://developer.unionbankph.com/apis
///     Auth     : OAuth 2.0 client_credentials flow
/// </summary>
public class MockBankingApiClient : IBankingApiClient
{
    // ?? Mock data ??????????????????????????????????????????????????????????????
    private bool _isAuthenticated = false;

    private readonly List<BankingAccount> _accounts =
    [
        new BankingAccount
        {
            AccountId     = "UB-001",
            AccountNumber = "****7890",
            AccountName   = "Juan dela Cruz",
            BankName      = "UnionBank",
            AccountType   = "SAVINGS",
            Balance       = 15_750.50m,
            Currency      = "PHP",
            LastUpdated   = DateTime.Now
        },
        new BankingAccount
        {
            AccountId     = "BDO-001",
            AccountNumber = "****4321",
            AccountName   = "Juan dela Cruz",
            BankName      = "BDO",
            AccountType   = "CHECKING",
            Balance       = 8_320.75m,
            Currency      = "PHP",
            LastUpdated   = DateTime.Now
        }
    ];

    private readonly List<BankingTransaction> _transactions =
    [
        new BankingTransaction
        {
            TransactionId   = "TXN-001",
            Date            = DateTime.Now.AddDays(-1),
            Description     = "Puregold Supermarket",
            Amount          = -1_250.00m,
            Type            = "DEBIT",
            Balance         = 15_750.50m,
            Status          = "COMPLETED",
            ReferenceNumber = "REF20250528001"
        },
        new BankingTransaction
        {
            TransactionId   = "TXN-002",
            Date            = DateTime.Now.AddDays(-3),
            Description     = "Salary – ABC Company",
            Amount          = 22_000.00m,
            Type            = "CREDIT",
            Balance         = 17_000.50m,
            Status          = "COMPLETED",
            ReferenceNumber = "REF20250526001"
        },
        new BankingTransaction
        {
            TransactionId   = "TXN-003",
            Date            = DateTime.Now.AddDays(-5),
            Description     = "Meralco Bill Payment",
            Amount          = -2_140.00m,
            Type            = "DEBIT",
            Balance         = 15_000.50m,
            Status          = "COMPLETED",
            ReferenceNumber = "REF20250524001"
        },
        new BankingTransaction
        {
            TransactionId   = "TXN-004",
            Date            = DateTime.Now.AddDays(-7),
            Description     = "Mercury Drug – Maintenance meds",
            Amount          = -850.00m,
            Type            = "DEBIT",
            Balance         = 12_860.50m,
            Status          = "COMPLETED",
            ReferenceNumber = "REF20250522001"
        },
        new BankingTransaction
        {
            TransactionId   = "TXN-005",
            Date            = DateTime.Now.AddDays(-10),
            Description     = "SSS Pension Deposit",
            Amount          = 8_000.00m,
            Type            = "CREDIT",
            Balance         = 13_710.50m,
            Status          = "COMPLETED",
            ReferenceNumber = "REF20250519001"
        }
    ];

    // ?? IBankingApiClient ??????????????????????????????????????????????????????

    public async Task<bool> AuthenticateAsync(string accessToken)
    {
        await SimulateLatencyAsync(300, 600);
        // [SIMULATION] Accept any non-empty token
        // PRODUCTION: POST /oauth/token, validate JWT, store bearer token
        _isAuthenticated = !string.IsNullOrWhiteSpace(accessToken);
        Debug.WriteLine($"[BankingClient][MOCK] AuthenticateAsync ? {_isAuthenticated}");
        return _isAuthenticated;
    }

    public async Task<BankingAccount> GetAccountAsync(string accountId)
    {
        await SimulateLatencyAsync(400, 900);
        EnsureAuthenticated();
        var account = _accounts.FirstOrDefault(a => a.AccountId == accountId)
            ?? throw new InvalidOperationException($"Account {accountId} not found.");
        Debug.WriteLine($"[BankingClient][MOCK] GetAccountAsync({accountId}) ? {account.BankName} {account.Balance:C}");
        return account;
    }

    public async Task<decimal> GetBalanceAsync(string accountId)
    {
        await SimulateLatencyAsync(500, 1_200);
        EnsureAuthenticated();
        var account = _accounts.FirstOrDefault(a => a.AccountId == accountId)
            ?? throw new InvalidOperationException($"Account {accountId} not found.");
        Debug.WriteLine($"[BankingClient][MOCK] GetBalanceAsync({accountId}) ? ?{account.Balance:N2}");
        return account.Balance;
    }

    public async Task<IEnumerable<BankingTransaction>> GetTransactionHistoryAsync(
        string accountId, int limit = 10)
    {
        await SimulateLatencyAsync(600, 1_500);
        EnsureAuthenticated();
        var txns = _transactions
            .OrderByDescending(t => t.Date)
            .Take(limit)
            .ToList();
        Debug.WriteLine($"[BankingClient][MOCK] GetTransactionHistoryAsync({accountId}) ? {txns.Count} records");
        return txns;
    }

    public async Task<BankingTransferResponse> InitiateTransferAsync(BankingTransferRequest request)
    {
        await SimulateLatencyAsync(1_500, 3_000);
        EnsureAuthenticated();

        var from = _accounts.FirstOrDefault(a => a.AccountId == request.FromAccountId);
        if (from == null)
            return Fail("Source account not found.");
        if (from.Balance < request.Amount)
            return Fail($"Insufficient funds. Available: ?{from.Balance:N2}");

        // [SIMULATION] Deduct balance and log transaction
        from.Balance -= request.Amount;
        from.LastUpdated = DateTime.Now;

        var txnId = $"TXN-{DateTime.Now.Ticks % 100_000:D5}";
        _transactions.Insert(0, new BankingTransaction
        {
            TransactionId   = txnId,
            Date            = DateTime.Now,
            Description     = $"Transfer to {request.ToAccountNumber} — {request.Description}",
            Amount          = -request.Amount,
            Type            = "DEBIT",
            Balance         = from.Balance,
            Status          = "COMPLETED",
            ReferenceNumber = $"REF{DateTime.Now:yyyyMMddHHmmss}"
        });

        Debug.WriteLine($"[BankingClient][MOCK] Transfer ?{request.Amount:N2} ? {request.ToAccountNumber}. New balance: ?{from.Balance:N2}");

        return new BankingTransferResponse
        {
            TransferId  = txnId,
            Status      = "SUCCESS",
            Message     = $"Transfer of ?{request.Amount:N2} to {request.ToAccountNumber} completed.",
            CreatedAt   = DateTime.Now,
            RequiresOtp = false
        };
    }

    public async Task<bool> ConfirmTransferAsync(string transferId, string otp)
    {
        await SimulateLatencyAsync(400, 800);
        // [SIMULATION] Accept any 4–6 digit OTP for demo
        var valid = otp.Length is >= 4 and <= 6 && otp.All(char.IsDigit);
        Debug.WriteLine($"[BankingClient][MOCK] ConfirmTransferAsync({transferId}, otp={otp}) ? {valid}");
        return valid;
    }

    public async Task<bool> IsAvailableAsync()
    {
        await SimulateLatencyAsync(100, 300);
        // [SIMULATION] Always available
        // PRODUCTION: GET /health or /ping on the banking API
        Debug.WriteLine("[BankingClient][MOCK] IsAvailableAsync ? true");
        return true;
    }

    public async Task<IEnumerable<BankingLinkedAccount>> GetLinkedAccountsAsync()
    {
        await SimulateLatencyAsync(400, 900);
        EnsureAuthenticated();
        var linked = _accounts.Select(a => new BankingLinkedAccount
        {
            AccountId     = a.AccountId,
            AccountNumber = a.AccountNumber,
            AccountName   = a.AccountName,
            BankName      = a.BankName,
            AccountType   = a.AccountType,
            Balance       = a.Balance
        }).ToList();
        Debug.WriteLine($"[BankingClient][MOCK] GetLinkedAccountsAsync ? {linked.Count} accounts");
        return linked;
    }

    // ?? Helpers ????????????????????????????????????????????????????????????????

    private void EnsureAuthenticated()
    {
        // In simulation we are lenient — auto-authenticate if not done
        if (!_isAuthenticated)
            _isAuthenticated = true;
    }

    private static BankingTransferResponse Fail(string message) => new()
    {
        Status  = "FAILED",
        Message = message,
        CreatedAt = DateTime.Now
    };

    private static async Task SimulateLatencyAsync(int minMs, int maxMs) =>
        await Task.Delay(Random.Shared.Next(minMs, maxMs));
}
