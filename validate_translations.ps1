# validate_translations.ps1
# Script to validate PO files for ERPNext, Frappe, and HRMS translations.
# Runs 5 validation checks:
# 1. Placeholder Validation (e.g. {0}, {name})
# 2. Jinja Syntax Validation (e.g. {{ doc.name }}, {% if comments %})
# 3. JS Template Literal Validation (e.g. ${type})
# 4. Forbidden Literal Translation Detection (e.g. "hàng đợi", "máy chủ")
# 5. gettext msgfmt check (if available in PATH)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$csharpSource = @'
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class PoSuiteValidator
{
    public class PoEntry
    {
        public int LineNumber { get; set; }
        public string MsgId { get; set; }
        public string MsgStr { get; set; }

        public PoEntry()
        {
            MsgId = "";
            MsgStr = "";
        }
    }

    private static string DecodePoString(List<string> rawLines)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var line in rawLines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
            {
                string s = trimmed.Substring(1, trimmed.Length - 2);
                s = s.Replace("\\n", "\n")
                     .Replace("\\t", "\t")
                     .Replace("\\\"", "\"")
                     .Replace("\\\\", "\\");
                sb.Append(s);
            }
        }
        return sb.ToString();
    }

    public static bool ValidateFile(string path)
    {
        Console.WriteLine("\n==================================================");
        Console.WriteLine("VALIDATING: " + Path.GetFileName(path));
        Console.WriteLine("==================================================");

        if (!File.Exists(path))
        {
            Console.WriteLine("File not found: " + path);
            return false;
        }

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        List<PoEntry> entries = new List<PoEntry>();
        PoEntry current = new PoEntry { LineNumber = 1 };
        bool inMsgId = false;
        bool inMsgStr = false;
        List<string> rawMsgId = new List<string>();
        List<string> rawMsgStr = new List<string>();

        int totalLines = lines.Length;
        for (int i = 0; i < totalLines; i++)
        {
            string line = lines[i];
            string trimmed = line.Trim();

            if (trimmed.StartsWith("#"))
            {
                if (inMsgId || inMsgStr)
                {
                    current.MsgId = DecodePoString(rawMsgId);
                    current.MsgStr = DecodePoString(rawMsgStr);
                    entries.Add(current);
                    current = new PoEntry { LineNumber = i + 1 };
                    rawMsgId.Clear();
                    rawMsgStr.Clear();
                    inMsgId = false;
                    inMsgStr = false;
                }
            }
            else if (trimmed.StartsWith("msgid "))
            {
                if (inMsgId || inMsgStr)
                {
                    current.MsgId = DecodePoString(rawMsgId);
                    current.MsgStr = DecodePoString(rawMsgStr);
                    entries.Add(current);
                    current = new PoEntry { LineNumber = i + 1 };
                    rawMsgId.Clear();
                    rawMsgStr.Clear();
                    inMsgStr = false;
                }
                inMsgId = true;
                rawMsgId.Add(line.Substring(6));
            }
            else if (trimmed.StartsWith("msgstr "))
            {
                inMsgId = false;
                inMsgStr = true;
                rawMsgStr.Add(line.Substring(7));
            }
            else if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
            {
                if (inMsgId) rawMsgId.Add(line);
                else if (inMsgStr) rawMsgStr.Add(line);
            }
            else if (trimmed == "")
            {
                if (inMsgId || inMsgStr)
                {
                    current.MsgId = DecodePoString(rawMsgId);
                    current.MsgStr = DecodePoString(rawMsgStr);
                    entries.Add(current);
                    current = new PoEntry { LineNumber = i + 2 };
                    rawMsgId.Clear();
                    rawMsgStr.Clear();
                    inMsgId = false;
                    inMsgStr = false;
                }
            }
        }
        if (rawMsgId.Count > 0 || rawMsgStr.Count > 0)
        {
            current.MsgId = DecodePoString(rawMsgId);
            current.MsgStr = DecodePoString(rawMsgStr);
            entries.Add(current);
        }

        Console.WriteLine(string.Format("Successfully parsed {0} entries.", entries.Count));

        int errors = 0;
        int warnings = 0;

        Regex bracePlaceholder = new Regex(@"\{[0-9]+\}");
        Regex braceNamePlaceholder = new Regex(@"\{[a-zA-Z0-9_]+\}");
        Regex percentPlaceholder = new Regex(@"%[a-zA-Z0-9_().]*[sdfFexXoO%]");
        Regex jinjaTag = new Regex(@"\{\%[^%]+\%\}|\{\{[^}]+\}\}");
        Regex jsTemplateLiteral = new Regex(@"\$\{[^}]+\}");

        // Forbidden terms
        List<string> forbiddenTerms = new List<string> {
            "hàng đợi", "máy chủ", "móc web", "Đảng chung"
        };

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.MsgId)) continue; // skip header
            if (string.IsNullOrEmpty(entry.MsgStr)) continue; // skip untranslated

            string id = entry.MsgId;
            string str = entry.MsgStr;

            // --- CHECK 1: PLACEHOLDERS ---
            var idBraces = GetMatches(id, bracePlaceholder);
            var strBraces = GetMatches(str, bracePlaceholder);
            foreach (var b in idBraces)
            {
                if (!strBraces.Contains(b))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("[ERROR] Line {0}: Missing numeric placeholder '{1}' in translation.", entry.LineNumber, b));
                    Console.ResetColor();
                    errors++;
                }
            }

            var idNameBraces = GetMatches(id, braceNamePlaceholder);
            var strNameBraces = GetMatches(str, braceNamePlaceholder);
            foreach (var b in idNameBraces)
            {
                // If it is a Jinja expression, we validate under Jinja tags check instead
                if (b.StartsWith("{doc.") || b.StartsWith("{row.") || b.StartsWith("{frappe.") || b.StartsWith("{_")) continue;
                if (!strNameBraces.Contains(b))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("[ERROR] Line {0}: Missing named placeholder '{1}' in translation.", entry.LineNumber, b));
                    Console.ResetColor();
                    errors++;
                }
            }

            var idPercents = GetMatches(id, percentPlaceholder);
            var strPercents = GetMatches(str, percentPlaceholder);
            foreach (var p in idPercents)
            {
                if (!strPercents.Contains(p))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(string.Format("[WARNING] Line {0}: Missing percent placeholder '{1}' in translation.", entry.LineNumber, p));
                    Console.ResetColor();
                    warnings++;
                }
            }

            // --- CHECK 2: JINJA TAGS ---
            var idJinja = GetMatches(id, jinjaTag);
            var strJinja = GetMatches(str, jinjaTag);
            foreach (var tag in idJinja)
            {
                // Check if the control flow tags have been translated (e.g. {% if %})
                if (tag.StartsWith("{%") && tag.Contains("if") && !tag.Contains("endif"))
                {
                    bool translatedIf = true;
                    foreach (var sTag in strJinja)
                    {
                        if (sTag.StartsWith("{%") && sTag.Contains("if")) { translatedIf = false; break; }
                    }
                    if (translatedIf)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(string.Format("[ERROR] Line {0}: Jinja control flow tag '{1}' was incorrectly translated.", entry.LineNumber, tag));
                        Console.ResetColor();
                        errors++;
                    }
                }
            }

            // --- CHECK 3: JS TEMPLATE LITERALS ---
            var idJs = GetMatches(id, jsTemplateLiteral);
            var strJs = GetMatches(str, jsTemplateLiteral);
            foreach (var js in idJs)
            {
                if (!strJs.Contains(js))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("[ERROR] Line {0}: JS template literal '{1}' was altered or missing.", entry.LineNumber, js));
                    Console.ResetColor();
                    errors++;
                }
            }

            // --- CHECK 4: FORBIDDEN TERMS ---
            foreach (var term in forbiddenTerms)
            {
                if (str.ToLower().Contains(term.ToLower()))
                {
                    // exception: "sức khỏe" is allowed for human health, but flagged for ledger
                    if (term == "sức khỏe" && !str.Contains("Sức khỏe sổ cái") && !str.Contains("sức khỏe sổ cái")) continue;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(string.Format("[WARNING] Line {0}: Contains forbidden term '{1}'. Prefer English tech term or business standard.", entry.LineNumber, term));
                    Console.ResetColor();
                    warnings++;
                }
            }
        }

        Console.WriteLine(string.Format("Validation Finished. Errors: {0}, Warnings: {1}", errors, warnings));
        return (errors == 0);
    }

    private static HashSet<string> GetMatches(string text, Regex regex)
    {
        HashSet<string> matches = new HashSet<string>();
        foreach (Match match in regex.Matches(text))
        {
            matches.Add(match.Value);
        }
        return matches;
    }
}
'@

Add-Type -TypeDefinition $csharpSource

# Get all PO files in the current folder
$poFiles = Get-ChildItem -Filter *.po

$hasErrors = $false
foreach ($file in $poFiles) {
    $ok = [PoSuiteValidator]::ValidateFile($file.FullName)
    if (-not $ok) {
        $hasErrors = $true
    }
}

# --- CHECK 5: MSGFMT COMPILATION CHECK ---
Write-Host "`n--------------------------------------------------"
Write-Host "RUNNING GETTEXT MSGFMT COMPILE CHECK..."
Write-Host "--------------------------------------------------"

$msgfmtPath = Get-Command msgfmt -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Path
if (-not $msgfmtPath) {
    # Check common Git folder as fallback
    if (Test-Path "C:\Program Files\Git\usr\bin\msgfmt.exe") {
        $msgfmtPath = "C:\Program Files\Git\usr\bin\msgfmt.exe"
    }
}

if ($msgfmtPath) {
    Write-Host "Found msgfmt at: $msgfmtPath"
    foreach ($file in $poFiles) {
        Write-Host "Testing compilation for $($file.Name)..."
        $null = & $msgfmtPath -o "$($file.FullName).mo" --check-format --check-header $file.FullName 2>&1 | Out-String -OutVariable msgfmtOut
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "msgfmt compilation failed for $($file.Name):`n$msgfmtOut"
            $hasErrors = $true
        } else {
            Write-Host "  Success! Compiled to $($file.Name).mo"
            # Cleanup generated binary file
            Remove-Item "$($file.FullName).mo" -ErrorAction SilentlyContinue
        }
    }
} else {
    Write-Warning "msgfmt command not found in PATH or Git folder. Compilation check skipped."
    Write-Host "To install gettext compiler:"
    Write-Host "  Windows: winget install GnuWin32.GetText  (or install Git with MinGW)"
    Write-Host "  Linux/Ubuntu: sudo apt-get install gettext"
    Write-Host "  macOS: brew install gettext"
}

if ($hasErrors) {
    Write-Host "`n[FAIL] Some PO files have validation errors." -ForegroundColor Red
    Exit 1
} else {
    Write-Host "`n[SUCCESS] All PO files passed checks!" -ForegroundColor Green
    Exit 0
}
