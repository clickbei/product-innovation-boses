using BosesApp.Core.Interfaces;
using Microsoft.SemanticKernel;
using System.Text.RegularExpressions;

namespace BosesApp.Core.Services;

/// <summary>
/// AI orchestration service using Semantic Kernel
/// Routes voice commands to appropriate plugins
/// Simulates Google Gemini integration for NLU
/// </summary>
public class AiOrchestratorService : IAiOrchestrator
{
    private Kernel? _kernel;
    private readonly IUserRepository _userRepository;
    private bool _isInitialized;

    public bool SimulationMode { get; set; } = true;

    public AiOrchestratorService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        // Initialize Semantic Kernel
        var builder = Kernel.CreateBuilder();

        // TODO: In production, add Google Gemini or OpenAI connector
        // builder.AddGoogleGeminiChatCompletion("gemini-pro", apiKey);

        _kernel = builder.Build();
        _isInitialized = true;

        await Task.CompletedTask;
    }

    public async Task<string> ProcessCommandAsync(string userInput, int userId)
    {
        if (!_isInitialized)
            await InitializeAsync();

        if (SimulationMode)
        {
            return await ProcessCommandSimulatedAsync(userInput, userId);
        }

        // TODO: Use Semantic Kernel with real AI model
        // var result = await _kernel.InvokePromptAsync(prompt);
        // return result.ToString();

        return await ProcessCommandSimulatedAsync(userInput, userId);
    }

    private async Task<string> ProcessCommandSimulatedAsync(string userInput, int userId)
    {
        // Simulate AI processing delay
        await Task.Delay(new Random().Next(800, 1500));

        var input = userInput.ToLower();

        // Balance inquiry
        if (input.Contains("balance") || input.Contains("balanse") || input.Contains("magkano"))
        {
            return "Ang iyong kasalukuyang balanse ay 15,750 pesos at 50 sentimos. Mayroon ka ring 8,320 pesos sa iyong checking account.";
        }

        // Transfer money
        if (input.Contains("transfer") || input.Contains("ipadala") || input.Contains("magpadala"))
        {
            var amount = ExtractAmount(input);
            if (amount.HasValue)
            {
                return $"Gusto mo bang magpadala ng {amount.Value:N2} pesos? Para sa iyong seguridad, kailangan muna nating i-verify ang iyong boses. Pakisabi ang iyong pangalan.";
            }
            return "Magkano ang gusto mong ipadala?";
        }

        // Transaction history
        if (input.Contains("transaction") || input.Contains("history") || input.Contains("mga binili"))
        {
            return "Narito ang iyong mga kamakailang transaksyon: Grocery store, 1,250 pesos kahapon. Sahod, 12,000 pesos noong Lunes. Kuryente bill, 850 pesos noong isang linggo.";
        }

        // PWD discount
        if (input.Contains("pwd") || input.Contains("discount") || input.Contains("diskwento"))
        {
            return "Ang PWD discount ay 20% para sa mga gamot at 5% para sa iba pang produkto. Gusto mo bang i-calculate ang iyong diskwento?";
        }

        // Help/Unknown
        return "Naiintindihan ko. Mayroon akong mga sumusunod na serbisyo: Tingnan ang balanse, Magpadala ng pera, Tingnan ang mga transaksyon, at Kalkulahin ang PWD discount. Ano ang gusto mong gawin?";
    }

    public async Task<bool> RequiresGuardianVerificationAsync(string command)
    {
        await Task.Delay(200); // Simulate processing

        var input = command.ToLower();

        // High-risk commands that need guardian verification
        var highRiskPatterns = new[]
        {
            "transfer",
            "ipadala",
            "magpadala",
            "withdraw",
            "kunin",
            "loan",
            "utang"
        };

        foreach (var pattern in highRiskPatterns)
        {
            if (input.Contains(pattern))
            {
                var amount = ExtractAmount(input);
                // Require guardian for amounts over 5000 PHP
                return amount.HasValue && amount.Value > 5000;
            }
        }

        return false;
    }

    public async Task<TransactionIntent?> ExtractTransactionIntentAsync(string command)
    {
        await Task.Delay(300); // Simulate NLU processing

        var input = command.ToLower();
        var intent = new TransactionIntent();

        // Detect action
        if (input.Contains("transfer") || input.Contains("ipadala") || input.Contains("magpadala"))
        {
            intent.Action = "TRANSFER";
        }
        else if (input.Contains("withdraw") || input.Contains("kunin"))
        {
            intent.Action = "WITHDRAW";
        }
        else if (input.Contains("balance") || input.Contains("balanse"))
        {
            intent.Action = "BALANCE_INQUIRY";
        }
        else
        {
            return null;
        }

        // Extract amount
        intent.Amount = ExtractAmount(input);

        // Extract recipient (simple pattern matching)
        var recipientMatch = Regex.Match(input, @"(?:kay|to)\s+(\w+)");
        if (recipientMatch.Success)
        {
            intent.Recipient = recipientMatch.Groups[1].Value;
        }

        return intent;
    }

    private decimal? ExtractAmount(string input)
    {
        // Try to extract numeric amount
        var matches = Regex.Matches(input, @"(\d+(?:,\d{3})*(?:\.\d{2})?)");
        if (matches.Count > 0)
        {
            var amountStr = matches[0].Value.Replace(",", "");
            if (decimal.TryParse(amountStr, out var amount))
            {
                return amount;
            }
        }

        return null;
    }
}
