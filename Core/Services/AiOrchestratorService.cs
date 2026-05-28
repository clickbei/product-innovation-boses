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

        // ?? Greeting / Introduction ?????????????????????????????????????????
        if (input.Contains("kumusta") || input.Contains("hello") || input.Contains("hi ") || input == "hi"
            || input.Contains("magandang") || input.Contains("good morning") || input.Contains("good afternoon"))
        {
            return "Kumusta! Ako si Boses, ang iyong voice assistant para sa digital banking at mga serbisyo. " +
                   "Maari akong tumulong sa: balanse, transfer, bayad ng bills, withdrawal, PWD/senior discounts, at scam protection. Ano ang kailangan mo?";
        }

        // ?? Help / Menu ??????????????????????????????????????????????????????
        if (input.Contains("tulong") || input.Contains("help") || input.Contains("menu")
            || input.Contains("ano ang kaya mo") || input.Contains("what can you do"))
        {
            return "Narito ang mga serbisyong maari kong gawin:\n" +
                   "1. Tingnan ang balanse — 'Magkano ang balance ko?'\n" +
                   "2. Mag-transfer ng pera — 'Mag-transfer ng 500 pesos kay Juan'\n" +
                   "3. Mag-withdraw — 'Mag-withdraw ng 2000 pesos'\n" +
                   "4. Tingnan ang transactions — 'Ano ang mga recent transactions ko?'\n" +
                   "5. Bayad ng bills — 'Bayaran ang kuryente bill'\n" +
                   "6. GCash/Maya send — 'Mag-send ng 300 pesos sa GCash'\n" +
                   "7. PWD discount — 'Kalkulahin ang PWD discount para sa 1000 pesos'\n" +
                   "8. Senior discount — 'Senior citizen discount para sa 500 pesos na pagkain'\n" +
                   "9. Loan inquiry — 'Magkano ang pwede ko pang i-loan?'\n" +
                   "10. Scam check — Pindutin ang Scam Detection Demo button";
        }

        // ?? Balance inquiry ??????????????????????????????????????????????????
        if (input.Contains("balance") || input.Contains("balanse") || input.Contains("magkano")
            || input.Contains("laman ng account") || input.Contains("how much"))
        {
            return "Ang iyong kasalukuyang balanse ay:\n" +
                   "• Savings Account: ?15,750.50\n" +
                   "• Checking Account: ?8,320.00\n" +
                   "• GCash Wallet: ?1,240.75\n" +
                   "Kabuuan: ?25,311.25";
        }

        // ?? Send to GCash / Maya / e-wallet ??????????????????????????????????
        if (input.Contains("gcash") || input.Contains("maya") || input.Contains("e-wallet")
            || input.Contains("ewallet") || input.Contains("send money") || input.Contains("mag-send"))
        {
            var amount = ExtractAmount(input);
            var wallet = input.Contains("gcash") ? "GCash" : input.Contains("maya") ? "Maya" : "e-wallet";
            if (amount.HasValue)
                return $"Gusto mo bang mag-send ng ?{amount.Value:N2} sa {wallet}? " +
                       $"I-verify muna ang iyong pagkakakilanlan. Pakisabi ang iyong pangalan.";
            return $"Magkano ang gusto mong i-send sa {wallet}?";
        }

        // ?? Transfer money ????????????????????????????????????????????????????
        if (input.Contains("transfer") || input.Contains("ipadala") || input.Contains("magpadala")
            || input.Contains("mag-transfer") || input.Contains("padala"))
        {
            var amount = ExtractAmount(input);
            var recipientMatch = System.Text.RegularExpressions.Regex.Match(input, @"(?:kay|to|para kay|para sa)\s+(\w+)");
            var recipient = recipientMatch.Success ? recipientMatch.Groups[1].Value : null;

            if (amount.HasValue && recipient != null)
                return $"Gusto mo bang mag-transfer ng ?{amount.Value:N2} kay {recipient}? " +
                       "Para sa iyong seguridad, kailangan muna nating i-verify ang iyong boses. Pakisabi ang iyong pangalan.";
            if (amount.HasValue)
                return $"Gusto mo bang mag-transfer ng ?{amount.Value:N2}? Kanino mo ito ipapadala?";
            return "Magkano ang gusto mong i-transfer at kanino?";
        }

        // ?? Withdraw / ATM ????????????????????????????????????????????????????
        if (input.Contains("withdraw") || input.Contains("mag-withdraw") || input.Contains("kunin")
            || input.Contains("pag-atm") || input.Contains("atm") || input.Contains("cash out")
            || input.Contains("cashout"))
        {
            var amount = ExtractAmount(input);
            if (amount.HasValue)
                return $"Para sa ?{amount.Value:N2} na withdrawal, pumunta sa pinakamalapit na ATM o bangko. " +
                       $"Ang iyong available balance ay ?15,750.50. " +
                       $"Tandaan: May ?15.00 na service fee para sa ibang bangkong ATM.";
            return "Magkano ang gusto mong i-withdraw? Available balance mo ay ?15,750.50.";
        }

        // ?? Bill payment ??????????????????????????????????????????????????????
        if (input.Contains("bayad") || input.Contains("bayaran") || input.Contains("bill")
            || input.Contains("payment") || input.Contains("magbayad") || input.Contains("mag-bayad"))
        {
            string biller;
            if (input.Contains("kuryente") || input.Contains("meralco") || input.Contains("electric"))
                biller = "Meralco (Kuryente)";
            else if (input.Contains("tubig") || input.Contains("water") || input.Contains("maynilad") || input.Contains("mwd"))
                biller = "Maynilad/MWD (Tubig)";
            else if (input.Contains("internet") || input.Contains("wifi") || input.Contains("pldt") || input.Contains("converge"))
                biller = "PLDT/Converge (Internet)";
            else if (input.Contains("phone") || input.Contains("load") || input.Contains("smart") || input.Contains("globe"))
                biller = "Smart/Globe (Phone)";
            else if (input.Contains("sss") || input.Contains("philhealth") || input.Contains("pagibig") || input.Contains("pag-ibig"))
                biller = input.Contains("sss") ? "SSS" : input.Contains("philhealth") ? "PhilHealth" : "Pag-IBIG";
            else
                biller = "bill";

            var amount = ExtractAmount(input);
            if (amount.HasValue)
                return $"Nagbabayad ng ?{amount.Value:N2} para sa {biller}. " +
                       $"Kailangan ng verification. Pakikumpirma: bayad ng ?{amount.Value:N2} sa {biller}?";
            return $"Magkano ang babayaran mo para sa {biller}?";
        }

        // ?? Transaction history ???????????????????????????????????????????????
        if (input.Contains("transaction") || input.Contains("history") || input.Contains("mga binili")
            || input.Contains("kasaysayan") || input.Contains("nakaraang") || input.Contains("last"))
        {
            return "Narito ang iyong 5 pinakabagong transaksyon:\n" +
                   "1. Grocery (SM Supermarket) — -?1,250.00 kahapon\n" +
                   "2. Sahod (Company Payroll) — +?12,000.00 Lunes\n" +
                   "3. Meralco Bill — -?850.00 nakaraang linggo\n" +
                   "4. GCash Send (Juan) — -?500.00 2 araw na nakalipas\n" +
                   "5. ATM Withdrawal — -?2,000.00 3 araw na nakalipas";
        }

        // ?? PWD discount ??????????????????????????????????????????????????????
        if (input.Contains("pwd") || (input.Contains("discount") && !input.Contains("senior"))
            || input.Contains("diskwento") || input.Contains("person with disability"))
        {
            var amount = ExtractAmount(input);
            if (amount.HasValue)
            {
                var isFood = input.Contains("pagkain") || input.Contains("food") || input.Contains("restaurant");
                var isMed  = input.Contains("gamot") || input.Contains("medicine") || input.Contains("botika");
                var rate   = isMed ? 0.20m : 0.05m;
                var label  = isMed ? "20% (gamot)" : isFood ? "5% (pagkain)" : "5%";
                var disc   = amount.Value * rate;
                return $"PWD Discount Calculation:\n" +
                       $"Original: ?{amount.Value:N2}\n" +
                       $"Discount ({label}): -?{disc:N2}\n" +
                       $"Babayaran mo: ?{amount.Value - disc:N2}";
            }
            return "Ang PWD discount ay 20% para sa mga gamot at 5% para sa pagkain at ibang produkto. " +
                   "Sabihin ang halaga para kalkulahin — halimbawa: 'PWD discount para sa 1000 pesos na gamot'.";
        }

        // ?? Senior citizen discount ???????????????????????????????????????????
        if (input.Contains("senior") || input.Contains("matanda") || input.Contains("lolo") || input.Contains("lola"))
        {
            var amount = ExtractAmount(input);
            if (amount.HasValue)
            {
                var isMed  = input.Contains("gamot") || input.Contains("medicine") || input.Contains("botika");
                var isFood = input.Contains("pagkain") || input.Contains("food") || input.Contains("restaurant");
                // Senior citizen: 20% on medicine, food, medical/dental services; 5% elsewhere
                var rate   = (isMed || isFood) ? 0.20m : 0.05m;
                var label  = (isMed || isFood) ? "20%" : "5%";
                var disc   = amount.Value * rate;
                return $"Senior Citizen Discount Calculation:\n" +
                       $"Original: ?{amount.Value:N2}\n" +
                       $"Discount ({label}): -?{disc:N2}\n" +
                       $"Babayaran mo: ?{amount.Value - disc:N2}\n" +
                       "Tandaan: Ipakita ang iyong Senior Citizen ID sa kahera.";
            }
            return "Ang Senior Citizen discount ay 20% sa pagkain sa restaurant, gamot, at medikal na serbisyo. " +
                   "Sabihin ang halaga — halimbawa: 'Senior discount para sa 500 pesos na pagkain'.";
        }

        // ?? Loan inquiry ??????????????????????????????????????????????????????
        if (input.Contains("loan") || input.Contains("utang") || input.Contains("pautang")
            || input.Contains("mag-loan") || input.Contains("borrow") || input.Contains("credit"))
        {
            return "Batay sa iyong account standing, eligible ka para sa:\n" +
                   "• Personal Loan: hanggang ?50,000 sa 12–24 buwan\n" +
                   "• Salary Loan: hanggang ?30,000 (kung nakakonekta ang payroll)\n" +
                   "• Pag-IBIG Multi-Purpose Loan: hanggang ?80,000\n" +
                   "Pumunta sa pinakamalapit na sangay o mag-apply sa banking app. " +
                   "Kailangan ng valid ID at proof of income.";
        }

        // ?? Account / Profile inquiry ?????????????????????????????????????????
        if (input.Contains("account number") || input.Contains("account ko") || input.Contains("aking account")
            || input.Contains("profile") || input.Contains("impormasyon"))
        {
            return "Para sa seguridad, hindi ko ibabahagi ang buong account number sa boses. " +
                   "Ang iyong savings account ay nagtatapos sa ...4521. " +
                   "Para sa kumpletong impormasyon, bisitahin ang bangko nang personal na may valid ID.";
        }

        // ?? Interest rates / products ?????????????????????????????????????????
        if (input.Contains("interest") || input.Contains("rate") || input.Contains("interest rate")
            || input.Contains("savings rate"))
        {
            return "Kasalukuyang interest rates:\n" +
                   "• Regular Savings: 0.10% per annum\n" +
                   "• Time Deposit (1 taon): 3.50% per annum\n" +
                   "• Time Deposit (5 taon): 5.25% per annum\n" +
                   "Para sa mas mataas na interest, alamin ang aming Time Deposit at UITF products.";
        }

        // ?? Nearest branch / ATM ??????????????????????????????????????????????
        if (input.Contains("branch") || input.Contains("sangay") || input.Contains("saan") && input.Contains("bangko")
            || input.Contains("nearest atm") || input.Contains("pinakamalapit"))
        {
            return "Para mahanap ang pinakamalapit na sangay o ATM:\n" +
                   "• Gamitin ang aming banking app o website\n" +
                   "• I-text ang 'BRANCH [LUGAR]' sa aming hotline\n" +
                   "• Tawagan ang customer service: 1800-10-BOSES (26737)";
        }

        // ?? Emergency / block card / report ??????????????????????????????????
        if (input.Contains("block") || input.Contains("emergency") || input.Contains("nawala") || input.Contains("lost")
            || input.Contains("ninakaw") || input.Contains("stolen") || input.Contains("i-report"))
        {
            return "?? EMERGENCY — Para agad na i-block ang iyong card o account:\n" +
                   "• Tawagan ang 24/7 hotline: 1800-10-BOSES (26737)\n" +
                   "• I-freeze ang account sa aming app\n" +
                   "• Pumunta sa pinakamalapit na sangay na may valid ID\n" +
                   "Ginawa ko na ang paunang security alert sa iyong account.";
        }

        // ?? Greet agent / live support ????????????????????????????????????????
        if (input.Contains("agent") || input.Contains("tao") || input.Contains("representative")
            || input.Contains("customer service") || input.Contains("live support"))
        {
            return "Iko-konekta kita sa isang live agent. Pakihintay... " +
                   "Ang average waiting time ay 3–5 minuto. " +
                   "Maaari ka ring tumawag sa 1800-10-BOSES (26737) para sa mas mabilis na serbisyo.";
        }

        // ?? Unknown / fallback ????????????????????????????????????????????????
        return "Paumanhin, hindi ko lubos na naunawaan. Subukan mo itong sabihin:\n" +
               "• 'Balance ko' — para sa balanse\n" +
               "• 'Mag-transfer ng pera' — para sa transfer\n" +
               "• 'Bayad ng bills' — para sa bill payment\n" +
               "• 'Mag-withdraw' — para sa ATM/withdrawal\n" +
               "• 'Tulong' — para sa kumpletong listahan ng mga utos\n" +
               "O pindutin ang isang Quick Action button sa ibaba.";
    }


    public async Task<bool> RequiresGuardianVerificationAsync(string command)
    {
        await Task.Delay(200); // Simulate processing

        var input = command.ToLower();

        // High-risk command keywords
        var highRiskPatterns = new[]
        {
            "transfer", "ipadala", "magpadala", "mag-transfer", "padala",
            "withdraw", "mag-withdraw", "kunin", "cashout", "cash out",
            "loan", "utang", "pautang", "mag-loan",
            "gcash", "maya", "send money", "mag-send",
            "bayad", "bayaran", "magbayad", "mag-bayad"
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

        // Detect action — ordered most-specific first
        if (input.Contains("gcash") || input.Contains("maya") || input.Contains("send money") || input.Contains("mag-send"))
            intent.Action = "EWALLET_SEND";
        else if (input.Contains("transfer") || input.Contains("ipadala") || input.Contains("magpadala")
                 || input.Contains("mag-transfer") || input.Contains("padala"))
            intent.Action = "TRANSFER";
        else if (input.Contains("withdraw") || input.Contains("mag-withdraw") || input.Contains("kunin")
                 || input.Contains("cashout") || input.Contains("atm"))
            intent.Action = "WITHDRAW";
        else if (input.Contains("balance") || input.Contains("balanse") || input.Contains("magkano")
                 || input.Contains("laman ng account"))
            intent.Action = "BALANCE_INQUIRY";
        else if (input.Contains("transaction") || input.Contains("history") || input.Contains("mga binili")
                 || input.Contains("kasaysayan") || input.Contains("nakaraang"))
            intent.Action = "TRANSACTION_HISTORY";
        else if (input.Contains("bayad") || input.Contains("bayaran") || input.Contains("bill")
                 || input.Contains("magbayad") || input.Contains("mag-bayad"))
            intent.Action = "BILL_PAYMENT";
        else if (input.Contains("pwd") || input.Contains("diskwento") || input.Contains("person with disability")
                 || (input.Contains("discount") && !input.Contains("senior")))
            intent.Action = "PWD_DISCOUNT";
        else if (input.Contains("senior") || input.Contains("matanda") || input.Contains("lolo") || input.Contains("lola"))
            intent.Action = "SENIOR_DISCOUNT";
        else if (input.Contains("loan") || input.Contains("utang") || input.Contains("pautang") || input.Contains("credit"))
            intent.Action = "LOAN_INQUIRY";
        else if (input.Contains("block") || input.Contains("nawala") || input.Contains("ninakaw") || input.Contains("lost") || input.Contains("stolen"))
            intent.Action = "EMERGENCY_BLOCK";
        else if (input.Contains("tulong") || input.Contains("help") || input.Contains("menu"))
            intent.Action = "HELP";
        else if (input.Contains("kumusta") || input.Contains("hello") || input.Contains("magandang"))
            intent.Action = "GREETING";
        else
            return null;

        // Extract amount
        intent.Amount = ExtractAmount(input);

        // Extract recipient
        var recipientMatch = Regex.Match(input, @"(?:kay|to|para kay|para sa)\s+(\w+)");
        if (recipientMatch.Success)
            intent.Recipient = recipientMatch.Groups[1].Value;

        // Extract biller name for bill payments
        if (intent.Action == "BILL_PAYMENT")
        {
            if (input.Contains("meralco") || input.Contains("kuryente")) intent.Parameters["biller"] = "Meralco";
            else if (input.Contains("maynilad") || input.Contains("tubig")) intent.Parameters["biller"] = "Maynilad";
            else if (input.Contains("pldt") || input.Contains("internet")) intent.Parameters["biller"] = "PLDT";
            else if (input.Contains("sss")) intent.Parameters["biller"] = "SSS";
            else if (input.Contains("philhealth")) intent.Parameters["biller"] = "PhilHealth";
            else if (input.Contains("pag-ibig") || input.Contains("pagibig")) intent.Parameters["biller"] = "Pag-IBIG";
        }

        // Extract e-wallet type
        if (intent.Action == "EWALLET_SEND")
        {
            if (input.Contains("gcash")) intent.Parameters["wallet"] = "GCash";
            else if (input.Contains("maya")) intent.Parameters["wallet"] = "Maya";
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

    /// <inheritdoc/>
    public async Task<ScamDetectionResult> SimulateScamDetectionAsync(string message)
    {
        // Simulate NLU processing delay
        await Task.Delay(new Random().Next(600, 1200));

        var input = message.ToLower();
        var redFlags = new List<string>();

        // --- OTP / verification code harvesting ---
        if (input.Contains("otp") || input.Contains("one-time") || input.Contains("verification code")
            || input.Contains("pin") && (input.Contains("ibigay") || input.Contains("share") || input.Contains("send")))
        {
            if (input.Contains("ibigay") || input.Contains("share") || input.Contains("send")
                || input.Contains("tell") || input.Contains("sabihin"))
                redFlags.Add("Asking you to share OTP/PIN");
        }

        // --- Impersonating a bank agent ---
        if ((input.Contains("bank") || input.Contains("bdo") || input.Contains("bpi")
             || input.Contains("gcash") || input.Contains("maya"))
            && (input.Contains("agent") || input.Contains("representative") || input.Contains("customer service")))
            redFlags.Add("Claiming to be a bank/e-wallet agent");

        // --- Urgency / account suspension threats ---
        if (input.Contains("suspend") || input.Contains("blocked") || input.Contains("naka-block")
            || input.Contains("naka-suspend") || input.Contains("agad") || input.Contains("immediately")
            || input.Contains("within 24 hours") || input.Contains("bago mabura"))
            redFlags.Add("Creating artificial urgency or threat");

        // --- Prize / lottery scam ---
        if ((input.Contains("nanalo") || input.Contains("won") || input.Contains("winner") || input.Contains("prize"))
            && (input.Contains("claim") || input.Contains("kunin") || input.Contains("bayad") || input.Contains("fee")))
            redFlags.Add("Prize/lottery requiring upfront payment");

        // --- Remote access / app install ---
        if (input.Contains("i-download") || input.Contains("install") || input.Contains("anydesk")
            || input.Contains("teamviewer") || input.Contains("remote"))
            redFlags.Add("Requesting remote access to device");

        // --- Account verification fishing ---
        if ((input.Contains("verify") || input.Contains("i-verify") || input.Contains("confirm"))
            && (input.Contains("account number") || input.Contains("card number") || input.Contains("cvv")
                || input.Contains("expiry") || input.Contains("password")))
            redFlags.Add("Phishing for account/card credentials");

        // --- Determine result ---
        var isScam = redFlags.Count > 0;
        var confidence = isScam ? Math.Min(60 + redFlags.Count * 13, 99) : new Random().Next(2, 12);

        string category, explanation, action;

        if (!isScam)
        {
            category    = "None";
            explanation = "No scam signals detected in this message.";
            action      = "You may proceed normally.";
        }
        else if (redFlags.Any(r => r.Contains("OTP") || r.Contains("PIN")))
        {
            category    = "OTP Harvesting";
            explanation = "This caller is trying to steal your one-time password to take over your account.";
            action      = "Hang up immediately. Your bank will NEVER ask for your OTP.";
        }
        else if (redFlags.Any(r => r.Contains("bank") || r.Contains("agent")))
        {
            category    = "Bank Impersonation";
            explanation = "The caller is pretending to be a bank representative to gain your trust.";
            action      = "Hang up and call your bank's official hotline to verify.";
        }
        else if (redFlags.Any(r => r.Contains("Prize")))
        {
            category    = "Prize / Lottery Scam";
            explanation = "You did not join any contest. Scammers invent prizes to trick you into paying fees.";
            action      = "Do not pay anything. Block and report the number.";
        }
        else if (redFlags.Any(r => r.Contains("remote")))
        {
            category    = "Remote Access Scam";
            explanation = "Scammer wants to see or control your phone to steal your accounts.";
            action      = "Never install apps from unknown callers. Hang up now.";
        }
        else
        {
            category    = "Credential Phishing";
            explanation = "This message is attempting to extract sensitive account information from you.";
            action      = "Never share your account number, card details, or passwords with anyone.";
        }

        return new ScamDetectionResult
        {
            IsScam             = isScam,
            Category           = category,
            ConfidencePercent  = confidence,
            Explanation        = explanation,
            RecommendedAction  = action,
            RedFlags           = redFlags
        };
    }
}

