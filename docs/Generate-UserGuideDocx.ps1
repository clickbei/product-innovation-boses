# Generate-UserGuideDocx.ps1
# Creates Boses-UserGuide.docx in the same folder.
# Pure PowerShell — no Word, no Pandoc, no external tools required.
# Builds a valid Open XML (.docx) package from scratch.
#
# Usage:  cd docs;  .\Generate-UserGuideDocx.ps1

param()
$ErrorActionPreference = "Stop"

$OutFile = Join-Path $PSScriptRoot "Boses-UserGuide.docx"
$tmpDir  = Join-Path $env:TEMP ("BosesDocx_" + [guid]::NewGuid().ToString("N"))

# ?? Create folder structure (PS 5.1 compatible — Join-Path only takes 2 args) ?
$wordDir    = [IO.Path]::Combine($tmpDir, "word")
$relsDir    = [IO.Path]::Combine($tmpDir, "_rels")
$wordRelsDir = [IO.Path]::Combine($wordDir, "_rels")

New-Item -ItemType Directory -Path $relsDir     | Out-Null
New-Item -ItemType Directory -Path $wordDir     | Out-Null
New-Item -ItemType Directory -Path $wordRelsDir | Out-Null

# ?? File path variables ???????????????????????????????????????????????????????
$ctFile      = [IO.Path]::Combine($tmpDir,    "[Content_Types].xml")
$relsFile    = [IO.Path]::Combine($relsDir,   ".rels")
$docRelsFile = [IO.Path]::Combine($wordRelsDir, "document.xml.rels")
$docFile     = [IO.Path]::Combine($wordDir,   "document.xml")
$stylesFile  = [IO.Path]::Combine($wordDir,   "styles.xml")
$numFile     = [IO.Path]::Combine($wordDir,   "numbering.xml")

# ?? [Content_Types].xml ???????????????????????????????????????????????????????
$ctXml = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml"  ContentType="application/xml"/>
  <Override PartName="/word/document.xml"  ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
  <Override PartName="/word/styles.xml"    ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/>
  <Override PartName="/word/numbering.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml"/>
</Types>
'@

# ?? _rels/.rels ???????????????????????????????????????????????????????????????
$relsXml = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
</Relationships>
'@

# ?? word/_rels/document.xml.rels ?????????????????????????????????????????????
$docRelsXml = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"    Target="styles.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/numbering" Target="numbering.xml"/>
</Relationships>
'@

# ?? word/styles.xml ???????????????????????????????????????????????????????????
$stylesXml = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:styles xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
          xmlns:w14="http://schemas.microsoft.com/office/word/2010/wordml">
  <!-- Default paragraph style -->
  <w:style w:type="paragraph" w:default="1" w:styleId="Normal">
    <w:name w:val="Normal"/>
    <w:pPr><w:spacing w:after="160"/></w:pPr>
    <w:rPr><w:sz w:val="24"/><w:szCs w:val="24"/></w:rPr>
  </w:style>
  <!-- Heading 1 -->
  <w:style w:type="paragraph" w:styleId="Heading1">
    <w:name w:val="heading 1"/>
    <w:basedOn w:val="Normal"/>
    <w:pPr>
      <w:numPr><w:ilvl w:val="0"/><w:numId w:val="0"/></w:numPr>
      <w:spacing w:before="480" w:after="160"/>
      <w:jc w:val="left"/>
    </w:pPr>
    <w:rPr>
      <w:b/><w:color w:val="1E3A5F"/>
      <w:sz w:val="40"/><w:szCs w:val="40"/>
    </w:rPr>
  </w:style>
  <!-- Heading 2 -->
  <w:style w:type="paragraph" w:styleId="Heading2">
    <w:name w:val="heading 2"/>
    <w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:before="320" w:after="120"/></w:pPr>
    <w:rPr>
      <w:b/><w:color w:val="2C3E50"/>
      <w:sz w:val="32"/><w:szCs w:val="32"/>
    </w:rPr>
  </w:style>
  <!-- Heading 3 -->
  <w:style w:type="paragraph" w:styleId="Heading3">
    <w:name w:val="heading 3"/>
    <w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:before="240" w:after="80"/></w:pPr>
    <w:rPr>
      <w:b/><w:color w:val="34495E"/>
      <w:sz w:val="28"/><w:szCs w:val="28"/>
    </w:rPr>
  </w:style>
  <!-- ListBullet -->
  <w:style w:type="paragraph" w:styleId="ListBullet">
    <w:name w:val="List Bullet"/>
    <w:basedOn w:val="Normal"/>
    <w:pPr>
      <w:numPr><w:ilvl w:val="0"/><w:numId w:val="1"/></w:numPr>
      <w:ind w:left="720" w:hanging="360"/>
    </w:pPr>
  </w:style>
  <!-- Code / monospace -->
  <w:style w:type="paragraph" w:styleId="Code">
    <w:name w:val="Code"/>
    <w:basedOn w:val="Normal"/>
    <w:pPr>
      <w:shd w:val="clear" w:color="auto" w:fill="F4F4F4"/>
      <w:ind w:left="360"/>
      <w:spacing w:before="80" w:after="80"/>
    </w:pPr>
    <w:rPr>
      <w:rFonts w:ascii="Courier New" w:hAnsi="Courier New"/>
      <w:sz w:val="20"/><w:szCs w:val="20"/>
      <w:color w:val="C0392B"/>
    </w:rPr>
  </w:style>
  <!-- Tip / info box -->
  <w:style w:type="paragraph" w:styleId="Tip">
    <w:name w:val="Tip"/>
    <w:basedOn w:val="Normal"/>
    <w:pPr>
      <w:shd w:val="clear" w:color="auto" w:fill="EAF4FB"/>
      <w:ind w:left="360"/>
      <w:spacing w:before="80" w:after="80"/>
    </w:pPr>
    <w:rPr><w:color w:val="1A5276"/></w:rPr>
  </w:style>
</w:styles>
'@

# ?? word/numbering.xml ????????????????????????????????????????????????????????
$numberingXml = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:numbering xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:abstractNum w:abstractNumId="0">
    <w:multiLevelType w:val="hybridMultilevel"/>
    <w:lvl w:ilvl="0">
      <w:start w:val="1"/>
      <w:numFmt w:val="bullet"/>
      <w:lvlText w:val="&#x2022;"/>
      <w:lvlJc w:val="left"/>
      <w:pPr><w:ind w:left="720" w:hanging="360"/></w:pPr>
    </w:lvl>
  </w:abstractNum>
  <w:num w:numId="1">
    <w:abstractNumId w:val="0"/>
  </w:num>
</w:numbering>
'@

# ?? Helper: wrap plain text as a <w:p> run ????????????????????????????????????
function Para {
    param([string]$text, [string]$style = "Normal", [switch]$bold)
    $bOpen  = if ($bold) { "<w:b/>" } else { "" }
    $escaped = $text -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;' -replace '"','&quot;'
    return "<w:p><w:pPr><w:pStyle w:val=`"$style`"/></w:pPr><w:r><w:rPr>$bOpen</w:rPr><w:t xml:space=`"preserve`">$escaped</w:t></w:r></w:p>"
}

function Bullet { param([string]$text)
    $escaped = $text -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'
    return "<w:p><w:pPr><w:pStyle w:val=`"ListBullet`"/></w:pPr><w:r><w:t xml:space=`"preserve`">$escaped</w:t></w:r></w:p>"
}

function CodeLine { param([string]$text)
    $escaped = $text -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'
    return "<w:p><w:pPr><w:pStyle w:val=`"Code`"/></w:pPr><w:r><w:t xml:space=`"preserve`">$escaped</w:t></w:r></w:p>"
}

function TipLine { param([string]$text)
    $escaped = $text -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'
    return "<w:p><w:pPr><w:pStyle w:val=`"Tip`"/></w:pPr><w:r><w:t xml:space=`"preserve`">$escaped</w:t></w:r></w:p>"
}

function PageBreak {
    return "<w:p><w:r><w:br w:type=`"page`"/></w:r></w:p>"
}

# ?? Build document body ???????????????????????????????????????????????????????
$body = ""

# Cover
$body += Para "Boses" "Heading1" -bold
$body += Para "Voice-First Accessibility Platform" "Heading2"
$body += Para "User Guide — v1.0" "Normal"
$body += Para "For Elderly Filipinos and Persons with Disabilities (PWDs)" "Normal"
$body += PageBreak

# 1. Welcome
$body += Para "1. Welcome to Boses" "Heading1"
$body += Para "Boses (Filipino for 'Voice') is a voice-first digital banking assistant designed specifically for elderly Filipinos and Persons with Disabilities. Simply speak in Tagalog or English — Boses understands you and handles your banking needs safely." "Normal"
$body += Para "Key Benefits:" "Heading3"
$body += Bullet "No typing required — just speak naturally"
$body += Bullet "Works in Tagalog and English"
$body += Bullet "Built-in scam protection for every transaction"
$body += Bullet "Guardian notifications keep your family informed"
$body += Bullet "Hands-free mode for zero-tap operation"
$body += PageBreak

# 2. Getting Started
$body += Para "2. Getting Started" "Heading1"
$body += Para "System Requirements" "Heading2"
$body += Bullet "Windows 10 (build 19041) or Windows 11 — recommended"
$body += Bullet "Android 13+ (API 33) or iOS 16+"
$body += Bullet "Microphone (required for voice commands)"
$body += Bullet "Internet connection (required for AI responses)"

$body += Para "First Launch — Onboarding Steps" "Heading2"
$body += Para "Step 1: Choose Your Language" "Heading3"
$body += Para "Select English or Filipino (Tagalog). You can change this later in Settings." "Normal"
$body += Para "Step 2: Create Your Profile" "Heading3"
$body += Para "Enter your name and contact information. This is stored only on your device." "Normal"
$body += Para "Step 3: Register Your Voice" "Heading3"
$body += Para "Speak 3 sample phrases to enroll your voice biometric. This is used to verify your identity before high-value transactions." "Normal"
$body += TipLine "Tip: Speak clearly and at a normal volume. A quiet room gives the best results."
$body += PageBreak

# 3. Voice Commands
$body += Para "3. Voice Commands" "Heading1"
$body += Para "Tap the green microphone button and speak. Boses understands natural Filipino and English phrases." "Normal"

$body += Para "Balance and Account" "Heading2"
$body += Bullet "'Magkano ang balance ko?' — Check all account balances"
$body += Bullet "'Ano ang laman ng aking account?' — Alternative phrasing"

$body += Para "Transfer and Send Money" "Heading2"
$body += Bullet "'Mag-transfer ng 500 pesos kay Juan' — Bank transfer"
$body += Bullet "'Mag-send ng 300 pesos sa GCash' — GCash e-wallet send"
$body += Bullet "'I-send ang 500 pesos sa Maya' — Maya e-wallet send"

$body += Para "Withdraw / ATM" "Heading2"
$body += Bullet "'Mag-withdraw ng 2000 pesos' — ATM withdrawal guide"
$body += Bullet "'Cash out ng 1500 pesos' — Alternative phrasing"

$body += Para "Bill Payment" "Heading2"
$body += Bullet "'Bayaran ang Meralco bill ng 850 pesos' — Electricity bill"
$body += Bullet "'Mag-bayad ng Maynilad tubig' — Water bill"
$body += Bullet "'Bayad ng PLDT internet' — Internet bill"
$body += Bullet "'Bayaran ang SSS / PhilHealth / Pag-IBIG' — Government contributions"

$body += Para "PWD Discount Calculator" "Heading2"
$body += Bullet "'PWD discount para sa 1000 pesos na gamot' — 20% medicine discount"
$body += Bullet "'PWD discount para sa 500 pesos' — 5% general discount"

$body += Para "Senior Citizen Discount" "Heading2"
$body += Bullet "'Senior discount para sa 400 pesos na pagkain' — 20% food discount"
$body += Bullet "'Senior discount para sa 800 pesos na gamot' — 20% medicine discount"

$body += Para "Other Commands" "Heading2"
$body += Bullet "'Tulong' or 'Help' — Show full command menu"
$body += Bullet "'Kumusta Boses!' — Greeting"
$body += Bullet "'Magkano ang pwede ko pang i-loan?' — Loan inquiry"
$body += Bullet "'Nawala ang aking ATM card' — Emergency card block"
$body += PageBreak

# 4. Quick Action Buttons
$body += Para "4. Quick Action Buttons" "Heading1"
$body += Para "For one-tap access without speaking, use the buttons on the main screen:" "Normal"
$body += Bullet "Balance — Check all account balances"
$body += Bullet "Transactions — View recent transaction history"
$body += Bullet "Transfer — Initiate a fund transfer"
$body += Bullet "Withdraw — ATM withdrawal guidance"
$body += Bullet "GCash — Send money via GCash"
$body += Bullet "Bills — Pay bills"
$body += Bullet "Loan — Loan eligibility inquiry"
$body += Bullet "PWD Discount — Calculate PWD discount (PWD users only)"
$body += Bullet "Senior Discount — Calculate senior citizen discount"
$body += Bullet "Block Card — Emergency card block"
$body += Bullet "Scam Detection Demo — See how scam protection works"
$body += Bullet "Hands-Free — Toggle continuous voice mode"
$body += PageBreak

# 5. Hands-Free Mode
$body += Para "5. Hands-Free Mode" "Heading1"
$body += Para "Hands-Free Mode lets you use Boses without touching the screen — ideal for users with mobility challenges." "Normal"
$body += Para "How to use:" "Heading3"
$body += Bullet "Tap the Hands-Free button to start"
$body += Bullet "Boses listens, responds, and automatically listens again"
$body += Bullet "To stop, say 'Stop', 'Itigil', or 'Hinto'"
$body += TipLine "Tip: The button turns green when Hands-Free Mode is active."
$body += PageBreak

# 6. Guardian Protection
$body += Para "6. Guardian Anti-Scam Protection" "Heading1"
$body += Para "Boses automatically monitors your transactions and alerts your designated guardian for high-risk activities." "Normal"
$body += Para "How it works:" "Heading3"
$body += Bullet "Transactions over PHP 5,000 require guardian approval"
$body += Bullet "Your guardian receives a Telegram or SMS notification"
$body += Bullet "The transaction only proceeds after guardian confirms"
$body += Bullet "Scam messages are automatically detected and flagged"

$body += Para "Scam Detection" "Heading2"
$body += Para "Tap the Scam Detection Demo button to see how Boses identifies:" "Normal"
$body += Bullet "OTP harvesting attempts"
$body += Bullet "Bank impersonation calls"
$body += Bullet "Prize and lottery scams"
$body += Bullet "Remote access requests"
$body += Bullet "Urgency and threat tactics"
$body += TipLine "Remember: Your real bank will NEVER ask for your OTP, password, or card number."
$body += PageBreak

# 7. Discount Calculators
$body += Para "7. Discount Calculators" "Heading1"
$body += Para "PWD Discount (Republic Act 9442)" "Heading2"
$body += Bullet "20% discount on medicine and medical supplies"
$body += Bullet "5% discount on food, clothing, and other products"
$body += Para "Example:" "Heading3"
$body += CodeLine "Say: 'PWD discount para sa 1000 pesos na gamot'"
$body += CodeLine "Result: PHP 1,000 - PHP 200 (20%) = PHP 800"

$body += Para "Senior Citizen Discount (Republic Act 9994)" "Heading2"
$body += Bullet "20% discount on food at restaurants"
$body += Bullet "20% discount on medicine and medical services"
$body += Bullet "5% discount on other goods and services"
$body += Para "Example:" "Heading3"
$body += CodeLine "Say: 'Senior discount para sa 500 pesos na pagkain'"
$body += CodeLine "Result: PHP 500 - PHP 100 (20%) = PHP 400"
$body += TipLine "Reminder: Always present your PWD ID or Senior Citizen ID to the cashier."
$body += PageBreak

# 8. Privacy and Security
$body += Para "8. Privacy and Security" "Heading1"
$body += Bullet "Your name and profile are stored only on your device — encrypted"
$body += Bullet "Your voice biometric is never uploaded to any server"
$body += Bullet "Conversation history stays on your device only"
$body += Bullet "Banking data is fetched in real-time — never stored"
$body += Bullet "Guardian alerts are sent only to your designated contact"

$body += Para "Security Tips" "Heading2"
$body += Bullet "Never share your voice registration phrase with anyone"
$body += Bullet "Do not let others use Boses while logged in as you"
$body += Bullet "Always verify unexpected guardian alerts by calling your guardian directly"
$body += Bullet "Keep the app updated to receive the latest security fixes"
$body += PageBreak

# 9. Troubleshooting
$body += Para "9. Troubleshooting" "Heading1"
$body += Para "Microphone not working" "Heading2"
$body += Bullet "Go to Settings -> Privacy -> Microphone and ensure access is ON"
$body += Bullet "Ensure your default microphone is set in Sound settings"
$body += Bullet "Restart the app and try again"

$body += Para "Boses not understanding my command" "Heading2"
$body += Bullet "Speak clearly and at normal speed"
$body += Bullet "Reduce background noise"
$body += Bullet "Use the Quick Action buttons as an alternative"
$body += Bullet "Try rephrasing — e.g. 'balance' instead of 'pera ko'"

$body += Para "App starts slowly" "Heading2"
$body += Bullet "First launch may take 10-15 seconds while AI initialises"
$body += Bullet "Subsequent launches are faster"

$body += Para "Reset the app" "Heading2"
$body += TipLine "Warning: Resetting deletes your voice registration and profile. Only do this if instructed by support."
$body += CodeLine "Delete: %LOCALAPPDATA%\Boses\boses.db"

$body += Para "Contact Support" "Heading2"
$body += Bullet "Email: hello@boses.ph"
$body += Bullet "For source code and technical docs: github.com/boses"

# ?? Assemble document.xml ?????????????????????????????????????????????????????
$docXml = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:body>
    $body
    <w:sectPr>
      <w:pgSz w:w="12240" w:h="15840"/>
      <w:pgMar w:top="1440" w:right="1440" w:bottom="1440" w:left="1440"/>
    </w:sectPr>
  </w:body>
</w:document>
"@

# ?? Write all package files ???????????????????????????????????????????????????
[IO.File]::WriteAllText($ctFile,      $ctXml,      [System.Text.Encoding]::UTF8)
[IO.File]::WriteAllText($relsFile,    $relsXml,    [System.Text.Encoding]::UTF8)
[IO.File]::WriteAllText($docRelsFile, $docRelsXml, [System.Text.Encoding]::UTF8)
[IO.File]::WriteAllText($docFile,     $docXml,     [System.Text.Encoding]::UTF8)
[IO.File]::WriteAllText($stylesFile,  $stylesXml,  [System.Text.Encoding]::UTF8)
[IO.File]::WriteAllText($numFile,     $numberingXml, [System.Text.Encoding]::UTF8)

# ?? Zip tmp folder into .docx ?????????????????????????????????????????????????
if (Test-Path $OutFile) { Remove-Item $OutFile -Force }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($tmpDir, $OutFile)

# ?? Cleanup and open ??????????????????????????????????????????????????????????
Remove-Item -Recurse -Force $tmpDir

$size = [math]::Round((Get-Item $OutFile).Length / 1KB, 1)
Write-Host ""
Write-Host "DOCX saved: $OutFile  ($size KB)" -ForegroundColor Green

Start-Process $OutFile
