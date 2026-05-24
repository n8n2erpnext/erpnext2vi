using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class PoTranslator
{
    public class PoEntry
    {
        public int LineNumber { get; set; }
        public List<string> Comments { get; set; }
        public string MsgId { get; set; }
        public string MsgStr { get; set; }

        public PoEntry()
        {
            Comments = new List<string>();
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

    private static string Escape(string s)
    {
        return s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                .Replace("\r", "\\r");
    }

    private static void WriteMultiline(StreamWriter sw, string prefix, string text)
    {
        if (text == "")
        {
            sw.WriteLine(prefix + " \"\"");
            return;
        }

        if (text.Contains("\n"))
        {
            sw.WriteLine(prefix + " \"\"");
            List<string> parts = new List<string>();
            int start = 0;
            int index;
            while ((index = text.IndexOf('\n', start)) != -1)
            {
                parts.Add(text.Substring(start, index - start + 1));
                start = index + 1;
            }
            if (start < text.Length)
            {
                parts.Add(text.Substring(start));
            }

            foreach (var part in parts)
            {
                sw.WriteLine("\"" + Escape(part) + "\"");
            }
        }
        else
        {
            sw.WriteLine(prefix + " \"" + Escape(text) + "\"");
        }
    }

    public static List<PoEntry> Parse(string path, out List<string> headerLines)
    {
        headerLines = new List<string>();
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

            // If we haven't seen msgid yet, everything is header (project info, versions, etc.)
            if (entries.Count == 0 && !trimmed.StartsWith("msgid ") && !trimmed.StartsWith("#") && !trimmed.StartsWith("\"") && trimmed != "")
            {
                headerLines.Add(line);
                continue;
            }
            if (entries.Count == 0 && headerLines.Count > 0 && !trimmed.StartsWith("msgid ") && !trimmed.StartsWith("#"))
            {
                headerLines.Add(line);
                continue;
            }

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
                current.Comments.Add(line);
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

        return entries;
    }

    public static void Save(string path, List<PoEntry> entries, List<string> headerLines)
    {
        using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
        {
            foreach (var line in headerLines)
            {
                sw.WriteLine(line);
            }
            foreach (var entry in entries)
            {
                foreach (var comment in entry.Comments)
                {
                    sw.WriteLine(comment);
                }
                WriteMultiline(sw, "msgid", entry.MsgId);
                WriteMultiline(sw, "msgstr", entry.MsgStr);
                sw.WriteLine();
            }
        }
    }

    public static void ProcessErpNext(string path)
    {
        List<string> headers;
        List<PoEntry> entries = Parse(path, out headers);
        Console.WriteLine("Loaded " + entries.Count + " entries from erpnext_vi.po");

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.MsgId)) continue;

            // 1. Fix line 21 negative stock message
            if (entry.MsgId.Contains("The Batch {0} of an item {1} has negative stock"))
            {
                entry.MsgStr = "\n\t\t\tLô {0} của mặt hàng {1} đang bị âm kho trong kho {2}{3}.\n\t\t\tVui lòng bổ sung số lượng tồn kho là {4} để tiếp tục thực hiện bút toán này.\n\t\t\tNếu không thể tạo bút toán điều chỉnh, vui lòng bật 'Cho phép âm kho theo Lô' trong Cấu hình Kho để tiếp tục.\n\t\t\tTuy nhiên, việc bật cấu hình này có thể dẫn đến âm kho trong hệ thống.\n\t\t\tVì vậy, hãy đảm bảo số lượng tồn kho được điều chỉnh sớm nhất có thể để duy trì tỷ giá định giá chính xác.";
                continue;
            }

            // 2. Queue replacements
            if (entry.MsgStr.Contains("hàng đợi") || entry.MsgStr.Contains("Hàng đợi"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("được xếp hàng đợi", "được xếp vào queue")
                    .Replace("đưa vào hàng đợi", "đưa vào queue")
                    .Replace("xếp hàng đợi", "đưa vào queue")
                    .Replace("hàng đợi", "queue")
                    .Replace("Hàng đợi", "Queue");
            }

            // 3. Server replacements
            if (entry.MsgStr.Contains("máy chủ") || entry.MsgStr.Contains("Máy chủ"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("máy chủ", "server")
                    .Replace("Máy chủ", "Server");
            }

            // 4. Plaid warning / dashboard
            if (entry.MsgId.Contains("There was an issue connecting to Plaid's authentication server"))
            {
                entry.MsgStr = "Đã xảy ra sự cố khi kết nối với server xác thực của Plaid. Kiểm tra console trình duyệt để biết thêm thông tin";
            }
            if (entry.MsgStr.Contains("Bảng điều khiển nhà máy"))
            {
                entry.MsgStr = entry.MsgStr.Replace("Bảng điều khiển nhà máy", "Dashboard nhà máy");
            }
            if (entry.MsgStr.Contains("Bảng điều khiển trạm làm việc"))
            {
                entry.MsgStr = entry.MsgStr.Replace("Bảng điều khiển trạm làm việc", "Dashboard máy trạm");
            }

            // 5. Sức khỏe -> Trạng thái
            if (entry.MsgStr.Contains("sức khỏe") || entry.MsgStr.Contains("Sức khỏe"))
            {
                if (entry.MsgStr.Contains("Sức khỏe sổ cái"))
                    entry.MsgStr = entry.MsgStr.Replace("Sức khỏe sổ cái", "Trạng thái sổ cái");
                if (entry.MsgStr.Contains("sức khỏe sổ cái"))
                    entry.MsgStr = entry.MsgStr.Replace("sức khỏe sổ cái", "trạng thái sổ cái");
                if (entry.MsgStr.Contains("Giám sát Sức khỏe"))
                    entry.MsgStr = entry.MsgStr.Replace("Giám sát Sức khỏe", "Giám sát Trạng thái");
                if (entry.MsgStr.Contains("giám sát sức khỏe"))
                    entry.MsgStr = entry.MsgStr.Replace("giám sát sức khỏe", "giám sát trạng thái");
            }

            // 6. Common Party -> Đối tác chung
            if (entry.MsgStr.Contains("Đảng chung"))
            {
                entry.MsgStr = entry.MsgStr.Replace("Đảng chung", "Đối tác chung");
            }

            // 7. Subcontracting -> Gia công / Gia công ngoài
            if (entry.MsgStr.Contains("ký gửi") || entry.MsgStr.Contains("Ký gửi"))
            {
                if (entry.MsgId.Contains("Subcontract") || entry.MsgId.Contains("subcontract") || 
                    entry.MsgId.Contains("Job Worker") || entry.MsgId.Contains("Job worker"))
                {
                    entry.MsgStr = entry.MsgStr
                        .Replace("Ký gửi phụ", "Gia công ngoài")
                        .Replace("ký gửi phụ", "gia công ngoài")
                        .Replace("Đơn hàng vào ký gửi phụ", "Đơn nhận gia công")
                        .Replace("Đơn bán hàng ký gửi phụ", "Đơn bán hàng gia công")
                        .Replace("Đơn ký gửi phụ", "Đơn đặt gia công")
                        .Replace("PO Ký gửi phụ", "PO gia công")
                        .Replace("Biên nhận Ký gửi", "Biên nhận gia công")
                        .Replace("Đơn nhập ký gửi", "Đơn nhận gia công")
                        .Replace("Công nhân ký gửi", "Đơn vị gia công")
                        .Replace("công nhân ký gửi", "đơn vị gia công")
                        .Replace("Đơn mua hàng ký gửi", "Đơn mua hàng gia công")
                        .Replace("Đơn mua hàng Ký gửi", "Đơn mua hàng gia công")
                        .Replace("mặt hàng ký gửi", "mặt hàng gia công ngoài")
                        .Replace("nguyên vật liệu ký gửi", "nguyên vật liệu gia công ngoài")
                        .Replace("Đơn bán hàng ký gửi", "Đơn bán hàng gia công")
                        .Replace("Đơn ký gửi", "Đơn đặt gia công")
                        .Replace("Đơn hàng Ký gửi", "Đơn đặt gia công")
                        .Replace("đơn mua hàng ký gửi", "đơn mua hàng gia công");
                }
                
                if (entry.MsgId.Contains("Outstanding Checks and Deposits to clear"))
                {
                    entry.MsgStr = entry.MsgStr.Replace("Ký gửi", "Tiền gửi");
                }
            }
        }

        Save(path, entries, headers);
        Console.WriteLine("Saved erpnext_vi.po successfully.");
    }

    public static void ProcessFrappe(string path)
    {
        List<string> headers;
        List<PoEntry> entries = Parse(path, out headers);
        Console.WriteLine("Loaded " + entries.Count + " entries from frappe_vi.po");

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.MsgId)) continue;

            // 1. Fix line 616 syntax error
            if (entry.MsgStr.Contains("{% nếu nhận xét %}"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("{% nếu nhận xét %}", "{% if comments %}");
            }

            // 2. Fix line 32912 syntax error
            if (entry.MsgStr.Contains("${skip_list ? \"\" : loại}"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("${skip_list ? \"\" : loại}", "${skip_list ? \"\" : type}");
            }

            // 3. Queue replacements
            if (entry.MsgStr.Contains("hàng đợi") || entry.MsgStr.Contains("Hàng đợi"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("được xếp hàng đợi", "được đưa vào queue")
                    .Replace("xếp hàng đợi", "đưa vào queue")
                    .Replace("đưa vào hàng đợi", "đưa vào queue")
                    .Replace("hàng đợi", "queue")
                    .Replace("Hàng đợi", "Queue");
            }

            // 4. Server replacements
            if (entry.MsgStr.Contains("máy chủ") || entry.MsgStr.Contains("Máy chủ"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("phía máy chủ", "phía server")
                    .Replace("máy chủ email", "server email")
                    .Replace("máy chủ SMTP", "server SMTP")
                    .Replace("máy chủ xác thực", "server xác thực")
                    .Replace("máy chủ", "server")
                    .Replace("Máy chủ", "Server");
            }

            // 5. Domain replacements
            if (entry.MsgStr.Contains("tên miền") || entry.MsgStr.Contains("Tên miền"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("Tên miền email", "Domain email")
                    .Replace("tên miền email", "domain email")
                    .Replace("Cài đặt tên miền", "Cài đặt domain")
                    .Replace("Tên miền HTML", "Domain HTML")
                    .Replace("tên miền HTML", "domain HTML")
                    .Replace("Tên miền", "Domain")
                    .Replace("tên miền", "domain");
            }

            // 6. Webhook replacements
            if (entry.MsgStr.Contains("móc web") || entry.MsgStr.Contains("Móc web"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("móc web", "webhook")
                    .Replace("Móc web", "Webhook");
            }
        }

        Save(path, entries, headers);
        Console.WriteLine("Saved frappe_vi.po successfully.");
    }

    public static void ProcessHrms(string path)
    {
        List<string> headers;
        List<PoEntry> entries = Parse(path, out headers);
        Console.WriteLine("Loaded " + entries.Count + " entries from hrms_vi.po");

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.MsgId)) continue;

            if (entry.MsgStr.Contains("hàng đợi") || entry.MsgStr.Contains("Hàng đợi"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("được xếp hàng đợi", "được đưa vào queue")
                    .Replace("xếp hàng đợi", "đưa vào queue")
                    .Replace("đưa vào hàng đợi", "đưa vào queue")
                    .Replace("hàng đợi", "queue")
                    .Replace("Hàng đợi", "Queue");
            }
        }

        Save(path, entries, headers);
        Console.WriteLine("Saved hrms_vi.po successfully.");
    }

    public static void ProcessCrm(string path)
    {
        List<string> headers;
        List<PoEntry> entries = Parse(path, out headers);
        Console.WriteLine("Loaded " + entries.Count + " entries from crm_vi.po");

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.MsgId)) continue;

            // Domain replacements
            if (entry.MsgStr.Contains("tên miền") || entry.MsgStr.Contains("Tên miền"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("Tên miền phụ", "Subdomain")
                    .Replace("tên miền phụ", "subdomain")
                    .Replace("Tên miền", "Domain")
                    .Replace("tên miền", "domain");
            }

            // Server replacements
            if (entry.MsgStr.Contains("máy chủ") || entry.MsgStr.Contains("Máy chủ"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("máy chủ", "server")
                    .Replace("Máy chủ", "Server");
            }
        }

        Save(path, entries, headers);
        Console.WriteLine("Saved crm_vi.po successfully.");
    }

    public static void ProcessHelpdesk(string path)
    {
        List<string> headers;
        List<PoEntry> entries = Parse(path, out headers);
        Console.WriteLine("Loaded " + entries.Count + " entries from helpdesk_vi.po");

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.MsgId)) continue;

            // Queue replacements
            if (entry.MsgStr.Contains("hàng đợi") || entry.MsgStr.Contains("Hàng đợi"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("thêm nó vào hàng đợi", "thêm vào queue")
                    .Replace("thêm vào hàng đợi", "thêm vào queue")
                    .Replace("hàng đợi email", "queue email")
                    .Replace("được xếp hàng đợi", "được đưa vào queue")
                    .Replace("xếp hàng đợi", "đưa vào queue")
                    .Replace("đưa vào hàng đợi", "đưa vào queue")
                    .Replace("hàng đợi", "queue")
                    .Replace("Hàng đợi", "Queue");
            }

            // Domain replacements
            if (entry.MsgStr.Contains("tên miền") || entry.MsgStr.Contains("Tên miền"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("Chỉnh sửa tên miền", "Chỉnh sửa domain")
                    .Replace("Chỉnh sửa Tên miền", "Chỉnh sửa Domain")
                    .Replace("Tên miền", "Domain")
                    .Replace("tên miền", "domain");
            }

            // Server replacements
            if (entry.MsgStr.Contains("máy chủ") || entry.MsgStr.Contains("Máy chủ"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("máy chủ", "server")
                    .Replace("Máy chủ", "Server");
            }
        }

        Save(path, entries, headers);
        Console.WriteLine("Saved helpdesk_vi.po successfully.");
    }

    public static void ProcessInsights(string path)
    {
        List<string> headers;
        List<PoEntry> entries = Parse(path, out headers);
        Console.WriteLine("Loaded " + entries.Count + " entries from insights_vi.po");

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.MsgId)) continue;

            // Queue replacements
            if (entry.MsgStr.Contains("hàng đợi") || entry.MsgStr.Contains("Hàng đợi"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("Hàng đợi nhập hàng loạt", "Queue nhập hàng loạt")
                    .Replace("hàng đợi nhập hàng loạt", "queue nhập hàng loạt")
                    .Replace("Tiến độ hàng đợi hàng loạt", "Tiến độ queue hàng loạt")
                    .Replace("tiến độ hàng đợi hàng loạt", "tiến độ queue hàng loạt")
                    .Replace("đưa vào hàng đợi", "đưa vào queue")
                    .Replace("Đưa vào hàng đợi", "Đưa vào queue")
                    .Replace("được xếp hàng đợi", "được đưa vào queue")
                    .Replace("xếp hàng đợi", "đưa vào queue")
                    .Replace("hàng đợi", "queue")
                    .Replace("Hàng đợi", "Queue");
            }

            // Server replacements
            if (entry.MsgStr.Contains("máy chủ") || entry.MsgStr.Contains("Máy chủ"))
            {
                entry.MsgStr = entry.MsgStr
                    .Replace("máy chủ", "server")
                    .Replace("Máy chủ", "Server");
            }
        }

        Save(path, entries, headers);
        Console.WriteLine("Saved insights_vi.po successfully.");
    }
}
