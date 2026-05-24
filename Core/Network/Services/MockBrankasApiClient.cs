using BosesApp.Core.Network.Interfaces;

namespace BosesApp.Core.Network.Services;

/// <summary>
/// Mock implementation of Brankas Open Banking API
/// Simulates realistic network delays and responses for hackathon demo
/// Production-ready architecture with async patterns
/// </summary>
public class MockBrankasApiClient : IBankApiClient
{
    private readonly Random _random = new();
    private readonly Dictionary<string, BankAccount> _mockAccounts = new();
    private readonly List<BankTransaction> _mockTransactions = new();

    public bool SimulationMode { get; set; } = true;

    public MockBrankasApiClient()
    {
        InitializeMockData();
    }

    private void InitializeMockData()
    {
        // Create mock accounts
        _mockAccounts["ACC001"] = new BankAccount
        {
            AccountId = "ACC001",
            AccountNumber = "1234567890",
            BankName = "UnionBank",
            AccountType = "SAVINGS",
            Balance = 15750.50m
        };

        _mockAccounts["ACC002"] = new BankAccount
        {
            AccountId = "ACC002",
            AccountNumber = "0987654321",
            BankName = "BDO",
            AccountType = "CHECKING",
            Balance = 8320.75m
        };

        // Create mock transactions
        _mockTransactions.AddRange(new[]
        {
            new BankTransaction
            {
                TransactionId = "TXN001",
                Date = DateTime.Now.AddDays(-2),
                Description = "Grocery Store Purchase",
                Amount = -1250.00m,
                Type = "DEBIT",
                Balance = 15750.50m
            },
            new BankTransaction
            {
                TransactionId = "TXN002",
                Date = DateTime.Now.AddDays(-5),
                Description = "Salary Deposit",
                Amount = 12000.00m,
                Type = "CREDIT",
                Balance = 17000.50m
            },
            new BankTransaction
            {
                TransactionId = "TXN003",
                Date = DateTime.Now.AddDays(-7),
                Description = "Utility Bill Payment",
                Amount = -850.00m,
                Type = "DEBIT",
                Balance = 5000.50m
            }
        });
    }

    public async Task<BankAccountBalance> GetBalanceAsync(string accountId)
    {
        // Simulate network delay
        await Task.Delay(_random.Next(500, 1500));

        if (!_mockAccounts.TryGetValue(accountId, out var account))
        {
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        return new BankAccountBalance
        {
            AccountId = accountId,
            AvailableBalance = account.Balance,
            CurrentBalance = account.Balance,
            Currency = "PHP",
            AsOfDate = DateTime.Now
        };
    }

    public async Task<IEnumerable<BankTransaction>> GetTransactionsAsync(string accountId, int limit = 10)
    {
        // Simulate network delay
        await Task.Delay(_random.Next(800, 2000));

        if (!_mockAccounts.ContainsKey(accountId))
        {
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        return _mockTransactions
            .OrderByDescending(t => t.Date)
            .Take(limit)
            .ToList();
    }

    public async Task<TransferResult> TransferFundsAsync(TransferRequest request)
    {
        // Simulate network delay for transaction processing
        await Task.Delay(_random.Next(2000, 4000));

        if (!_mockAccounts.TryGetValue(request.FromAccountId, out var fromAccount))
        {
            return new TransferResult
            {
                Success = false,
                Message = "Source account not found",
                Timestamp = DateTime.Now
            };
        }

        if (fromAccount.Balance < request.Amount)
        {
            return new TransferResult
            {
                Success = false,
                Message = "Insufficient funds",
                Timestamp = DateTime.Now
            };
        }

        // Simulate successful transfer
        fromAccount.Balance -= request.Amount;
        var transactionId = $"TXN{DateTime.Now.Ticks}";

        // Add transaction to history
        _mockTransactions.Add(new BankTransaction
        {
            TransactionId = transactionId,
            Date = DateTime.Now,
            Description = $"Transfer to {request.ToAccountNumber}",
            Amount = -request.Amount,
            Type = "DEBIT",
            Balance = fromAccount.Balance
        });

        return new TransferResult
        {
            Success = true,
            TransactionId = transactionId,
            Message = "Transfer completed successfully",
            Timestamp = DateTime.Now,
            NewBalance = fromAccount.Balance
        };
    }

    public async Task<IEnumerable<BankAccount>> GetLinkedAccountsAsync(int userId)
    {
        // Simulate network delay
        await Task.Delay(_random.Next(500, 1200));

        return _mockAccounts.Values.ToList();
    }

    public async Task<bool> VerifyAccountAsync(string accountId)
    {
        // Simulate network delay
        await Task.Delay(_random.Next(300, 800));

        return _mockAccounts.ContainsKey(accountId);
    }
}
