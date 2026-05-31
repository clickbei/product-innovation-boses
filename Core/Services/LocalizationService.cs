using BosesApp.Core.Data.Models;
using BosesApp.Core.Interfaces;

namespace BosesApp.Core.Services;

/// <summary>
/// Localization service for English and Tagalog translations
/// </summary>
public class LocalizationService : ILocalizationService
{
    private AppLanguage _currentLanguage = AppLanguage.English;

    // English translations
    private readonly Dictionary<string, string> _englishStrings = new()
    {
        // Language Selection
        ["language_selection_title"] = "Choose Your Language",
        ["language_selection_subtitle"] = "Select your preferred language",
        ["language_english"] = "English",
        ["language_tagalog"] = "Tagalog",
        ["language_continue"] = "Continue",

        // Onboarding - Accessibility Check
        ["onboarding_can_see_title"] = "Can you see this screen?",
        ["onboarding_can_see_yes"] = "Yes, I can see",
        ["onboarding_can_see_no"] = "No, I cannot see",
        ["onboarding_voice_mode_start"] = "I will guide you through setup using only voice. You won't need to press anything. Just speak your answers clearly.",

        // Onboarding - User Type
        ["onboarding_user_type_title"] = "What describes you best?",
        ["onboarding_user_type_senior"] = "Senior Citizen (60+)",
        ["onboarding_user_type_pwd"] = "Person with Disability (PWD)",
        ["onboarding_user_type_both"] = "Both (Senior + PWD)",

        // Onboarding - PWD Category
        ["onboarding_pwd_category_title"] = "What type of disability?",
        ["onboarding_pwd_visual"] = "Visual (Blind/Low Vision)",
        ["onboarding_pwd_hearing"] = "Hearing (Deaf/Hard of Hearing)",
        ["onboarding_pwd_mobility"] = "Mobility (Wheelchair/Crutches)",
        ["onboarding_pwd_cognitive"] = "Cognitive (Intellectual)",
        ["onboarding_pwd_psychosocial"] = "Psychosocial (Mental Health)",
        ["onboarding_pwd_multiple"] = "Multiple Disabilities",

        // Onboarding - Personal Info
        ["onboarding_personal_info_title"] = "Personal Information",
        ["onboarding_name_label"] = "Full Name",
        ["onboarding_name_placeholder"] = "Enter your full name",
        ["onboarding_phone_label"] = "Phone Number",
        ["onboarding_phone_placeholder"] = "09XX XXX XXXX",
        ["onboarding_dob_label"] = "Date of Birth",
        ["onboarding_pwd_id_label"] = "PWD ID Number (Optional)",
        ["onboarding_senior_id_label"] = "Senior Citizen ID (Optional)",

        // Onboarding - Guardian
        ["onboarding_guardian_title"] = "Guardian Information",
        ["onboarding_guardian_subtitle"] = "For your security, add a trusted guardian",
        ["onboarding_guardian_name_label"] = "Guardian Name",
        ["onboarding_guardian_phone_label"] = "Guardian Phone",
        ["onboarding_skip_guardian"] = "Skip for now",

        // Onboarding - Voice Registration
        ["onboarding_voice_title"] = "Voice Registration",
        ["onboarding_voice_subtitle"] = "Register your voice for secure authentication",
        ["onboarding_voice_sample"] = "Voice Sample",
        ["onboarding_voice_instruction"] = "Tap the microphone and say: 'My name is [Your Name]'",
        ["onboarding_voice_recording"] = "Recording...",
        ["onboarding_voice_complete"] = "Complete Registration",

        // Common
        ["button_next"] = "Next",
        ["button_back"] = "Back",
        ["button_finish"] = "Finish",
        ["button_skip"] = "Skip",
        ["button_yes"] = "Yes",
        ["button_no"] = "No",

        // Main App
        ["main_title"] = "Boses - Voice Assistant",
        ["main_listening"] = "Listening... Please speak your command",
        ["main_processing"] = "Processing your request...",
        ["main_tap_to_speak"] = "Tap to speak",

        // Voice Commands
        ["voice_balance_query"] = "What is my balance?",
        ["voice_transfer"] = "Transfer money",
        ["voice_transactions"] = "Show my transactions",
        ["voice_help"] = "Help",

        // Voice Registration Validation Phrases
        ["voice_phrase_1"] = "My voice is my password",
        ["voice_phrase_2"] = "I authorize this transaction",
        ["voice_phrase_3"] = "This is my secure voice",

        // Voice Registration Validation Messages
        ["voice_validation_instruction"] = "Please say: \"{0}\"",
        ["voice_validation_success"] = "✅ Phrase validated! Your voice matches.",
        ["voice_validation_failed"] = "❌ Phrase doesn't match. Please say: \"{0}\"",
        ["voice_validation_retry"] = "Try again and speak clearly",
        ["voice_validation_processing"] = "Validating your speech...",

        // Errors
        ["error_microphone_permission"] = "Microphone permission is required",
        ["error_voice_registration_failed"] = "Voice registration failed. Please try again.",
        ["error_network"] = "Network error. Please check your connection.",
        ["error_generic"] = "An error occurred. Please try again."
    };

    // Tagalog translations
    private readonly Dictionary<string, string> _tagalogStrings = new()
    {
        // Language Selection
        ["language_selection_title"] = "Pumili ng Wika",
        ["language_selection_subtitle"] = "Piliin ang iyong gustong wika",
        ["language_english"] = "Ingles",
        ["language_tagalog"] = "Tagalog",
        ["language_continue"] = "Magpatuloy",

        // Onboarding - Accessibility Check
        ["onboarding_can_see_title"] = "Nakikita mo ba ang screen na ito?",
        ["onboarding_can_see_yes"] = "Oo, nakikita ko",
        ["onboarding_can_see_no"] = "Hindi, hindi ko makita",
        ["onboarding_voice_mode_start"] = "Gagabayan kita gamit lamang ang boses. Hindi mo kailangan pindutin ang kahit ano. Magsalita lang ng malinaw.",

        // Onboarding - User Type
        ["onboarding_user_type_title"] = "Ano ang naglalarawan sa iyo?",
        ["onboarding_user_type_senior"] = "Senior Citizen (60 pataas)",
        ["onboarding_user_type_pwd"] = "Person with Disability (PWD)",
        ["onboarding_user_type_both"] = "Pareho (Senior at PWD)",

        // Onboarding - PWD Category
        ["onboarding_pwd_category_title"] = "Anong uri ng kapansanan?",
        ["onboarding_pwd_visual"] = "Visual (Bulag/Mahina ang Paningin)",
        ["onboarding_pwd_hearing"] = "Pandinig (Bingi/Mahina ang Pandinig)",
        ["onboarding_pwd_mobility"] = "Mobilidad (Wheelchair/Saklay)",
        ["onboarding_pwd_cognitive"] = "Cognitive (Intelektwal)",
        ["onboarding_pwd_psychosocial"] = "Psychosocial (Kalusugan ng Isip)",
        ["onboarding_pwd_multiple"] = "Maraming Kapansanan",

        // Onboarding - Personal Info
        ["onboarding_personal_info_title"] = "Personal na Impormasyon",
        ["onboarding_name_label"] = "Buong Pangalan",
        ["onboarding_name_placeholder"] = "Ilagay ang iyong buong pangalan",
        ["onboarding_phone_label"] = "Numero ng Telepono",
        ["onboarding_phone_placeholder"] = "09XX XXX XXXX",
        ["onboarding_dob_label"] = "Petsa ng Kapanganakan",
        ["onboarding_pwd_id_label"] = "PWD ID Number (Opsyonal)",
        ["onboarding_senior_id_label"] = "Senior Citizen ID (Opsyonal)",

        // Onboarding - Guardian
        ["onboarding_guardian_title"] = "Impormasyon ng Tagapangalaga",
        ["onboarding_guardian_subtitle"] = "Para sa iyong seguridad, magdagdag ng pinagkakatiwalaang tagapangalaga",
        ["onboarding_guardian_name_label"] = "Pangalan ng Tagapangalaga",
        ["onboarding_guardian_phone_label"] = "Telepono ng Tagapangalaga",
        ["onboarding_skip_guardian"] = "Laktawan muna",

        // Onboarding - Voice Registration
        ["onboarding_voice_title"] = "Pagpaparehistro ng Boses",
        ["onboarding_voice_subtitle"] = "Irehistro ang iyong boses para sa secure authentication",
        ["onboarding_voice_sample"] = "Sample ng Boses",
        ["onboarding_voice_instruction"] = "Pindutin ang mikropono at sabihin: 'Ang pangalan ko ay [Iyong Pangalan]'",
        ["onboarding_voice_recording"] = "Nagrerekord...",
        ["onboarding_voice_complete"] = "Kumpletuhin ang Pagpaparehistro",

        // Common
        ["button_next"] = "Susunod",
        ["button_back"] = "Bumalik",
        ["button_finish"] = "Tapusin",
        ["button_skip"] = "Laktawan",
        ["button_yes"] = "Oo",
        ["button_no"] = "Hindi",

        // Main App
        ["main_title"] = "Boses - Voice Assistant",
        ["main_listening"] = "Nakikinig... Magsalita ng iyong utos",
        ["main_processing"] = "Pinoproseso ang iyong kahilingan...",
        ["main_tap_to_speak"] = "Pindutin para magsalita",

        // Voice Commands
        ["voice_balance_query"] = "Magkano ang aking balance?",
        ["voice_transfer"] = "Maglipat ng pera",
        ["voice_transactions"] = "Ipakita ang aking mga transaksyon",
        ["voice_help"] = "Tulong",

        // Voice Registration Validation Phrases
        ["voice_phrase_1"] = "Ang aking boses ay aking password",
        ["voice_phrase_2"] = "Pinahihintulutan ko ang transaksyon na ito",
        ["voice_phrase_3"] = "Ito ang aking secure na boses",

        // Voice Registration Validation Messages
        ["voice_validation_instruction"] = "Pakisabi: \"{0}\"",
        ["voice_validation_success"] = "✅ Na-validate ang parirala! Tumugma ang iyong boses.",
        ["voice_validation_failed"] = "❌ Hindi tumugma ang parirala. Pakisabi: \"{0}\"",
        ["voice_validation_retry"] = "Subukan muli at magsalita nang malinaw",
        ["voice_validation_processing"] = "Vina-validate ang iyong sinabi...",

        // Errors
        ["error_microphone_permission"] = "Kailangan ang pahintulot sa mikropono",
        ["error_voice_registration_failed"] = "Nabigo ang pagpaparehistro ng boses. Subukan ulit.",
        ["error_network"] = "May problema sa network. Suriin ang iyong koneksyon.",
        ["error_generic"] = "May naganap na error. Subukan ulit."
    };

    public AppLanguage CurrentLanguage => _currentLanguage;

    public void SetLanguage(AppLanguage language)
    {
        _currentLanguage = language;
    }

    public string GetString(string key)
    {
        var dictionary = _currentLanguage == AppLanguage.Tagalog ? _tagalogStrings : _englishStrings;
        return dictionary.TryGetValue(key, out var value) ? value : key;
    }

    public string GetString(string key, params object[] args)
    {
        var format = GetString(key);
        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return format;
        }
    }
}
