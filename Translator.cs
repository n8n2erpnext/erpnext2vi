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

    private static Dictionary<string, string> dictExact = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // System UI & Navigation
        { "Desktop", "Tổng quan" },
        { "Workspace", "Khu làm việc" },
        { "Reload", "Làm mới" },
        { "Global Defaults", "Thiết lập chung" },
        { "User Settings", "Thiết lập tài khoản" },
        { "Assign To", "Giao việc" },
        { "Attachments", "Tài liệu đính kèm" },
        { "Activity", "Nhật ký hoạt động" },
        { "Timeline", "Lịch sử" },
        { "Rename", "Đổi mã" },
        { "Customize", "Tuỳ biến" },
        { "Getting Started", "Hướng dẫn nhanh" },

        // CRM
        { "Lead", "Khách hàng tiềm năng" },
        { "Deal", "Cơ hội bán hàng" },
        { "Opportunity", "Cơ hội kinh doanh" },
        { "Contact", "Liên hệ" },
        { "Call Logs", "Lịch sử cuộc gọi" },
        { "Notes", "Ghi chú" },
        { "Campaign", "Chiến dịch" },
        { "Prospect", "Khách tiềm năng" },
        { "Pipeline", "Quy trình bán hàng" },
        { "Stage", "Giai đoạn" },
        { "Lost Reason", "Lý do thất bại" },

        // HR / HRMS
        { "Employee", "Nhân viên" },
        { "Attendance", "Chấm công" },
        { "Shift", "Ca làm" },
        { "Leave Application", "Đơn nghỉ phép" },
        { "Payroll", "Bảng lương" },
        { "Salary Slip", "Phiếu lương" },
        { "Expense Claim", "Đề nghị thanh toán" },
        { "Department", "Phòng ban" },
        { "Designation", "Chức vụ" },
        { "Holiday List", "Lịch nghỉ" },
        { "Check In", "Ghi nhận vào ca" },
        { "Check Out", "Ghi nhận ra ca" },

        // Kho / Stock
        { "Stock Entry", "Phiếu kho" },
        { "Material Request", "Yêu cầu vật tư" },
        { "Delivery Note", "Phiếu giao hàng" },
        { "Purchase Receipt", "Phiếu nhập hàng" },
        { "Pick List", "Danh sách lấy hàng" },
        { "Item", "Mặt hàng" },
        { "Item Group", "Nhóm mặt hàng" },
        { "Warehouse", "Kho" },
        { "Reorder Level", "Mức tồn tối thiểu" },
        { "Stock Ledger", "Sổ kho" },
        { "Stock Reconciliation", "Đối soát tồn kho" },
        { "Stock Aging", "Tuổi tồn kho" },
        { "UOM", "Đơn vị tính" },

        // Buying
        { "Supplier", "Nhà cung cấp" },
        { "Request for Quotation", "Yêu cầu báo giá" },
        { "Supplier Quotation", "Báo giá nhà cung cấp" },
        { "Purchase Order", "Đơn mua hàng" },
        { "Purchase Invoice", "Hoá đơn mua hàng" },
        { "Landed Cost Voucher", "Chi phí nhập hàng" },

        // Selling
        { "Customer", "Khách hàng" },
        { "Quotation", "Báo giá" },
        { "Sales Order", "Đơn bán hàng" },
        { "Sales Invoice", "Hoá đơn bán hàng" },
        { "Pricing Rule", "Chính sách giá" },
        { "Territory", "Khu vực bán hàng" },

        // Accounting
        { "Journal Entry", "Bút toán" },
        { "Payment Entry", "Phiếu thanh toán" },
        { "Fiscal Year", "Năm tài chính" },
        { "Cost Center", "Trung tâm chi phí" },
        { "Chart of Accounts", "Hệ thống tài khoản" },
        { "General Ledger", "Sổ cái" },
        { "Receivable", "Công nợ phải thu" },
        { "Payable", "Công nợ phải trả" },
        { "Outstanding Amount", "Công nợ còn lại" },

        // Manufacturing
        { "BOM", "Định mức nguyên vật liệu" },
        { "Work Order", "Lệnh sản xuất" },
        { "Job Card", "Phiếu công đoạn" },
        { "Production Plan", "Kế hoạch sản xuất" },
        { "Routing", "Quy trình sản xuất" },

        // Helpdesk / Support
        { "Ticket", "Phiếu hỗ trợ" },
        { "Issue", "Sự cố" },
        { "Resolution", "Hướng xử lý" },
        { "Escalation", "Chuyển cấp xử lý" },

        // Website / CMS
        { "Blog Post", "Bài viết" },
        { "Web Page", "Trang nội dung" },
        { "Landing Page", "Trang giới thiệu" },
        { "Navigation", "Điều hướng" },
        { "Footer", "Chân trang" }
    };

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

    private static string ApplyGlobalReplacements(string text, string msgid)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Table 1: Technical & Operational Terms to Keep
        text = text.Replace("Thỏa thuận cấp độ dịch vụ", "SLA")
                   .Replace("Thoả thuận cấp độ dịch vụ", "SLA")
                   .Replace("Thỏa thuận Cấp độ Dịch vụ", "SLA")
                   .Replace("thỏa thuận cấp độ dịch vụ", "SLA");
        
        text = text.Replace("Điểm bán hàng (POS)", "POS")
                   .Replace("Điểm bán hàng", "POS")
                   .Replace("điểm bán hàng", "POS")
                   .Replace("Điểm bán lẻ", "POS");

        text = text.Replace("Bảng điều khiển", "Dashboard")
                   .Replace("bảng điều khiển", "dashboard");

        text = text.Replace("Quy trình làm việc", "Workflow")
                   .Replace("quy trình làm việc", "workflow");

        text = text.Replace("Đồng bộ hóa", "Sync")
                   .Replace("Đồng bộ hoá", "Sync")
                   .Replace("đồng bộ hóa", "sync")
                   .Replace("đồng bộ hoá", "sync");

        text = text.Replace("Nhật ký hệ thống", "Log")
                   .Replace("nhật ký hệ thống", "log");

        text = text.Replace("Số sê-ri", "Serial")
                   .Replace("Số sê ri", "Serial")
                   .Replace("Số seri", "Serial")
                   .Replace("số sê-ri", "serial")
                   .Replace("số sê ri", "serial")
                   .Replace("số seri", "serial");

        text = text.Replace("Số lô", "Batch")
                   .Replace("Lô hàng", "Batch")
                   .Replace("số lô", "batch")
                   .Replace("lô hàng", "batch");

        text = text.Replace("Nhập khẩu tệp", "Import tệp")
                   .Replace("Nhập khẩu dữ liệu", "Import dữ liệu")
                   .Replace("Nhập dữ liệu từ tệp", "Import dữ liệu");

        text = text.Replace("Xuất khẩu tệp", "Export tệp")
                   .Replace("Xuất khẩu dữ liệu", "Export dữ liệu");

        text = text.Replace("Bộ lọc", "Filter")
                   .Replace("bộ lọc", "filter");

        text = text.Replace("Chủ đề", "Theme")
                   .Replace("chủ đề", "theme");

        text = text.Replace("Vai trò", "Role")
                   .Replace("vai trò", "role");

        text = text.Replace("Phiên làm việc", "Session")
                   .Replace("phiên làm việc", "session");

        text = text.Replace("Mã thông báo", "Token")
                   .Replace("mã thông báo", "token");

        text = text.Replace("Hàng đợi", "Queue")
                   .Replace("hàng đợi", "queue");

        text = text.Replace("Bộ nhớ đệm", "Cache")
                   .Replace("bộ nhớ đệm", "cache");

        text = text.Replace("Móc web", "Webhook")
                   .Replace("móc web", "webhook");

        text = text.Replace("Tên miền", "Domain")
                   .Replace("tên miền", "domain");

        text = text.Replace("Máy chủ", "Server")
                   .Replace("máy chủ", "server");

        // Subcontracting (Gia công ngoài)
        if (msgid.Contains("Subcontract") || msgid.Contains("subcontract") || msgid.Contains("Job Worker") || msgid.Contains("Job worker"))
        {
            text = text
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

        // Table 2 to 11 Replacements (inside translations to clean up)
        text = text.Replace("Không gian làm việc", "Khu làm việc");
        text = text.Replace("Mặc định toàn cục", "Thiết lập chung");
        text = text.Replace("Cài đặt người dùng", "Thiết lập tài khoản");
        text = text.Replace("Tệp đính kèm", "Tài liệu đính kèm");
        text = text.Replace("Dòng thời gian", "Lịch sử");
        text = text.Replace("Tải lại", "Làm mới");
        text = text.Replace("Sơ đồ tài khoản", "Hệ thống tài khoản");
        text = text.Replace("Sơ đồ Tài khoản", "Hệ thống tài khoản");
        text = text.Replace("sơ đồ tài khoản", "hệ thống tài khoản");
        text = text.Replace("Biên nhận mua hàng", "Phiếu nhập hàng");
        text = text.Replace("Phiếu nhận mua hàng", "Phiếu nhập hàng");
        text = text.Replace("Phiếu nhập kho mua hàng", "Phiếu nhập hàng");
        text = text.Replace("Yêu cầu nguyên vật liệu", "Yêu cầu vật tư");
        text = text.Replace("Yêu cầu vật liệu", "Yêu cầu vật tư");
        text = text.Replace("Yêu cầu chào giá", "Yêu cầu báo giá");
        text = text.Replace("Đơn xin nghỉ phép", "Đơn nghỉ phép");
        text = text.Replace("Đơn đăng ký nghỉ phép", "Đơn nghỉ phép");
        text = text.Replace("Bảng thanh toán lương", "Bảng lương");
        text = text.Replace("Yêu cầu thanh toán chi phí", "Đề nghị thanh toán");
        text = text.Replace("Yêu cầu chi phí", "Đề nghị thanh toán");
        text = text.Replace("Bảng lương của nhân viên", "Bảng lương");
        text = text.Replace("Thẻ công việc", "Phiếu công đoạn");
        text = text.Replace("Định tuyến", "Quy trình sản xuất");
        text = text.Replace("Lý do mất", "Lý do thất bại");
        text = text.Replace("Công nợ còn lại", "Công nợ còn lại");
        text = text.Replace("Đảng chung", "Đối tác chung");
        text = text.Replace("Sức khỏe sổ cái", "Trạng thái sổ cái");
        text = text.Replace("sức khỏe sổ cái", "trạng thái sổ cái");

        return text;
    }

    private static void ProcessFile(string path, string appName)
    {
        List<string> headers;
        List<PoEntry> entries = Parse(path, out headers);
        Console.WriteLine("Loaded " + entries.Count + " entries from " + Path.GetFileName(path));

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.MsgId)) continue;

            // 1. Exact Glossary Override (Case-Insensitive Match)
            if (dictExact.ContainsKey(entry.MsgId))
            {
                entry.MsgStr = dictExact[entry.MsgId];
                continue;
            }

            // 2. ErpNext line 21 specific case
            if (appName == "erpnext" && entry.MsgId.Contains("The Batch {0} of an item {1} has negative stock"))
            {
                entry.MsgStr = "\n\t\t\tLô {0} của mặt hàng {1} đang bị âm kho trong kho {2}{3}.\n\t\t\tVui lòng bổ sung số lượng tồn kho là {4} để tiếp tục thực hiện bút toán này.\n\t\t\tNếu không thể tạo bút toán điều chỉnh, vui lòng bật 'Cho phép âm kho theo Lô' trong Cấu hình Kho để tiếp tục.\n\t\t\tTuy nhiên, việc bật cấu hình này có thể dẫn đến âm kho trong hệ thống.\n\t\t\tVì vậy, hãy đảm bảo số lượng tồn kho được điều chỉnh sớm nhất có thể để duy trì tỷ giá định giá chính xác.";
                continue;
            }

            // 3. Frappe syntax fixes
            if (appName == "frappe")
            {
                if (entry.MsgStr.Contains("{% nếu nhận xét %}"))
                {
                    entry.MsgStr = entry.MsgStr.Replace("{% nếu nhận xét %}", "{% if comments %}");
                }
                if (entry.MsgStr.Contains("${skip_list ? \"\" : loại}"))
                {
                    entry.MsgStr = entry.MsgStr.Replace("${skip_list ? \"\" : loại}", "${skip_list ? \"\" : type}");
                }
            }

            // 4. Apply general replacement rules to translated strings
            entry.MsgStr = ApplyGlobalReplacements(entry.MsgStr, entry.MsgId);
        }

        Save(path, entries, headers);
        Console.WriteLine("Saved " + Path.GetFileName(path) + " successfully.");
    }

    public static void ProcessErpNext(string path)
    {
        ProcessFile(path, "erpnext");
    }

    public static void ProcessFrappe(string path)
    {
        ProcessFile(path, "frappe");
    }

    public static void ProcessHrms(string path)
    {
        ProcessFile(path, "hrms");
    }

    public static void ProcessCrm(string path)
    {
        ProcessFile(path, "crm");
    }

    public static void ProcessHelpdesk(string path)
    {
        ProcessFile(path, "helpdesk");
    }

    public static void ProcessInsights(string path)
    {
        ProcessFile(path, "insights");
    }
}
