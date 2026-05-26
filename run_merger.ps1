# run_merger.ps1
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$csharpSource = Get-Content -Encoding UTF8 -Path "C:\Users\Admin\.gemini\antigravity\scratch\MergePo.cs" -Raw

# Compile
Add-Type -TypeDefinition $csharpSource

Write-Host "Merging POT templates with existing PO translations and custom dictionary..."
[PoMerger]::Merge("C:\Users\Admin\.gemini\antigravity\scratch\erpnext_main.pot", "C:\Users\Admin\.gemini\antigravity\scratch\erpnext_vi.po", "C:\Users\Admin\.gemini\antigravity\scratch\erpnext_vi_v2.po")
[PoMerger]::Merge("C:\Users\Admin\.gemini\antigravity\scratch\frappe_main.pot", "C:\Users\Admin\.gemini\antigravity\scratch\frappe_vi.po", "C:\Users\Admin\.gemini\antigravity\scratch\frappe_vi_v2.po")
[PoMerger]::Merge("C:\Users\Admin\.gemini\antigravity\scratch\hrms_main.pot", "C:\Users\Admin\.gemini\antigravity\scratch\hrms_vi.po", "C:\Users\Admin\.gemini\antigravity\scratch\hrms_vi_v2.po")
[PoMerger]::Merge("C:\Users\Admin\.gemini\antigravity\scratch\crm_main.pot", "C:\Users\Admin\.gemini\antigravity\scratch\crm_vi.po", "C:\Users\Admin\.gemini\antigravity\scratch\crm_vi_v2.po")
[PoMerger]::Merge("C:\Users\Admin\.gemini\antigravity\scratch\insights_main.pot", "C:\Users\Admin\.gemini\antigravity\scratch\insights_vi.po", "C:\Users\Admin\.gemini\antigravity\scratch\insights_vi_v2.po")

function Show-Missing($path, $label) {
    $headers = New-Object System.Collections.Generic.List[string]
    $entries = [PoMerger]::Parse($path, [ref] $headers)
    $missing = 0
    foreach ($e in $entries) {
        if ($e.MsgId -ne "" -and $e.MsgStr -eq "") {
            if ($missing -eq 0) {
                Write-Host "`n=== Missing in $label ==="
            }
            Write-Host " - '$($e.MsgId.Replace("`n", "\n"))'"
            $missing++
        }
    }
}

Show-Missing "C:\Users\Admin\.gemini\antigravity\scratch\erpnext_vi_v2.po" "ERPNext"
Show-Missing "C:\Users\Admin\.gemini\antigravity\scratch\frappe_vi_v2.po" "Frappe"
Show-Missing "C:\Users\Admin\.gemini\antigravity\scratch\hrms_vi_v2.po" "HRMS"

Write-Host "`nAll merges completed!"
