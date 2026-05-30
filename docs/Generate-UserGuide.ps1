# Generate-UserGuide.ps1
# Creates Boses-UserGuide.pdf in the same folder.
# Method: writes a self-contained HTML file then prints it to PDF
# using Microsoft Edge (built into Windows 10/11) — no extra tools needed.
#
# Usage:  cd docs;  .\Generate-UserGuide.ps1

param()
$ErrorActionPreference = "Stop"

$HtmlPath = Join-Path $PSScriptRoot "Boses-UserGuide.html"
$PdfPath  = Join-Path $PSScriptRoot "Boses-UserGuide.pdf"

# ?? HTML content ??????????????????????????????????????????????
$html = @"
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1"/>
<title>Boses - User Guide</title>
<style>
  @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap');

  * { box-sizing: border-box; margin: 0; padding: 0; }

  body {
    font-family: 'Inter', Arial, sans-serif;
    font-size: 13px;
    line-height: 1.6;
    color: #2C3E50;
    background: #fff;
    padding: 0;
  }

  /* ?? Cover page ?? */
  .cover {
    background: linear-gradient(135deg, #1E3A5F 60%, #2980B9 100%);
    color: #fff;
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    text-align: center;
    padding: 60px 40px;
    page-break-after: always;
  }
  .cover .logo { font-size: 64px; margin-bottom: 12px; }
  .cover h1 { font-size: 48px; font-weight: 700; color: #F5A623; margin-bottom: 8px; }
  .cover .tagline { font-size: 20px; color: #BDC3C7; margin-bottom: 32px; }
  .cover .badge {
    background: #1ABC9C;
    color: #fff;
    padding: 8px 22px;
    border-radius: 20px;
    font-size: 13px;
    font-weight: 600;
    margin: 6px;
    display: inline-block;
  }
  .cover .version { margin-top: 48px; font-size: 12px; color: #7F8C8D; }

  /* ?? TOC ?? */
  .toc {
    padding: 60px 60px 40px;
    page-break-after: always;
  }
  .toc h2 { font-size: 28px; font-weight: 700; color: #1E3A5F; margin-bottom: 24px; border-bottom: 3px solid #F5A623; padding-bottom: 8px; }
  .toc ol { counter-reset: toc; list-style: none; }
  .toc ol li { counter-increment: toc; padding: 8px 0; font-size: 15px; border-bottom: 1px dotted #E0E0E0; display: flex; justify-content: space-between; }
  .toc ol li::before { content: counter(toc) "."; color: #F5A623; font-weight: 700; margin-right: 12px; }
  .toc ol li span { color: #7F8C8D; }

  /* ?? Sections ?? */
  .section {
    padding: 50px 60px 40px;
    page-break-before: always;
  }
  .section:first-of-type { page-break-before: avoid; }

  .section-header {
    display: flex;
    align-items: center;
    margin-bottom: 24px;
    padding-bottom: 10px;
    border-bottom: 3px solid #1E3A5F;
  }
  .section-number {
    background: #1E3A5F;
    color: #F5A623;
    font-size: 22px;
    font-weight: 700;
    width: 46px;
    height: 46px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-right: 16px;
    flex-shrink: 0;
  }
  .section-header h2 { font-size: 26px; font-weight: 700; color: #1E3A5F; }

  h3 { font-size: 17px; font-weight: 600; color: #2C3E50; margin: 22px 0 10px; }
  h4 { font-size: 14px; font-weight: 600; color: #2980B9; margin: 14px 0 6px; }
  p  { margin-bottom: 10px; }

  /* ?? Info boxes ?? */
  .info  { background: #EBF5FB; border-left: 4px solid #2980B9; padding: 12px 16px; border-radius: 0 8px 8px 0; margin: 12px 0; }
  .tip   { background: #E8F8F5; border-left: 4px solid #1ABC9C; padding: 12px 16px; border-radius: 0 8px 8px 0; margin: 12px 0; }
  .warn  { background: #FEF9E7; border-left: 4px solid #F5A623; padding: 12px 16px; border-radius: 0 8px 8px 0; margin: 12px 0; }
  .danger{ background: #FDEDEC; border-left: 4px solid #E74C3C; padding: 12px 16px; border-radius: 0 8px 8px 0; margin: 12px 0; }
  .box-title { font-weight: 700; margin-bottom: 4px; }

  /* ?? Command table ?? */
  table { width: 100%; border-collapse: collapse; margin: 14px 0; font-size: 12.5px; }
  th { background: #1E3A5F; color: #F5A623; padding: 10px 14px; text-align: left; font-weight: 600; }
  td { padding: 9px 14px; border-bottom: 1px solid #E8E8E8; vertical-align: top; }
  tr:nth-child(even) td { background: #F8FAFC; }
  .cmd { font-family: monospace; background: #EBF5FB; padding: 3px 8px; border-radius: 4px; font-size: 12px; color: #1E3A5F; white-space: nowrap; }

  /* ?? Buttons visual ?? */
  .btn-row { display: flex; flex-wrap: wrap; gap: 8px; margin: 12px 0; }
  .btn {
    padding: 6px 14px;
    border-radius: 16px;
    font-size: 12px;
    font-weight: 600;
    color: white;
    display: inline-block;
  }
  .btn-blue   { background: #3498DB; }
  .btn-purple { background: #9B59B6; }
  .btn-teal   { background: #1ABC9C; }
  .btn-green  { background: #27AE60; }
  .btn-orange { background: #E67E22; }
  .btn-red    { background: #E74C3C; }
  .btn-dark   { background: #922B21; }
  .btn-gray   { background: #7F8C8D; }

  /* ?? Steps ?? */
  .steps ol { padding-left: 20px; }
  .steps ol li { margin-bottom: 10px; }

  /* ?? Print / PDF ?? */
  @media print {
    .cover  { min-height: auto; padding: 80px 60px; }
    .section { padding: 40px 60px 30px; }
    body { font-size: 12px; }
    @page { margin: 15mm 18mm; size: A4; }
  }
</style>
</head>
<body>

<!-- ============================================================ -->
<!-- COVER                                                         -->
<!-- ============================================================ -->
<div class="cover">
  <div class="logo">??</div>
  <h1>Boses</h1>
  <div class="tagline">Voice-First Banking Assistant for Elderly Filipinos and PWDs</div>
  <div>
    <span class="badge">Tagalog</span>
    <span class="badge">English</span>
    <span class="badge">Zero Screen Required</span>
    <span class="badge">Free to Use</span>
  </div>
  <div style="margin-top:40px; color:#ECF0F1; font-size:15px; max-width:520px;">
    This guide will help you set up Boses, learn every voice command,
    stay safe from scams, and get the most out of hands-free banking.
  </div>
  <div class="version">User Guide · Version 1.0 · 2025</div>
</div>

<!-- ============================================================ -->
<!-- TABLE OF CONTENTS                                             -->
<!-- ============================================================ -->
<div class="toc">
  <h2>Table of Contents</h2>
  <ol>
    <li>What is Boses? <span>2</span></li>
    <li>Getting Started &amp; First Launch <span>3</span></li>
    <li>Understanding the Main Screen <span>4</span></li>
    <li>Speaking to Boses — Voice Commands <span>5</span></li>
    <li>Quick Action Buttons <span>7</span></li>
    <li>Hands-Free Mode <span>8</span></li>
    <li>Guardian Anti-Scam Protection <span>9</span></li>
    <li>Voice Registration &amp; Biometric Auth <span>10</span></li>
    <li>Switching Language (Filipino / English) <span>11</span></li>
    <li>PWD and Senior Citizen Discounts <span>11</span></li>
    <li>Troubleshooting <span>12</span></li>
    <li>Privacy &amp; Security <span>13</span></li>
  </ol>
</div>

<!-- ============================================================ -->
<!-- SECTION 1 — WHAT IS BOSES                                    -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">1</div>
    <h2>What is Boses?</h2>
  </div>
  <p>
    <strong>Boses</strong> (Filipino for <em>Voice</em>) is a free voice-first banking assistant
    designed for elderly Filipinos and Persons with Disabilities (PWDs).
    Instead of navigating complex menus or reading small text, you simply
    <strong>speak naturally</strong> — in Tagalog or English — and Boses responds.
  </p>

  <h3>Key Features</h3>
  <table>
    <tr><th>Feature</th><th>What it does</th></tr>
    <tr><td>??? Voice Commands</td><td>Check balance, transfer money, pay bills — all by speaking</td></tr>
    <tr><td>?? Voice Biometrics</td><td>Your voice is your password — no PINs to remember</td></tr>
    <tr><td>??? Guardian Protection</td><td>Family member is notified before any large transaction</td></tr>
    <tr><td>?? Scam Detection</td><td>AI detects suspicious calls and warns you instantly</td></tr>
    <tr><td>?? Hands-Free Mode</td><td>Continuous voice loop — no screen tapping needed at all</td></tr>
    <tr><td>?? Bilingual</td><td>Switch between Filipino (Tagalog) and English any time</td></tr>
    <tr><td>? Accessibility</td><td>All features usable without reading or fine motor control</td></tr>
    <tr><td>?? Offline Fallback</td><td>Basic voice functions still work without internet</td></tr>
  </table>

  <div class="tip">
    <div class="box-title">?? Who is Boses for?</div>
    Senior citizens (60+), Persons with Disabilities, and anyone who finds
    banking apps difficult to use. No smartphone experience required.
  </div>
</div>

<!-- ============================================================ -->
<!-- SECTION 2 — GETTING STARTED                                  -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">2</div>
    <h2>Getting Started &amp; First Launch</h2>
  </div>

  <h3>Step 1 — Choose Your Language</h3>
  <p>When you open Boses for the first time, you will see a language selection screen.
  Tap <strong>Filipino</strong> or <strong>English</strong> — you can change this later at any time.</p>

  <h3>Step 2 — Onboarding</h3>
  <div class="steps">
  <ol>
    <li>Enter your <strong>full name</strong> and <strong>phone number</strong>.</li>
    <li>Select whether you are a <strong>Senior Citizen</strong>, a <strong>Person with Disability (PWD)</strong>, or both.</li>
    <li>If PWD, select your disability type (visual, hearing, mobility, etc.).</li>
    <li>Optionally enter your <strong>PWD ID</strong> or <strong>Senior Citizen ID</strong> number.</li>
    <li>Add a <strong>Guardian contact</strong> — a trusted family member who will receive scam alerts and high-value transaction approvals.</li>
    <li>Register your voice (see Section 8). You can skip and do this later.</li>
  </ol>
  </div>

  <div class="warn">
    <div class="box-title">?? Microphone Permission</div>
    Boses needs access to your microphone to hear your voice. When prompted, tap <strong>Allow</strong>.
    If you accidentally tapped <em>Deny</em>, go to <strong>Settings ? Privacy ? Microphone</strong> and enable it for Boses.
  </div>

  <h3>Requirements</h3>
  <table>
    <tr><th>Item</th><th>Requirement</th></tr>
    <tr><td>Device</td><td>Windows 10/11 PC, Android phone, iPhone, or iPad</td></tr>
    <tr><td>Internet</td><td>Recommended for best voice quality; works offline with limited features</td></tr>
    <tr><td>Microphone</td><td>Built-in or external microphone</td></tr>
    <tr><td>Cost</td><td>Free</td></tr>
  </table>
</div>

<!-- ============================================================ -->
<!-- SECTION 3 — MAIN SCREEN                                      -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">3</div>
    <h2>Understanding the Main Screen</h2>
  </div>

  <h3>Header Area (Top)</h3>
  <table>
    <tr><th>Element</th><th>Description</th></tr>
    <tr><td>?? Boses</td><td>App title</td></tr>
    <tr><td>Welcome, [Name]</td><td>Shows your registered name</td></tr>
    <tr><td>Status message</td><td>Shows what the app is doing (listening, thinking, speaking…)</td></tr>
    <tr><td>?? Filipino / English button</td><td>Tap to switch language instantly</td></tr>
    <tr><td>SIM MODE pill (orange)</td><td>Visible when the app is in demo/simulation mode</td></tr>
    <tr><td>??? N alerts badge (red)</td><td>Shows how many guardian alerts were sent today</td></tr>
  </table>

  <h3>Quick Action Buttons (Middle)</h3>
  <p>Coloured buttons that send a preset voice command with one tap — no speaking needed.
  See <strong>Section 5</strong> for the full list.</p>

  <h3>Conversation Window</h3>
  <p>Everything you say and every response from Boses appears here as a chat.
  A <span style="color:#E74C3C">? red bar</span> marks your messages;
  a <span style="color:#2ECC71">? green bar</span> marks Boses's replies.</p>

  <h3>Bottom Controls</h3>
  <table>
    <tr><th>Control</th><th>Action</th></tr>
    <tr><td>??? Clear button (left)</td><td>Clears the conversation history</td></tr>
    <tr><td>?? / ?? Big round mic (center)</td><td>Tap once to start listening; tap again to stop and get a reply</td></tr>
    <tr><td>?? Mode button (right)</td><td>Toggles between Simulation (demo) and Live mode</td></tr>
  </table>

  <div class="info">
    <div class="box-title">?? Simulation Mode</div>
    When the orange <strong>SIM MODE</strong> badge is visible, Boses uses simulated banking data.
    This is safe for practising. To use real banking data, tap <strong>?? Mode</strong> to switch to Live mode.
  </div>
</div>

<!-- ============================================================ -->
<!-- SECTION 4 — VOICE COMMANDS                                   -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">4</div>
    <h2>Speaking to Boses — Voice Commands</h2>
  </div>
  <p>Tap the big ?? microphone button, wait for <em>"Listening…"</em>, then speak naturally.
  You do not need to use exact words — Boses understands natural Tagalog and English.</p>

  <h3>?? Balance and Account</h3>
  <table>
    <tr><th>Say this (Tagalog)</th><th>Say this (English)</th></tr>
    <tr><td><span class="cmd">Magkano ang balance ko?</span></td><td><span class="cmd">What is my balance?</span></td></tr>
    <tr><td><span class="cmd">Ano ang laman ng aking account?</span></td><td><span class="cmd">Show my account</span></td></tr>
    <tr><td><span class="cmd">Ipakita ang account information ko</span></td><td><span class="cmd">Account details please</span></td></tr>
  </table>

  <h3>?? Transfer and Send Money</h3>
  <table>
    <tr><th>Say this</th><th>What happens</th></tr>
    <tr><td><span class="cmd">Mag-transfer ng 500 pesos kay Juan</span></td><td>Starts a bank transfer of ?500 to Juan</td></tr>
    <tr><td><span class="cmd">Ipadala ang 1000 pesos kay Maria</span></td><td>Same — sends ?1,000 to Maria</td></tr>
    <tr><td><span class="cmd">Mag-send ng 300 pesos sa GCash</span></td><td>Sends ?300 to your GCash wallet</td></tr>
    <tr><td><span class="cmd">I-send ang 500 pesos sa Maya</span></td><td>Sends ?500 to Maya e-wallet</td></tr>
    <tr><td><span class="cmd">Transfer 1000 pesos to Maria</span></td><td>English equivalent</td></tr>
  </table>

  <div class="warn">
    <div class="box-title">?? Large Transfers (over ?5,000)</div>
    Any transfer above ?5,000 automatically triggers a Guardian verification.
    Your registered guardian will receive a notification and must approve before the transfer proceeds.
  </div>

  <h3>?? Withdraw / ATM</h3>
  <table>
    <tr><th>Say this</th><th>What happens</th></tr>
    <tr><td><span class="cmd">Mag-withdraw ng 2000 pesos</span></td><td>Shows ATM guide and available balance</td></tr>
    <tr><td><span class="cmd">Gusto kong mag-withdraw mula sa ATM</span></td><td>Same without amount — Boses asks how much</td></tr>
    <tr><td><span class="cmd">Cash out ng 1500 pesos</span></td><td>Alternative phrasing</td></tr>
  </table>

  <h3>?? Bill Payment</h3>
  <table>
    <tr><th>Say this</th><th>Biller detected</th></tr>
    <tr><td><span class="cmd">Bayaran ang Meralco bill ng 850 pesos</span></td><td>Meralco (electricity)</td></tr>
    <tr><td><span class="cmd">Mag-bayad ng Maynilad tubig</span></td><td>Maynilad (water)</td></tr>
    <tr><td><span class="cmd">Bayad ng PLDT internet</span></td><td>PLDT (internet)</td></tr>
    <tr><td><span class="cmd">Bayaran ang SSS contribution</span></td><td>SSS</td></tr>
    <tr><td><span class="cmd">PhilHealth bayad</span></td><td>PhilHealth</td></tr>
    <tr><td><span class="cmd">Pag-IBIG contribution ko</span></td><td>Pag-IBIG</td></tr>
  </table>

  <h3>?? Transaction History</h3>
  <table>
    <tr><th>Say this</th><th>What you get</th></tr>
    <tr><td><span class="cmd">Ano ang mga recent transactions ko?</span></td><td>Last 5 transactions</td></tr>
    <tr><td><span class="cmd">Ipakita ang kasaysayan ng transaksyon</span></td><td>Transaction history</td></tr>
    <tr><td><span class="cmd">Show my recent transactions</span></td><td>English equivalent</td></tr>
  </table>

  <h3>?? Loan Inquiry</h3>
  <table>
    <tr><th>Say this</th><th>What you get</th></tr>
    <tr><td><span class="cmd">Magkano ang pwede ko pang i-loan?</span></td><td>Eligible loan products and amounts</td></tr>
    <tr><td><span class="cmd">Gusto kong mag-apply ng personal loan</span></td><td>Personal loan information</td></tr>
    <tr><td><span class="cmd">Pautang naman</span></td><td>Same in casual Tagalog</td></tr>
  </table>

  <h3>?? Emergency — Lost or Stolen Card</h3>
  <table>
    <tr><th>Say this</th><th>Action</th></tr>
    <tr><td><span class="cmd">I-block ang aking card</span></td><td>Emergency card block instructions + hotline</td></tr>
    <tr><td><span class="cmd">Nawala ang aking ATM card</span></td><td>Lost card guidance</td></tr>
    <tr><td><span class="cmd">Ninakaw ang aking account</span></td><td>Stolen account response + security alert</td></tr>
  </table>

  <h3>?? Help and Navigation</h3>
  <table>
    <tr><th>Say this</th><th>Response</th></tr>
    <tr><td><span class="cmd">Tulong</span> / <span class="cmd">Help</span></td><td>Full numbered command menu</td></tr>
    <tr><td><span class="cmd">Ano ang kaya mong gawin?</span></td><td>List of capabilities</td></tr>
    <tr><td><span class="cmd">Kumusta Boses!</span></td><td>Greeting and introduction</td></tr>
    <tr><td><span class="cmd">Kausapin ang agent</span></td><td>Connect to live support</td></tr>
    <tr><td><span class="cmd">Stop</span> / <span class="cmd">Itigil</span></td><td>Stop listening or exit Hands-Free mode</td></tr>
  </table>
</div>

<!-- ============================================================ -->
<!-- SECTION 5 — QUICK ACTION BUTTONS                             -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">5</div>
    <h2>Quick Action Buttons</h2>
  </div>
  <p>These buttons send a preset voice command instantly — no microphone tap needed.
  They are grouped into three rows.</p>

  <h3>Row A — Banking</h3>
  <div class="btn-row">
    <span class="btn btn-blue">?? Balance</span>
    <span class="btn btn-purple">?? Transactions</span>
    <span class="btn btn-teal">?? Transfer</span>
    <span class="btn btn-teal" style="background:#16A085">?? Withdraw</span>
    <span class="btn btn-green">?? GCash</span>
    <span class="btn btn-blue" style="background:#2980B9">?? Bills</span>
    <span class="btn btn-purple" style="background:#8E44AD">?? Loan</span>
  </div>
  <table>
    <tr><th>Button</th><th>Command sent</th></tr>
    <tr><td>?? Balance</td><td><span class="cmd">Magkano ang balance ko?</span></td></tr>
    <tr><td>?? Transactions</td><td><span class="cmd">Ano ang mga recent transactions ko?</span></td></tr>
    <tr><td>?? Transfer</td><td><span class="cmd">Mag-transfer ng 1000 pesos kay Maria</span></td></tr>
    <tr><td>?? Withdraw</td><td><span class="cmd">Mag-withdraw ng 2000 pesos</span></td></tr>
    <tr><td>?? GCash</td><td><span class="cmd">Mag-send ng 500 pesos sa GCash</span></td></tr>
    <tr><td>?? Bills</td><td><span class="cmd">Bayaran ang Meralco bill ng 850 pesos</span></td></tr>
    <tr><td>?? Loan</td><td><span class="cmd">Magkano ang pwede ko pang i-loan?</span></td></tr>
  </table>

  <h3>Row B — Discounts and Emergency</h3>
  <div class="btn-row">
    <span class="btn btn-orange">? PWD Discount</span>
    <span class="btn btn-orange" style="background:#D35400">?? Senior Discount</span>
    <span class="btn btn-red" style="background:#C0392B">?? Block Card</span>
    <span class="btn btn-gray">?? Help</span>
  </div>
  <table>
    <tr><th>Button</th><th>Who sees it</th><th>Command sent</th></tr>
    <tr><td>? PWD Discount</td><td>PWD users only</td><td><span class="cmd">PWD discount para sa 1000 pesos na gamot</span></td></tr>
    <tr><td>?? Senior Discount</td><td>Everyone</td><td><span class="cmd">Senior discount para sa 500 pesos na pagkain</span></td></tr>
    <tr><td>?? Block Card</td><td>Everyone</td><td><span class="cmd">Nawala ang aking ATM card</span></td></tr>
    <tr><td>?? Help</td><td>Everyone</td><td><span class="cmd">Tulong</span></td></tr>
  </table>

  <h3>Row C — Special Features</h3>
  <div class="btn-row">
    <span class="btn btn-red">?? Register Voice</span>
    <span class="btn btn-dark">?? Scam Detection Demo</span>
    <span class="btn btn-gray" style="background:#27AE60">?? Hands-Free: ON/OFF</span>
  </div>
  <table>
    <tr><th>Button</th><th>What it does</th></tr>
    <tr><td>?? Register Voice</td><td>Opens the voice biometric enrollment page</td></tr>
    <tr><td>?? Scam Detection Demo</td><td>Runs a simulated scam call and shows the AI analysis</td></tr>
    <tr><td>?? Hands-Free</td><td>Toggles continuous voice loop — no taps needed (see Section 6)</td></tr>
  </table>
</div>

<!-- ============================================================ -->
<!-- SECTION 6 — HANDS-FREE MODE                                  -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">6</div>
    <h2>Hands-Free Mode</h2>
  </div>
  <p>Hands-Free mode is designed for users who cannot easily tap the screen.
  Once enabled, Boses listens, responds, and restarts automatically — with no screen interaction needed.</p>

  <h3>How to Start</h3>
  <div class="steps">
  <ol>
    <li>Tap the <strong>?? Hands-Free</strong> button. It turns <span style="color:#27AE60">green</span> and the label changes to <strong>?? Hands-Free: ON</strong>.</li>
    <li>Boses says <em>"Hands-free mode on. I'm listening."</em></li>
    <li>The microphone opens automatically. Speak your command.</li>
    <li>Boses processes your command and speaks the reply aloud.</li>
    <li>The microphone reopens automatically for the next command.</li>
  </ol>
  </div>

  <h3>How to Stop</h3>
  <p>Say any of these words out loud:</p>
  <div class="btn-row">
    <span class="cmd">Stop</span>
    <span class="cmd">Itigil</span>
    <span class="cmd">Tigilan</span>
    <span class="cmd">Hinto</span>
    <span class="cmd">Exit</span>
  </div>
  <p>Or tap the <strong>?? Hands-Free</strong> button again.</p>

  <div class="tip">
    <div class="box-title">?? Best for elderly and PWD users</div>
    Place your phone or tablet on a stand, enable Hands-Free mode, and use
    Boses entirely by voice — no touching the screen at all.
    Each listening window is up to 8 seconds long.
  </div>

  <div class="info">
    <div class="box-title">?? Automatic timeout</div>
    If no speech is detected within 8 seconds, Boses restarts the microphone and listens again.
    It will keep doing this until you say "Stop."
  </div>
</div>

<!-- ============================================================ -->
<!-- SECTION 7 — GUARDIAN ANTI-SCAM PROTECTION                   -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">7</div>
    <h2>Guardian Anti-Scam Protection</h2>
  </div>
  <p>Boses automatically protects you against scams in two ways:</p>

  <h3>1. High-Value Transaction Verification</h3>
  <p>Any transfer, withdrawal, or bill payment exceeding <strong>?5,000</strong> triggers a Guardian alert.</p>
  <div class="steps">
  <ol>
    <li>You say a high-value command (e.g., <em>"Mag-transfer ng 10,000 pesos"</em>).</li>
    <li>Boses generates a one-time verification code and sends it to your guardian via Telegram or SMS.</li>
    <li>Your guardian approves or rejects the transaction.</li>
    <li>The transaction only proceeds after guardian approval.</li>
  </ol>
  </div>

  <h3>2. Scam Call Detection</h3>
  <p>Tap <strong>?? Scam Detection Demo</strong> to run the AI scam detector.
  Boses checks for six types of scam patterns:</p>
  <table>
    <tr><th>Scam Type</th><th>Example</th></tr>
    <tr><td>OTP Harvesting</td><td>"Share the OTP sent to your phone"</td></tr>
    <tr><td>Bank Impersonation</td><td>"I am from BDO Customer Service"</td></tr>
    <tr><td>Urgency / Threats</td><td>"Your account will be suspended in 24 hours"</td></tr>
    <tr><td>Prize / Lottery</td><td>"You won ?50,000 — pay a processing fee to claim"</td></tr>
    <tr><td>Remote Access</td><td>"Download AnyDesk so we can fix your phone"</td></tr>
    <tr><td>Credential Phishing</td><td>"Verify your account number and password"</td></tr>
  </table>

  <div class="danger">
    <div class="box-title">?? Remember</div>
    Your real bank will <strong>NEVER</strong> ask for your OTP, password, or PIN over the phone.
    If anyone asks for these, hang up immediately and call your bank's official hotline.
  </div>

  <h3>Viewing Guardian Alerts</h3>
  <p>The <span style="background:#E74C3C; color:white; padding:2px 8px; border-radius:8px; font-size:12px; font-weight:700;">??? N alerts</span> badge in the header shows how many guardian alerts were sent today.
  Your guardian receives each alert as a Telegram message in real time.</p>
</div>

<!-- ============================================================ -->
<!-- SECTION 8 — VOICE REGISTRATION                               -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">8</div>
    <h2>Voice Registration and Biometric Authentication</h2>
  </div>
  <p>Registering your voice lets Boses confirm your identity for sensitive transactions —
  no PIN or password needed.</p>

  <h3>How to Register Your Voice</h3>
  <div class="steps">
  <ol>
    <li>Tap <strong>?? Register Voice</strong> on the main screen.</li>
    <li>Read the phrase shown on screen when prompted. Example: <em>"My voice is my password."</em></li>
    <li>Boses records three samples of your voice.</li>
    <li>A checkmark appears when each sample is accepted.</li>
    <li>After three successful samples, tap <strong>Complete Registration</strong>.</li>
  </ol>
  </div>

  <div class="tip">
    <div class="box-title">?? Tips for best results</div>
    <ul style="padding-left:18px; margin-top:6px;">
      <li>Speak at a normal pace in a quiet room.</li>
      <li>Hold the device 20–30 cm from your mouth.</li>
      <li>Say the full phrase — do not cut words short.</li>
      <li>Repeat up to 3 times if a sample is rejected.</li>
    </ul>
  </div>

  <h3>How Authentication Works</h3>
  <p>When you make a sensitive transaction, Boses asks you to speak a phrase.
  Your voice is compared to your saved voice print using an 85% similarity threshold.
  If it does not match, you are prompted to try again or contact support.</p>

  <div class="info">
    <div class="box-title">?? Voice data stays on your device</div>
    Your voice print is stored only on your device in encrypted form. It is never sent to any server.
  </div>
</div>

<!-- ============================================================ -->
<!-- SECTION 9 — LANGUAGE                                         -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">9</div>
    <h2>Switching Language — Filipino / English</h2>
  </div>
  <p>Boses supports <strong>Filipino (Tagalog)</strong> and <strong>English</strong>.
  You can switch at any time while using the app.</p>

  <h3>How to Switch</h3>
  <p>Tap the <strong>?? Filipino</strong> or <strong>?? English</strong> button in the header.
  The button label shows which language you will <em>switch to</em> (not the current one).</p>
  <ul style="padding-left:20px; margin:10px 0;">
    <li>All button labels update instantly.</li>
    <li>Boses speaks a short confirmation in the new language.</li>
    <li>The microphone starts listening in the new language.</li>
    <li>AI responses are delivered in the selected language.</li>
  </ul>

  <div class="tip">
    <div class="box-title">?? Mixing languages</div>
    You can mix Tagalog and English in the same sentence — Boses understands Taglish.
    Example: <span class="cmd">Check my balance please, salamat</span>
  </div>
</div>

<!-- ============================================================ -->
<!-- SECTION 10 — DISCOUNTS                                       -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">10</div>
    <h2>PWD and Senior Citizen Discounts</h2>
  </div>
  <p>Boses calculates government-mandated discounts for PWDs and Senior Citizens.</p>

  <h3>PWD Discount</h3>
  <table>
    <tr><th>Item</th><th>Discount</th><th>Voice Command</th></tr>
    <tr><td>Medicine / Botika</td><td>20%</td><td><span class="cmd">PWD discount para sa 1000 pesos na gamot</span></td></tr>
    <tr><td>Food / Pagkain</td><td>5%</td><td><span class="cmd">PWD discount para sa 500 pesos</span></td></tr>
    <tr><td>Other items</td><td>5%</td><td><span class="cmd">Kalkulahin ang PWD discount para sa 800 pesos</span></td></tr>
  </table>

  <h3>Senior Citizen Discount</h3>
  <table>
    <tr><th>Item</th><th>Discount</th><th>Voice Command</th></tr>
    <tr><td>Food at restaurant</td><td>20%</td><td><span class="cmd">Senior discount para sa 500 pesos na pagkain</span></td></tr>
    <tr><td>Medicine</td><td>20%</td><td><span class="cmd">Senior discount sa 800 pesos na gamot</span></td></tr>
    <tr><td>Other items</td><td>5%</td><td><span class="cmd">Senior citizen discount para sa 300 pesos</span></td></tr>
  </table>

  <div class="info">
    <div class="box-title">?? Bring your ID</div>
    Remember to show your PWD ID or Senior Citizen ID to the cashier to claim the discount.
    Boses calculates the amount — the cashier applies it.
  </div>
</div>

<!-- ============================================================ -->
<!-- SECTION 11 — TROUBLESHOOTING                                 -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">11</div>
    <h2>Troubleshooting</h2>
  </div>

  <table>
    <tr><th>Problem</th><th>Solution</th></tr>
    <tr>
      <td>Boses does not hear me</td>
      <td>Check that the microphone is not muted. Go to Settings ? Privacy ? Microphone and make sure Boses has permission. Speak closer to the device.</td>
    </tr>
    <tr>
      <td>Voice not recognised correctly</td>
      <td>Speak slowly and clearly. Reduce background noise. Try using the Quick Action buttons instead.</td>
    </tr>
    <tr>
      <td>No sound / Boses does not speak</td>
      <td>Check your device volume. Make sure the speaker is not covered. Restart the app.</td>
    </tr>
    <tr>
      <td>App is stuck on "Thinking…"</td>
      <td>Check your internet connection. Tap the ??? Clear button and try again.</td>
    </tr>
    <tr>
      <td>Guardian alert not received</td>
      <td>Make sure your guardian's Telegram or phone number is correct in your profile. Check their spam folder.</td>
    </tr>
    <tr>
      <td>Voice registration keeps failing</td>
      <td>Speak louder and more clearly. Try in a quieter room. Make sure you say the full phrase shown on screen.</td>
    </tr>
    <tr>
      <td>Buttons show in wrong language</td>
      <td>Tap the ?? language button in the header to toggle back to your preferred language.</td>
    </tr>
    <tr>
      <td>App shows "SIM MODE" badge</td>
      <td>The app is in demo mode. Tap ?? Mode button to switch to Live mode for real banking data.</td>
    </tr>
  </table>

  <h3>Reset the App</h3>
  <p>If Boses behaves unexpectedly, you can reset by deleting the local database:</p>
  <div class="warn">
    <div class="box-title">?? Warning — this deletes your profile</div>
    Resetting will remove your voice registration and profile. You will need to go through onboarding again.
    Only do this if instructed by support.
  </div>
</div>

<!-- ============================================================ -->
<!-- SECTION 12 — PRIVACY AND SECURITY                            -->
<!-- ============================================================ -->
<div class="section">
  <div class="section-header">
    <div class="section-number">12</div>
    <h2>Privacy and Security</h2>
  </div>

  <table>
    <tr><th>Data</th><th>Where stored</th><th>Who can see it</th></tr>
    <tr><td>Your name and phone number</td><td>On your device (encrypted SQLite)</td><td>You only</td></tr>
    <tr><td>Voice biometric print</td><td>On your device only — never uploaded</td><td>You only</td></tr>
    <tr><td>Conversation history</td><td>On your device only</td><td>You only</td></tr>
    <tr><td>Guardian alerts</td><td>Sent via Telegram/SMS to your guardian</td><td>You and your guardian</td></tr>
    <tr><td>Banking data</td><td>Fetched from your bank in real time — not stored</td><td>You only</td></tr>
  </table>

  <h3>Security Tips</h3>
  <ul style="padding-left:20px; margin:10px 0; line-height:2;">
    <li>Never share your voice registration phrase with anyone.</li>
    <li>Do not let others use Boses while logged in as you.</li>
    <li>Your bank will never ask for your OTP or password through any app or call.</li>
    <li>Always verify unexpected guardian alerts by calling your guardian directly.</li>
    <li>Keep the app updated to receive the latest security fixes.</li>
  </ul>

  <div class="tip">
    <div class="box-title">?? Need help?</div>
    For support, contact: <strong>hello@boses.ph</strong><br/>
    For live demos and source code: <strong>github.com/boses</strong>
  </div>

  <div style="margin-top:40px; padding:20px; background:#1E3A5F; border-radius:12px; text-align:center; color:#ECF0F1;">
    <div style="font-size:28px; margin-bottom:8px;">??</div>
    <div style="font-size:18px; font-weight:700; color:#F5A623; margin-bottom:6px;">Boses</div>
    <div style="font-size:13px;">Empowering every Filipino with the power of voice.</div>
    <div style="font-size:11px; color:#7F8C8D; margin-top:12px;">© 2025 Boses · hello@boses.ph · User Guide v1.0</div>
  </div>
</div>

</body>
</html>
"@

# ?? Write HTML ????????????????????????????????????????????????
Set-Content -Path $HtmlPath -Value $html -Encoding UTF8
Write-Host "HTML written: $HtmlPath" -ForegroundColor Cyan

# ?? Convert to PDF via Microsoft Edge ?????????????????????????
$edge = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
if (-not (Test-Path $edge)) {
    $edge = "C:\Program Files\Microsoft\Edge\Application\msedge.exe"
}
if (-not (Test-Path $edge)) {
    Write-Warning "Microsoft Edge not found. Open $HtmlPath in any browser and use File -> Print -> Save as PDF."
    Start-Process $HtmlPath
    exit 0
}

Write-Host "Converting to PDF via Microsoft Edge (headless)..." -ForegroundColor Cyan

$args = @(
    "--headless=old",
    "--disable-gpu",
    "--no-margins",
    "--print-to-pdf=`"$PdfPath`"",
    "`"$HtmlPath`""
)

$proc = Start-Process -FilePath $edge -ArgumentList $args -Wait -PassThru -NoNewWindow
Start-Sleep -Seconds 3

if (Test-Path $PdfPath) {
    $size = [math]::Round((Get-Item $PdfPath).Length / 1KB, 1)
    Write-Host ""
    Write-Host "PDF saved: $PdfPath  ($size KB)" -ForegroundColor Green
    Start-Process $PdfPath
} else {
    Write-Warning "PDF was not created automatically. Opening HTML in browser — use File -> Print -> Save as PDF."
    Start-Process $HtmlPath
}
