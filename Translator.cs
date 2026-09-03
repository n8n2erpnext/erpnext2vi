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
        // Brand name overrides
        { "Frappe", "Frappe" },
        { "Frappe Support", "Hỗ trợ Frappe" },

        // Timesheet / Project Settings fixes
        { "Ignore Employee Time Overlap", "Bỏ qua trùng lặp thời gian nhân viên" },
        { "Ignore User Time Overlap", "Bỏ qua trùng lặp thời gian người dùng" },
        { "Ignore Workstation Time Overlap", "Bỏ qua trùng lặp thời gian trạm làm việc" },
        { "Enabling the check box will fetch timesheet on select of a Project in Sales Invoice", "Bật hộp kiểm sẽ lấy bảng chấm công khi chọn Dự án trong Hóa đơn bán hàng" },
        { "Float Precision", "Độ chính xác số thập phân" },

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
        { "First Name", "Họ" },
        { "Last Name", "Tên" },
        { "Series", "Chuỗi đặt tên" },
        { "New {0}", "{0} mới" },
        { "New {0} Created", "Đã tạo {0} mới" },
        { "New {0}: {1}", "{0} mới: {1}" },
        { "Mr", "Mr" },
        { "Mrs", "Mrs" },
        { "Ms", "Ms" },
        { "Miss", "Miss" },
        { "Dr", "Dr" },
        { "Madam", "Madam" },
        { "Assignment", "Phân công" },
        { "Assignment Rule", "Quy tắc phân công" },
        { "Assignment Rules", "Quy tắc phân công" },
        { "Assignment Completed", "Phân công đã hoàn thành" },
        { "View Assignment Rules", "Xem quy tắc phân công" },
        { "Clear Assignment", "Hủy giao" },
        { "Are you sure you want to clear the assignments?", "Bạn có chắc chắn muốn hủy giao không?" },
        { "Please save the document before removing assignment", "Vui lòng lưu tài liệu trước khi hủy giao" },
        { "Select records for removing assignment", "Chọn bản ghi để hủy giao" },
        { "Your assignment on task {0} has been removed by {1}", "Phân công của bạn trên công việc {0} đã bị hủy bởi {1}" },
        { "Your assignment on {0} {1} has been removed by {2}", "Phân công của bạn trên {0} {1} đã bị hủy bởi {2}" },
        { "{0} removed their assignment.", "{0} đã hủy giao của họ." },

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
        { "Holiday List Name", "Tên lịch nghỉ" },
        { "Applicable Holiday List", "Lịch nghỉ áp dụng" },
        { "Default Holiday List", "Lịch nghỉ mặc định" },
        { "Create New Holiday List", "Tạo lịch nghỉ mới" },
        { "Select Holiday List", "Chọn lịch nghỉ" },
        { "No holiday list found", "Không tìm thấy lịch nghỉ" },
        { "Check In", "Ghi nhận vào ca" },
        { "Check Out", "Ghi nhận ra ca" },

        // Kho / Stock
        { "Stock Entry", "Phiếu kho" },
        { "Material Request", "Yêu cầu vật tư" },
        { "Delivery Note", "Phiếu giao hàng" },
        { "Purchase Receipt", "Phiếu nhập hàng" },
        { "Pick List", "Danh sách lấy hàng" },
        { "Create Pick List", "Tạo danh sách lấy hàng" },
        { "Against Pick List", "Theo danh sách lấy hàng" },
        { "Pick List Incomplete", "Danh sách lấy hàng chưa hoàn tất" },
        { "Pick List Item", "Mặt hàng trong danh sách lấy hàng" },
        { "Item", "Mặt hàng" },
        { "Item Group", "Nhóm mặt hàng" },
        { "Warehouse", "Kho" },
        { "Reorder Level", "Mức tồn tối thiểu" },
        { "Stock Ledger", "Sổ kho" },
        { "Stock Reconciliation", "Đối soát tồn kho" },
        { "Stock Aging", "Tuổi tồn kho" },
        { "Valuation Rate", "Đơn giá định giá" },
        { "Allow Zero Valuation Rate", "Cho phép đơn giá định giá bằng không" },
        { "Current Valuation Rate", "Đơn giá định giá hiện tại" },
        { "Posting Date", "Ngày hạch toán" },
        { "Posting Time", "Giờ hạch toán" },
        { "Posting Datetime", "Thời điểm hạch toán" },
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
        { "Buying Price List", "Bảng giá mua" },
        { "Selling Price List", "Bảng giá bán" },
        { "Sales Price List", "Bảng giá bán" },
        { "Purchase Price List", "Bảng giá mua" },
        { "Default Buying Price List", "Bảng giá mua mặc định" },
        { "Price List Rate", "Đơn giá bảng giá" },
        { "Price List Rate (Company Currency)", "Đơn giá bảng giá (Tiền tệ công ty)" },
        { "Avg. Buying Price List Rate", "Đơn giá mua trung bình" },
        { "Avg. Selling Price List Rate", "Đơn giá bán trung bình" },
        { "Item-wise Price List Rate", "Đơn giá bảng giá theo mặt hàng" },
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
        { "Accounts Receivable", "Công nợ phải thu" },
        { "Accounts Payable", "Công nợ phải trả" },
        { "Accounts Receivable Summary", "Tổng hợp công nợ phải thu" },
        { "Accounts Payable Summary", "Tổng hợp công nợ phải trả" },
        { "Accounts Receivable / Payable Tuning", "Điều chỉnh Công nợ phải thu / phải trả" },
        { "Accounts Receivable/Payable", "Công nợ phải thu/phải trả" },
        { "Accounts Receivable Unpaid Account", "Tài khoản công nợ phải thu chưa thanh toán" },
        { "Opening Invoice Creation Tool", "Tạo công nợ đầu kỳ" },
        { "Opening Invoice Tool", "Tạo công nợ đầu kỳ" },
        { "Opening Invoice Creation Tool Item", "Mục Tạo công nợ đầu kỳ" },
        { "Opening Invoice Item", "Mục công nợ đầu kỳ" },
        { "Opening Invoices", "Hóa đơn công nợ đầu kỳ" },
        { "Opening Invoice", "Hóa đơn công nợ đầu kỳ" },
        { "Accounting Dimension", "Phân tích kế toán" },
        { "Accounting Dimensions", "Phân tích kế toán" },
        { "Accounting Dimension Detail", "Chi tiết phân tích kế toán" },
        { "Accounting Dimension Filter", "Bộ lọc phân tích kế toán" },
        { "Accounting Dimensions Filter", "Bộ lọc phân tích kế toán" },
        { "Journal Entry Template", "Mẫu định khoản" },
        { "Journal Entry Template Account", "Tài khoản mẫu định khoản" },

        // Stock / Ageing / Inventory Dimension overrides
        { "Stock Ageing", "Tuổi tồn kho" },
        { "Show Stock Ageing Data", "Hiển thị dữ liệu tuổi tồn kho" },
        { "Ageing Based On", "Tính tuổi nợ/kho theo" },
        { "Ageing Range", "Khoảng phân tích tuổi" },
        { "Ageing Report based on {0} up to {1}", "Báo cáo phân tích tuổi nợ dựa trên {0} đến {1}" },
        { "Inventory Dimension", "Thuộc tính tồn kho" },
        { "Inventory Dimension key", "Khóa thuộc tính tồn kho" },
        { "Inventory Dimension Negative Stock", "Âm kho theo thuộc tính tồn kho" },

        // HR / HRMS overrides
        { "Employee Exits", "Nhân viên nghỉ việc" },
        { "Employee Exit Settings", "Cài đặt nhân viên nghỉ việc" },
        { "Employee Exit", "Nhân viên nghỉ việc" },
        { "Employee Exit Template", "Mẫu nhân viên nghỉ việc" },
        { "Date of Joining", "Ngày vào làm" },
        { "Joining", "Nhận việc" },
        { "Exit", "Thôi việc" },
        { "Left", "Chờ thôi việc" },
        { "Employment Type", "Hình thức làm việc" },

        // Manufacturing
        { "Subcontract", "Gia công" },
        { "Sub-contracting", "Gia công" },
        { "Subcontracting", "Gia công" },
        { "Subcontracted Item", "Mặt hàng gia công" },
        { "Subcontracting Order", "Đơn gia công" },
        { "Subcontracting Inward", "Nhận gia công" },
        { "Subcontracting Inward Order", "Đơn nhận gia công" },
        { "Subcontracting Outward Order", "Đơn thuê gia công" },
        { "Subcontracting Receipt", "Phiếu nhận gia công" },
        { "Subcontracting Purchase Order", "Đơn mua hàng gia công" },
        { "Subcontracting Sales Order", "Đơn bán hàng gia công" },
        { "Subcontracting BOM", "Định mức gia công" },
        { "Job Worker", "Đơn vị gia công" },
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
        { "Web Form", "Biểu mẫu web" },
        { "Web Form Field", "Trường biểu mẫu web" },
        { "Web Form Fields", "Các trường biểu mẫu web" },
        { "Web Form List Column", "Cột danh sách biểu mẫu web" },
        { "Website", "Trang web" },
        { "Website Manager", "Quản lý trang web" },
        { "Website Settings", "Cài đặt trang web" },
        { "Website Sidebar", "Thanh bên trang web" },
        { "Website Sidebar Item", "Mục thanh bên trang web" },
        { "Website Slideshow", "Trình chiếu trang web" },
        { "Website Slideshow Item", "Mục trình chiếu trang web" },
        { "Website Theme", "Theme trang web" },
        { "Website Theme Ignore App", "Ứng dụng bỏ qua Theme trang web" },
        { "Website Theme Image", "Hình ảnh Theme trang web" },
        { "Website Themes Available", "Theme trang web khả dụng" },
        { "Website Users", "Người dùng trang web" },
        { "Website Visits", "Lượt truy cập trang web" },
        { "Menu", "Trình đơn" },
        { "Sidebar", "Thanh bên" },
        { "Child Table", "Bảng con" },
        { "System Console", "Console hệ thống" },
        { "Authenticate as Service Principal", "Xác thực bằng Service Principal" },
        { "Popover or Modal Description", "Mô tả cửa sổ nổi hoặc hộp thoại" },
        { "Round Robin", "Luân phiên" },
        { "Show Auth Server Metadata", "Hiển thị siêu dữ liệu Server xác thực" },
        { "SocketIO Ping Check", "Kiểm tra ping SocketIO" },
        { "SocketIO Transport Mode", "Chế độ truyền tải SocketIO" },
        { "Top", "Trên" },
        { "Top 10", "10 mục hàng đầu" },
        { "Top Bar Items", "Các mục trên thanh trên cùng" },
        { "Pull Emails", "Nhận email" },
        { "User Menu", "Trình đơn người dùng" },
        { "Portal Menu", "Trình đơn cổng thông tin" },
        { "Portal Menu Item", "Mục trình đơn cổng thông tin" },
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

    private static bool ContainsIgnoreCase(string source, string value)
    {
        return source != null && source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string ApplyGlobalReplacements(string text, string msgid)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Obvious machine-translation artifacts; these are never valid ERP terminology.
        text = text.Replace("Serialal", "Serial")
                   .Replace("serialal", "serial")
                   .Replace("Tài liệu trẻ em", "Tài liệu con")
                   .Replace("tài liệu trẻ em", "tài liệu con");

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

        // ERP semantic phrase families: source msgid gates every replacement.
        if (ContainsIgnoreCase(msgid, "Holiday List"))
        {
            text = text.Replace("Danh Sách Ngày Lễ", "Lịch nghỉ")
                       .Replace("Danh sách Ngày lễ", "Lịch nghỉ")
                       .Replace("Danh sách ngày lễ", "Lịch nghỉ")
                       .Replace("danh sách ngày lễ", "lịch nghỉ")
                       .Replace("Danh sách ngày nghỉ", "Lịch nghỉ")
                       .Replace("danh sách ngày nghỉ", "lịch nghỉ");
        }

        if (ContainsIgnoreCase(msgid, "Pick List"))
        {
            text = text.Replace("Danh sách chọn", "Danh sách lấy hàng")
                       .Replace("danh sách chọn", "danh sách lấy hàng");
        }

        if (ContainsIgnoreCase(msgid, "Price List"))
        {
            text = text.Replace("Danh sách Giá", "Bảng giá")
                       .Replace("Danh sách giá", "Bảng giá")
                       .Replace("Tiền tệ danh sách giá", "Tiền tệ bảng giá")
                       .Replace("tiền tệ danh sách giá", "tiền tệ bảng giá")
                       .Replace("danh sách Giá", "bảng giá")
                       .Replace("danh sách giá", "bảng giá");
        }

        if (ContainsIgnoreCase(msgid, "Price List Rate"))
        {
            text = text.Replace("Tỷ giá Bảng giá", "Đơn giá bảng giá")
                       .Replace("Tỷ giá bảng giá", "Đơn giá bảng giá")
                       .Replace("tỷ giá bảng giá", "đơn giá bảng giá")
                       .Replace("Tỷ lệ bảng giá", "Đơn giá bảng giá")
                       .Replace("tỷ lệ bảng giá", "đơn giá bảng giá");
        }

        if (ContainsIgnoreCase(msgid, "Sales Order"))
        {
            text = text.Replace("Đơn hàng Bán", "Đơn bán hàng")
                       .Replace("Đơn hàng bán", "Đơn bán hàng")
                       .Replace("đơn hàng bán", "đơn bán hàng")
                       .Replace("Đơn đặt hàng", "Đơn bán hàng");
        }

        if (ContainsIgnoreCase(msgid, "Landed Cost"))
        {
            text = text.Replace("chi phí đã đáp tàu", "chi phí nhập hàng")
                       .Replace("Chi phí đã đáp tàu", "Chi phí nhập hàng");
        }

        if (ContainsIgnoreCase(msgid, "Expense Claim"))
        {
            text = text.Replace("Claim chi phí", "Đề nghị thanh toán")
                       .Replace("claim chi phí", "đề nghị thanh toán")
                       .Replace("Claims chi phí", "các Đề nghị thanh toán")
                       .Replace("claims chi phí", "các đề nghị thanh toán");
        }

        if (ContainsIgnoreCase(msgid, "booked"))
        {
            text = text.Replace("đã book", "đã hạch toán")
                       .Replace("đã định sổ", "đã hạch toán")
                       .Replace("Đã định sổ", "Đã hạch toán");
        }

        if (ContainsIgnoreCase(msgid, "Workspace"))
        {
            text = text.Replace("Không gian làm việc", "Khu làm việc")
                       .Replace("không gian làm việc", "khu làm việc");
        }

        if (ContainsIgnoreCase(msgid, "Attachment"))
        {
            text = text.Replace("Tập tin đính kèm", "Tài liệu đính kèm")
                       .Replace("tập tin đính kèm", "tài liệu đính kèm")
                       .Replace("Tệp đính kèm", "Tài liệu đính kèm")
                       .Replace("tệp đính kèm", "tài liệu đính kèm");
        }

        if (ContainsIgnoreCase(msgid, "Timeline"))
        {
            text = text.Replace("Dòng thời gian", "Lịch sử")
                       .Replace("dòng thời gian", "lịch sử")
                       .Replace("Mốc thời gian", "Lịch sử")
                       .Replace("mốc thời gian", "lịch sử");
        }

        if (ContainsIgnoreCase(msgid, "Leave Application"))
        {
            text = text.Replace("Đơn Xin Nghỉ Phép", "Đơn nghỉ phép")
                       .Replace("Đơn xin nghỉ phép", "Đơn nghỉ phép")
                       .Replace("đơn xin nghỉ phép", "đơn nghỉ phép");
        }

        if (ContainsIgnoreCase(msgid, "Stock Entry"))
        {
            text = text.Replace("Bút toán Kho", "Phiếu kho")
                       .Replace("Bút toán kho", "Phiếu kho")
                       .Replace("bút toán kho", "phiếu kho")
                       .Replace("Mục Kho", "Phiếu kho")
                       .Replace("Mục kho", "Phiếu kho")
                       .Replace("mục kho", "phiếu kho")
                       .Replace("Mục tồn kho", "Phiếu kho")
                       .Replace("mục tồn kho", "phiếu kho")
                       .Replace("Mục nhập kho", "Phiếu kho")
                       .Replace("mục nhập kho", "phiếu kho")
                       .Replace("Bảng nhập kho", "Phiếu kho")
                       .Replace("Bút toán tồn kho", "Phiếu kho")
                       .Replace("bút toán tồn kho", "phiếu kho")
                       .Replace("Mục Hàng tồn kho", "Phiếu kho")
                       .Replace("Mục nhập tồn kho", "Phiếu kho")
                       .Replace("mục nhập tồn kho", "phiếu kho")
                       .Replace("Nhập kho", "Phiếu kho");
        }

        if (ContainsIgnoreCase(msgid, "Material Request"))
        {
            text = text.Replace("Yêu cầu Vật liệu", "Yêu cầu vật tư")
                       .Replace("yêu cầu vật liệu", "yêu cầu vật tư")
                       .Replace("Yêu cầu Nguyên vật liệu", "Yêu cầu vật tư")
                       .Replace("Yêu cầu Nguyên liệu", "Yêu cầu vật tư");
        }

        if (ContainsIgnoreCase(msgid, "Delivery Note"))
        {
            text = text.Replace("Ghi chú giao hàng", "Phiếu giao hàng")
                       .Replace("ghi chú giao hàng", "phiếu giao hàng");
        }

        if (ContainsIgnoreCase(msgid, "Purchase Receipt"))
        {
            text = text.Replace("Biên nhận Mua hàng", "Phiếu nhập hàng")
                       .Replace("Biên nhận mua hàng", "Phiếu nhập hàng")
                       .Replace("biên nhận mua hàng", "phiếu nhập hàng")
                       .Replace("Phiếu nhận hàng mua", "Phiếu nhập hàng")
                       .Replace("Phiếu nhận hàng", "Phiếu nhập hàng")
                       .Replace("phiếu nhận hàng", "phiếu nhập hàng")
                       .Replace("Phiếu nhập hàng mua", "Phiếu nhập hàng")
                       .Replace("phiếu nhập hàng mua", "phiếu nhập hàng")
                       .Replace("Biên lai mua hàng", "Phiếu nhập hàng")
                       .Replace("biên lai mua hàng", "phiếu nhập hàng")
                       .Replace("Phiếu nhận mua", "Phiếu nhập hàng")
                       .Replace("phiếu nhận mua", "phiếu nhập hàng");
        }

        if (ContainsIgnoreCase(msgid, "Stock Ledger"))
        {
            text = text.Replace("Sổ cái Tồn kho", "Sổ kho")
                       .Replace("Sổ cái tồn kho", "Sổ kho")
                       .Replace("sổ cái tồn kho", "sổ kho")
                       .Replace("Sổ tồn kho", "Sổ kho")
                       .Replace("sổ tồn kho", "sổ kho")
                       .Replace("Sổ cái chứng khoán", "Sổ kho");
        }

        if (ContainsIgnoreCase(msgid, "Stock Reconciliation"))
        {
            text = text.Replace("Đối soát Kho", "Đối soát tồn kho")
                       .Replace("Đối soát Hàng tồn kho", "Đối soát tồn kho")
                       .Replace("đối soát hàng tồn kho", "đối soát tồn kho");
        }

        if (ContainsIgnoreCase(msgid, "Supplier Quotation"))
        {
            text = text.Replace("Báo giá từ Nhà cung cấp", "Báo giá nhà cung cấp")
                       .Replace("Báo giá từ nhà cung cấp", "Báo giá nhà cung cấp")
                       .Replace("báo giá từ nhà cung cấp", "báo giá nhà cung cấp");
        }

        if (ContainsIgnoreCase(msgid, "Purchase Order"))
        {
            text = text.Replace("Đơn đặt hàng", "Đơn mua hàng")
                       .Replace("đơn đặt hàng", "đơn mua hàng")
                       .Replace("Purchase Order", "Đơn mua hàng")
                       .Replace("Đơn hàng mua", "Đơn mua hàng")
                       .Replace("đơn hàng mua", "đơn mua hàng")
                       .Replace("Đơn bán hàng Mua", "Đơn mua hàng");
        }

        if (ContainsIgnoreCase(msgid, "Purchase Invoice"))
        {
            text = text.Replace("Hóa đơn Mua hàng", "Hoá đơn mua hàng")
                       .Replace("Hóa đơn mua hàng", "Hoá đơn mua hàng")
                       .Replace("hóa đơn mua hàng", "hoá đơn mua hàng");
        }

        if (ContainsIgnoreCase(msgid, "Landed Cost Voucher"))
        {
            text = text.Replace("Chứng từ Chi phí Hạ cánh", "Chi phí nhập hàng")
                       .Replace("Phiếu chi phí vận chuyển", "Chi phí nhập hàng")
                       .Replace("Phiếu chi phí hạ tầng", "Chi phí nhập hàng")
                       .Replace("Chi phí Landed", "Chi phí nhập hàng")
                       .Replace("chi phí Landed", "chi phí nhập hàng");
        }

        if (ContainsIgnoreCase(msgid, "Sales Invoice"))
        {
            text = text.Replace("Hóa đơn Bán hàng", "Hoá đơn bán hàng")
                       .Replace("Hóa đơn bán hàng", "Hoá đơn bán hàng")
                       .Replace("hóa đơn bán hàng", "hoá đơn bán hàng");
        }

        if (ContainsIgnoreCase(msgid, "Pricing Rule"))
        {
            text = text.Replace("Quy tắc Định giá", "Chính sách giá")
                       .Replace("Quy tắc định giá", "Chính sách giá")
                       .Replace("quy tắc định giá", "chính sách giá")
                       .Replace("Quy tắc giá", "Chính sách giá")
                       .Replace("quy tắc giá", "chính sách giá")
                       .Replace("Pricing Rules", "Chính sách giá")
                       .Replace("Pricing Rule", "Chính sách giá");
        }

        if (ContainsIgnoreCase(msgid, "Payment Entry"))
        {
            text = text.Replace("Bút toán Thanh toán", "Phiếu thanh toán")
                       .Replace("Bút toán thanh toán", "Phiếu thanh toán")
                       .Replace("bút toán thanh toán", "phiếu thanh toán")
                       .Replace("Mục Thanh toán", "Phiếu thanh toán")
                       .Replace("Mục thanh toán", "Phiếu thanh toán")
                       .Replace("mục thanh toán", "phiếu thanh toán")
                       .Replace("Payment Entry", "Phiếu thanh toán");
        }

        if (ContainsIgnoreCase(msgid, "GL Entry"))
        {
            text = text.Replace("Các mục GL", "Các bút toán sổ cái")
                       .Replace("các mục GL", "các bút toán sổ cái")
                       .Replace("Mục GL", "Bút toán sổ cái")
                       .Replace("mục GL", "bút toán sổ cái");
        }

        if (ContainsIgnoreCase(msgid, "Work Order"))
        {
            text = text.Replace("Lệnh Công việc", "Lệnh sản xuất")
                       .Replace("Lệnh công việc", "Lệnh sản xuất")
                       .Replace("lệnh công việc", "lệnh sản xuất")
                       .Replace("Work Order", "Lệnh sản xuất")
                       .Replace("work order", "lệnh sản xuất")
                       .Replace("Đơn đặt hàng công việc", "Lệnh sản xuất")
                       .Replace("đơn đặt hàng công việc", "lệnh sản xuất")
                       .Replace("Đơn hàng công việc", "Lệnh sản xuất")
                       .Replace("đơn hàng công việc", "lệnh sản xuất");
        }

        if (ContainsIgnoreCase(msgid, "Job Card"))
        {
            text = text.Replace("Thẻ công việc", "Phiếu công đoạn")
                       .Replace("thẻ công việc", "phiếu công đoạn");
        }

        if (ContainsIgnoreCase(msgid, "Valuation Rate"))
        {
            text = text.Replace("Tỷ giá Định giá", "Đơn giá định giá")
                       .Replace("Tỷ giá định giá", "Đơn giá định giá")
                       .Replace("tỷ giá định giá", "đơn giá định giá")
                       .Replace("Tỷ lệ Định giá", "Đơn giá định giá")
                       .Replace("Tỷ lệ định giá", "Đơn giá định giá")
                       .Replace("tỷ lệ định giá", "đơn giá định giá");
        }

        if (ContainsIgnoreCase(msgid, "Posting Date") || ContainsIgnoreCase(msgid, "Posting Datetime"))
        {
            text = text.Replace("Ngày Đăng", "Ngày hạch toán")
                       .Replace("Ngày đăng", "Ngày hạch toán")
                       .Replace("ngày đăng", "ngày hạch toán")
                       .Replace("Ngày giờ đăng", "Thời điểm hạch toán")
                       .Replace("ngày giờ đăng", "thời điểm hạch toán");
        }

        if (ContainsIgnoreCase(msgid, "Posting Time") || ContainsIgnoreCase(msgid, "Posting Timestamp"))
        {
            text = text.Replace("Thời gian đăng", "Giờ hạch toán")
                       .Replace("thời gian đăng", "giờ hạch toán")
                       .Replace("Giờ đăng", "Giờ hạch toán")
                       .Replace("giờ đăng", "giờ hạch toán")
                       .Replace("Dấu thời gian đăng", "Thời điểm hạch toán")
                       .Replace("dấu thời gian đăng", "thời điểm hạch toán");
        }

        // Core Frappe UI families: translate ordinary UI nouns, retain project-approved technical terms.
        if (ContainsIgnoreCase(msgid, "Web Form"))
        {
            text = text.Replace("Web Forms", "Biểu mẫu web")
                       .Replace("Web Form", "Biểu mẫu web")
                       .Replace("web forms", "biểu mẫu web")
                       .Replace("web form", "biểu mẫu web");
        }
        if (ContainsIgnoreCase(msgid, "Website"))
        {
            text = text.Replace("Website", "Trang web")
                       .Replace("website", "trang web");
        }
        if (ContainsIgnoreCase(msgid, "Menu"))
        {
            text = text.Replace("Menu", "Trình đơn")
                       .Replace("menu", "trình đơn");
        }
        if (ContainsIgnoreCase(msgid, "Sidebar"))
        {
            text = text.Replace("Sidebar", "Thanh bên")
                       .Replace("sidebar", "thanh bên");
        }
        if (ContainsIgnoreCase(msgid, "Timeline"))
        {
            text = text.Replace("Timeline", "Lịch sử")
                       .Replace("timeline", "lịch sử");
        }
        if (ContainsIgnoreCase(msgid, "Child Table"))
        {
            text = text.Replace("Child Table", "Bảng con")
                       .Replace("child table", "bảng con");
        }
        if (ContainsIgnoreCase(msgid, "developer mode"))
        {
            text = text.Replace("chế độ developer", "chế độ nhà phát triển")
                       .Replace("Chế độ Developer", "Chế độ Nhà phát triển");
        }

        if (ContainsIgnoreCase(msgid, "Onboarding"))
        {
            text = text.Replace("Onboarding", "giới thiệu")
                       .Replace("onboarding", "giới thiệu");
        }
        if (ContainsIgnoreCase(msgid, "ToDo"))
        {
            text = text.Replace("ToDo", "Việc cần làm");
        }
        if (ContainsIgnoreCase(msgid, "Blog Post"))
        {
            text = text.Replace("Blog Post", "Bài viết");
        }

        // Subcontracting (Gia công ngoài)
        if (ContainsIgnoreCase(msgid, "Subcontract") || ContainsIgnoreCase(msgid, "Sub-contract") || ContainsIgnoreCase(msgid, "Job Worker"))
        {
            text = text
                .Replace("Ký gửi", "Gia công")
                .Replace("ký gửi", "gia công")
                .Replace("Giao việc ngoài", "Gia công")
                .Replace("giao việc ngoài", "gia công")
                .Replace("Đơn hàng phụ thuộc vào", "Đơn nhận gia công")
                .Replace("Đơn vị Gia công phụ", "Đơn vị gia công")
                .Replace("đơn vị gia công phụ", "đơn vị gia công")
                .Replace("Công nhân Việc", "Đơn vị gia công")
                .Replace("Gia công phụ", "Gia công")
                .Replace("gia công phụ", "gia công")
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

        // Additional replacements requested by user
        text = text.Replace("Chiều Kế toán", "Phân tích kế toán");
        text = text.Replace("Chiều kế toán", "Phân tích kế toán");
        text = text.Replace("chiều kế toán", "phân tích kế toán");

        text = text.Replace("Mẫu bút toán nhật ký", "Mẫu định khoản");
        text = text.Replace("mẫu bút toán nhật ký", "mẫu định khoản");

        text = text.Replace("Lão hóa tồn kho", "Tuổi tồn kho");
        text = text.Replace("lão hóa tồn kho", "tuổi tồn kho");

        text = text.Replace("Chiều Hàng tồn kho", "Thuộc tính tồn kho");
        text = text.Replace("Chiều hàng tồn kho", "Thuộc tính tồn kho");
        text = text.Replace("chiều hàng tồn kho", "thuộc tính tồn kho");

        text = text.Replace("Nghỉ việc nhân viên", "Nhân viên nghỉ việc");
        text = text.Replace("nghỉ việc nhân viên", "nhân viên nghỉ việc");

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
