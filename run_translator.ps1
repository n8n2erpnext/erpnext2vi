# run_translator.ps1
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$csharpSource = Get-Content -Encoding UTF8 -Path "C:\Users\Admin\.gemini\antigravity\scratch\Translator.cs" -Raw

# Compile
Add-Type -TypeDefinition $csharpSource

# Run translators
Write-Host "Running translation corrections with proper UTF8 encoding..."
[PoTranslator]::ProcessErpNext("C:\Users\Admin\.gemini\antigravity\scratch\erpnext_vi.po")
[PoTranslator]::ProcessFrappe("C:\Users\Admin\.gemini\antigravity\scratch\frappe_vi.po")
[PoTranslator]::ProcessHrms("C:\Users\Admin\.gemini\antigravity\scratch\hrms_vi.po")
[PoTranslator]::ProcessCrm("C:\Users\Admin\.gemini\antigravity\scratch\crm_vi.po")
[PoTranslator]::ProcessHelpdesk("C:\Users\Admin\.gemini\antigravity\scratch\helpdesk_vi.po")
[PoTranslator]::ProcessInsights("C:\Users\Admin\.gemini\antigravity\scratch\insights_vi.po")

Write-Host "All translations processed successfully!"
