# verify_po.ps1
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$csharpSource = @'
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class PoValidator
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

    public static void Validate(string path)
    {
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine("Validating file: " + Path.GetFileName(path));
        
        if (!File.Exists(path))
        {
            Console.WriteLine("File not found: " + path);
            return;
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

        Console.WriteLine(string.Format("Parsed {0} entries.", entries.Count));

        int errors = 0;
        int warnings = 0;
        
        Regex bracePlaceholder = new Regex(@"\{[^}]+\}");
        Regex percentPlaceholder = new Regex(@"%[a-zA-Z0-9_().]*[sdfFexXoO%]");

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.MsgId)) continue;
            if (string.IsNullOrEmpty(entry.MsgStr)) continue;

            var idBraces = GetMatches(entry.MsgId, bracePlaceholder);
            var strBraces = GetMatches(entry.MsgStr, bracePlaceholder);
            
            var idPercents = GetMatches(entry.MsgId, percentPlaceholder);
            var strPercents = GetMatches(entry.MsgStr, percentPlaceholder);

            foreach (var brace in idBraces)
            {
                if (!strBraces.Contains(brace))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("Error on line {0}: Missing curly placeholder '{1}' in translation.", entry.LineNumber, brace));
                    Console.ResetColor();
                    Console.WriteLine("  MsgId : " + entry.MsgId.Replace("\n", "\\n"));
                    Console.WriteLine("  MsgStr: " + entry.MsgStr.Replace("\n", "\\n"));
                    errors++;
                }
            }

            foreach (var pct in idPercents)
            {
                if (!strPercents.Contains(pct))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(string.Format("Warning on line {0}: Potential mismatch for percent placeholder '{1}' in translation.", entry.LineNumber, pct));
                    Console.ResetColor();
                    Console.WriteLine("  MsgId : " + entry.MsgId.Replace("\n", "\\n"));
                    Console.WriteLine("  MsgStr: " + entry.MsgStr.Replace("\n", "\\n"));
                    warnings++;
                }
            }
        }

        Console.WriteLine(string.Format("Validation finished. Errors: {0}, Warnings: {1}", errors, warnings));
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

# Compile C# class
Add-Type -TypeDefinition $csharpSource

# Run validation on all six files
[PoValidator]::Validate("C:\Users\Admin\.gemini\antigravity\scratch\erpnext_vi.po")
[PoValidator]::Validate("C:\Users\Admin\.gemini\antigravity\scratch\frappe_vi.po")
[PoValidator]::Validate("C:\Users\Admin\.gemini\antigravity\scratch\hrms_vi.po")
[PoValidator]::Validate("C:\Users\Admin\.gemini\antigravity\scratch\crm_vi.po")
[PoValidator]::Validate("C:\Users\Admin\.gemini\antigravity\scratch\helpdesk_vi.po")
[PoValidator]::Validate("C:\Users\Admin\.gemini\antigravity\scratch\insights_vi.po")
