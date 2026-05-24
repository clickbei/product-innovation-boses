using BosesApp.Core.Interfaces;
using BosesApp.Core.Network.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace BosesApp.Modules.Plugins;

/// <summary>
/// Semantic Kernel plugin for banking operations
/// Exposes banking functions to AI orchestrator
/// </summary>
public class BankingPlugin
{
    private readonly IBankApiClient _bankApiClient;
    private readonly IUserRepository _userRepository;

    public BankingPlugin(IBankApiClient bankApiClient, IUserRepository userRepository)
    {
        _bankApiClient = bankApiClient;
        _userRepository = userRepository;
    }

    [KernelFunction("get_account_balance")]
    [Description("Get the current balance of a user's bank account")]
    public async Task<string> GetAccountBalanceAsync(
        [Description("User ID")] int userId,
        [Description("Account ID (optional, uses primary if not specified)")] string? accountId = null)
    {
        try
        {
            // Get user's linked accounts
            var accounts = await _bankApiClient.GetLinkedAccountsAsync(userId);
            var account = string.IsNullOrEmpty(accountId)
                ? accounts.FirstOrDefault()
                : accounts.FirstOrDefault(a => a.AccountId == accountId);

            if (account == null)
                return "No linked bank accounts found.";

            var balance = await _bankApiClient.GetBalanceAsync(account.AccountId);

            return $"Your {account.BankName} {account.AccountType} account (ending in {account.AccountNumber[^4..]}) has a balance of ₱{balance.AvailableBalance:N2} as of {balance.AsOfDate:MMM dd, yyyy h:mm tt}.";
        }
        catch (Exception ex)
        {
            return $"Unable to retrieve balance: {ex.Message}";
        }
    }

    [KernelFunction("get_recent_transactions")]
    [Description("Get recent transaction history for a user's account")]
    public async Task<string> GetRecentTransactionsAsync(
        [Description("User ID")] int userId,
        [Description("Number of transactions to retrieve")] int limit = 5)
    {
        try
        {
            var accounts = await _bankApiClient.GetLinkedAccountsAsync(userId);
            var account = accounts.FirstOrDefault();

            if (account == null)
                return "No linked bank accounts found.";

            var transactions = await _bankApiClient.GetTransactionsAsync(account.AccountId, limit);
            var txnList = transactions.ToList();

            if (!txnList.Any())
                return "No recent transactions found.";

            var result = $"Recent transactions for {account.BankName} account:\n\n";
            foreach (var txn in txnList)
            {
                var sign = txn.Type == "CREDIT" ? "+" : "-";
                result += $"• {txn.Date:MMM dd}: {txn.Description} - {sign}₱{Math.Abs(txn.Amount):N2}\n";
            }

            return result;
        }
        catch (Exception ex)
        {
            return $"Unable to retrieve transactions: {ex.Message}";
        }
    }

    [KernelFunction("transfer_funds")]
    [Description("Transfer money from user's account to another account")]
    public async Task<string> TransferFundsAsync(
        [Description("User ID")] int userId,
        [Description("Amount to transfer")] decimal amount,
        [Description("Recipient account number")] string recipientAccount,
        [Description("Transfer purpose/description")] string purpose = "Fund Transfer")
    {
        try
        {
            var accounts = await _bankApiClient.GetLinkedAccountsAsync(userId);
            var account = accounts.FirstOrDefault();

            if (account == null)
                return "No linked bank accounts found.";

            if (amount <= 0)
                return "Invalid transfer amount.";

            if (account.Balance < amount)
                return $"Insufficient funds. Your available balance is ₱{account.Balance:N2}.";

            var request = new Core.Network.Interfaces.TransferRequest
            {
                FromAccountId = account.AccountId,
                ToAccountNumber = recipientAccount,
                Amount = amount,
                Purpose = purpose
            };

            var result = await _bankApiClient.TransferFundsAsync(request);

            if (result.Success)
            {
                return $"Transfer successful! ₱{amount:N2} has been sent to account {recipientAccount}. Transaction ID: {result.TransactionId}. New balance: ₱{result.NewBalance:N2}.";
            }
            else
            {
                return $"Transfer failed: {result.Message}";
            }
        }
        catch (Exception ex)
        {
            return $"Transfer error: {ex.Message}";
        }
    }

    [KernelFunction("calculate_pwd_discount")]
    [Description("Calculate PWD (Person with Disability) discount for a purchase")]
    public Task<string> CalculatePwdDiscountAsync(
        [Description("Original price")] decimal originalPrice,
        [Description("Product category: medicine, food, or other")] string category = "other")
    {
        if (originalPrice <= 0)
            return Task.FromResult("Invalid price amount.");

        decimal discountRate = category.ToLower() switch
        {
            "medicine" => 0.20m,  // 20% for medicines
            "food" => 0.05m,      // 5% for food
            _ => 0.05m            // 5% for other items
        };

        var discountAmount = originalPrice * discountRate;
        var finalPrice = originalPrice - discountAmount;

        var result = $"PWD Discount Calculation:\n" +
                    $"Original Price: ₱{originalPrice:N2}\n" +
                    $"Discount ({discountRate * 100}%): -₱{discountAmount:N2}\n" +
                    $"Final Price: ₱{finalPrice:N2}";

        return Task.FromResult(result);
    }
}
