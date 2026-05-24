using BosesApp.Core.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace BosesApp.Modules.Plugins;

/// <summary>
/// Guardian anti-scam protection plugin
/// Implements verification loop for high-risk transactions
/// </summary>
public class GuardianPlugin
{
    private readonly IUserRepository _userRepository;
    private readonly Dictionary<string, GuardianVerificationRequest> _pendingVerifications = new();

    public GuardianPlugin(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [KernelFunction("check_transaction_risk")]
    [Description("Analyze transaction for potential scam indicators")]
    public async Task<string> CheckTransactionRiskAsync(
        [Description("User ID")] int userId,
        [Description("Transaction amount")] decimal amount,
        [Description("Recipient information")] string recipient,
        [Description("Transaction description")] string description)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return "User not found.";

        var riskScore = CalculateRiskScore(amount, recipient, description);
        var riskLevel = riskScore switch
        {
            >= 80 => "HIGH",
            >= 50 => "MEDIUM",
            _ => "LOW"
        };

        var result = $"Transaction Risk Assessment:\n" +
                    $"Risk Level: {riskLevel}\n" +
                    $"Risk Score: {riskScore}/100\n\n";

        if (riskScore >= 50)
        {
            result += "⚠️ Warning Signs Detected:\n";

            if (amount > 10000)
                result += "• Large transaction amount\n";

            if (description.Contains("urgent", StringComparison.OrdinalIgnoreCase) ||
                description.Contains("emergency", StringComparison.OrdinalIgnoreCase))
                result += "• Urgency pressure detected\n";

            if (recipient.Contains("unknown", StringComparison.OrdinalIgnoreCase))
                result += "• Unknown recipient\n";

            result += "\nRecommendation: Guardian verification required.";
        }
        else
        {
            result += "✓ Transaction appears safe.";
        }

        return result;
    }

    [KernelFunction("request_guardian_verification")]
    [Description("Request guardian approval for high-risk transaction")]
    public async Task<string> RequestGuardianVerificationAsync(
        [Description("User ID")] int userId,
        [Description("Transaction details")] string transactionDetails)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return "User not found.";

        if (string.IsNullOrEmpty(user.GuardianPhoneNumber))
        {
            return "No guardian contact configured. Please set up a guardian in your profile settings.";
        }

        var verificationId = Guid.NewGuid().ToString("N")[..8];
        var request = new GuardianVerificationRequest
        {
            VerificationId = verificationId,
            UserId = userId,
            TransactionDetails = transactionDetails,
            RequestedAt = DateTime.Now,
            Status = "PENDING"
        };

        _pendingVerifications[verificationId] = request;

        // In production, this would send SMS to guardian
        var message = $"Guardian Verification Request:\n" +
                     $"Verification Code: {verificationId}\n" +
                     $"User: {user.FullName}\n" +
                     $"Transaction: {transactionDetails}\n\n" +
                     $"A verification request has been sent to {user.GuardianName} at {user.GuardianPhoneNumber}.\n" +
                     $"Please wait for their approval before proceeding.";

        return message;
    }

    [KernelFunction("verify_guardian_code")]
    [Description("Verify guardian approval code")]
    public Task<string> VerifyGuardianCodeAsync(
        [Description("Verification ID")] string verificationId,
        [Description("Guardian approval code")] string approvalCode)
    {
        if (!_pendingVerifications.TryGetValue(verificationId, out var request))
        {
            return Task.FromResult("Invalid verification ID.");
        }

        if (request.Status != "PENDING")
        {
            return Task.FromResult($"Verification already {request.Status.ToLower()}.");
        }

        // In simulation mode, accept any 4-digit code
        if (approvalCode.Length == 4 && approvalCode.All(char.IsDigit))
        {
            request.Status = "APPROVED";
            request.ApprovedAt = DateTime.Now;
            return Task.FromResult("✓ Guardian verification approved. You may proceed with the transaction.");
        }

        return Task.FromResult("Invalid approval code. Please check with your guardian.");
    }

    [KernelFunction("detect_scam_patterns")]
    [Description("Detect common scam patterns in transaction requests")]
    public Task<string> DetectScamPatternsAsync(
        [Description("Transaction description or message")] string message)
    {
        var scamIndicators = new List<string>();

        var scamKeywords = new[]
        {
            "urgent", "emergency", "immediately", "act now",
            "prize", "winner", "congratulations", "claim",
            "verify account", "suspended", "locked",
            "tax", "penalty", "legal action",
            "investment opportunity", "guaranteed returns"
        };

        foreach (var keyword in scamKeywords)
        {
            if (message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                scamIndicators.Add($"• Suspicious keyword: '{keyword}'");
            }
        }

        if (scamIndicators.Any())
        {
            var result = "⚠️ SCAM WARNING - Potential fraud detected!\n\n" +
                        "Suspicious indicators found:\n" +
                        string.Join("\n", scamIndicators) +
                        "\n\n🛡️ Safety Tips:\n" +
                        "• Never share OTP or verification codes\n" +
                        "• Banks will never ask for your password\n" +
                        "• Verify sender identity before sending money\n" +
                        "• Contact your guardian if unsure";

            return Task.FromResult(result);
        }

        return Task.FromResult("No obvious scam patterns detected. Stay vigilant!");
    }

    private int CalculateRiskScore(decimal amount, string recipient, string description)
    {
        var score = 0;

        // Amount-based risk
        if (amount > 50000) score += 40;
        else if (amount > 10000) score += 25;
        else if (amount > 5000) score += 15;

        // Recipient risk
        if (recipient.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            score += 30;

        // Description risk
        var urgentKeywords = new[] { "urgent", "emergency", "immediately", "now" };
        if (urgentKeywords.Any(k => description.Contains(k, StringComparison.OrdinalIgnoreCase)))
            score += 20;

        return Math.Min(score, 100);
    }

    private class GuardianVerificationRequest
    {
        public string VerificationId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string TransactionDetails { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED
    }
}
