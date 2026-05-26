using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class PoMerger
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

    private static Dictionary<string, string> customDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // --- ERPNext Missing ---
        { "Account is not set for the dashboard chart {0}", "Tài khoản chưa được thiết lập cho biểu đồ dashboard {0}" },
        { "Account {0} does not exists in the dashboard chart {1}", "Tài khoản {0} không tồn tại trong biểu đồ dashboard {1}" },
        { "Action If Same Rate is Not Maintained", "Hành động nếu không duy trì cùng một đơn giá" },
        { "All Parties ", "Tất cả các bên " },
        { "Allow Item To Be Added Multiple Times in a Transaction", "Cho phép thêm mặt hàng nhiều lần trong một giao dịch" },
        { "Allow Negative rates for Items", "Cho phép đơn giá âm đối với các mặt hàng" },
        { "Amount in Words", "Số tiền bằng chữ" },
        { "Asset Movement record {0} created", "Bản ghi điều chuyển tài sản {0} đã được tạo" },
        { "Auto Create Assets on Purchase", "Tự động tạo tài sản khi mua hàng" },
        { "Auto Create Purchase Receipt", "Tự động tạo Phiếu nhập hàng" },
        { "Auto Create Subcontracting Order", "Tự động tạo đơn đặt gia công" },
        { "BOM Stock Report", "Báo cáo tồn kho BOM" },
        { "Backflush Raw Materials of Subcontract Based On", "Hoàn nguyên nguyên vật liệu gia công dựa trên" },
        { "Bill for Rejected Quantity in Purchase Invoice", "Hóa đơn cho số lượng bị từ chối trong hoá đơn mua hàng" },
        { "Cannot set quantity less than delivered quantity", "Không thể đặt số lượng nhỏ hơn số lượng đã giao" },
        { "Cannot set quantity less than received quantity", "Không thể đặt số lượng nhỏ hơn số lượng đã nhận" },
        { "Contact: ", "Liên hệ: " },
        { "Disable Last Purchase Rate", "Vô hiệu hóa đơn giá mua cuối cùng" },
        { "Grand Total (Company Currency", "Tổng cộng (Tiền tệ công ty)" },
        { "How often should Project be updated of Total Purchase Cost ?", "Tần suất cập nhật tổng chi phí mua hàng cho dự án?" },
        { "Invoice ID", "ID hóa đơn" },
        { "Is Old Subcontracting Flow", "Là quy trình gia công cũ" },
        { "Is Purchase Order Required for Purchase Invoice & Receipt Creation?", "Yêu cầu Đơn mua hàng khi tạo hoá đơn mua hàng & Phiếu nhập hàng?" },
        { "Is Purchase Receipt Required for Purchase Invoice Creation?", "Yêu cầu Phiếu nhập hàng khi tạo hoá đơn mua hàng?" },
        { "Item {0} must be a Sub-contracted Item", "Mặt hàng {0} phải là mặt hàng gia công ngoài" },
        { "Maintain Same Rate Throughout the Purchase Cycle", "Duy trì cùng một đơn giá trong suốt chu kỳ mua hàng" },
        { "Minimum quantity should be as per Stock UOM", "Số lượng tối thiểu phải theo đơn vị tính kho" },
        { "Mobile: ", "Di động: " },
        { "Naming Series and Price Defaults", "Chuỗi đặt tên và giá mặc định" },
        { "Particulars", "Chi tiết" },
        { "Payment Status", "Trạng thái thanh toán" },
        { "Please select BOM in BOM field for Item {item_code}.", "Vui lòng chọn BOM trong trường BOM cho mặt hàng {item_code}." },
        { "Please select Subcontracting Order instead of Purchase Order {0}", "Vui lòng chọn đơn đặt gia công thay vì Đơn mua hàng {0}" },
        { "Please select a valid Purchase Order that has Service Items.", "Vui lòng chọn Đơn mua hàng hợp lệ có các mặt hàng dịch vụ." },
        { "Purchase Order Item Supplied", "Mặt hàng cung cấp theo Đơn mua hàng" },
        { "Reserved Warehouse is mandatory for the Item {item_code} in Raw Materials supplied.", "Kho giữ hàng là bắt buộc đối với mặt hàng {item_code} trong nguyên vật liệu được cung cấp." },
        { "Row #{0}: BOM is not specified for subcontracting item {0}", "Dòng #{0}: BOM chưa được chỉ định cho mặt hàng gia công {0}" },
        { "Set Valuation Rate for Rejected Materials", "Đặt tỷ giá định giá cho nguyên vật liệu bị từ chối" },
        { "Show Pay Button in Purchase Order Portal", "Hiển thị nút thanh toán trên cổng Đơn mua hàng" },
        { "Sub Total", "Tổng phụ" },
        { "Subcontract BOM", "BOM gia công" },
        { "Subscription Section", "Phần đăng ký" },
        { "Supplier Items", "Mặt hàng của nhà cung cấp" },
        { "Supplier-Wise Sales Analytics", "Phân tích doanh số theo nhà cung cấp" },
        { "Tax Id: ", "Mã số thuế: " },
        { "UOMs", "Đơn vị tính" },
        { "Units of Measure", "Đơn vị tính" },
        { "Until", "Đến khi" },
        { "Update frequency of Project", "Tần suất cập nhật dự án" },
        { "Validate Consumed Qty (as per BOM)", "Xác thực số lượng đã tiêu thụ (theo BOM)" },
        { "{field_label} is mandatory for sub-contracted {doctype}.", "{field_label} là bắt buộc đối với {doctype} gia công ngoài." },

        // --- Frappe Missing ---
        { "<a href=\"https://docs.frappe.io/framework/user/en/api/rest#1-token-based-authentication\" target=\"_blank\">\n  Click here to learn about token-based authentication\n</a>", "<a href=\"https://docs.frappe.io/framework/user/en/api/rest#1-token-based-authentication\" target=\"_blank\">\n  Nhấp vào đây để tìm hiểu về xác thực dựa trên token\n</a>" },
        { "<p><strong>Condition Examples:</strong></p>\n<pre><code class=\"language-python\">doc.status==\"Open\"<br>doc.due_date==nowdate()<br>doc.total &gt; 40000\n</code></pre>\n", "<p><strong>Ví dụ điều kiện:</strong></p>\n<pre><code class=\"language-python\">doc.status==\"Open\"<br>doc.due_date==nowdate()<br>doc.total &gt; 40000\n</code></pre>\n" },
        { "<p><strong>Condition Examples:</strong></p>\n<pre>doc.status==\"Open\"<br>doc.due_date==nowdate()<br>doc.total &gt; 40000\n</pre>", "<p><strong>Ví dụ điều kiện:</strong></p>\n<pre>doc.status==\"Open\"<br>doc.due_date==nowdate()<br>doc.total &gt; 40000\n</pre>" },
        { "Bulk Actions", "Hành động hàng loạt" },
        { "Deleted all documents successfully", "Đã xóa tất cả tài liệu thành công" },
        { "Existing Role", "Role hiện có" },
        { "Google Calender", "Google Calendar" },
        { "Input existing role name if you would like to extend it with access of another role.", "Nhập tên Role hiện tại nếu bạn muốn mở rộng vai trò đó với quyền truy cập của Role khác." },
        { "Invalid expression set in filter {0}", "Biểu thức không hợp lệ được đặt trong Filter {0}" },
        { "Mobile No", "Số di động" },
        { "Navigation Buttons", "Nút điều hướng" },
        { "New Role", "Role mới" },
        { "Note: Multiple sessions will be allowed in case of mobile device", "Lưu ý: Nhiều Session sẽ được cho phép đối với thiết bị di động" },
        { "Query must be of SELECT or read-only WITH type.", "Truy vấn phải là loại SELECT hoặc WITH chỉ đọc." },
        { "Queued for Submission. You can track the progress over {0}.", "Đã xếp vào Queue để gửi. Bạn có thể theo dõi tiến độ tại {0}." },
        { "Reload File", "Làm mới tệp" },
        { "Replicating...", "Đang sao chép..." },
        { "Replication completed.", "Sao chép hoàn thành." },
        { "Report cannot be set for Single types", "Không thể đặt báo cáo cho các loại Single" },
        { "Role Replication", "Sao chép Role" },
        { "Row # {0}: Non administrator user can not set the role {1} to the custom doctype", "Dòng # {0}: Người dùng không phải administrator không thể đặt Role {1} cho doctype tùy chỉnh" },
        { "Search Bar", "Thanh tìm kiếm" },
        { "These announcements will appear inside a dismissible alert below the Navbar.", "Các thông báo này sẽ xuất hiện trong một cảnh báo có thể bỏ qua bên dưới Navbar." },
        { "This document has already been queued for submission. You can track the progress over {0}.", "Tài liệu này đã được đưa vào Queue để gửi. Bạn có thể theo dõi tiến độ tại {0}." },
        { "Timeline", "Lịch sử" },
        { "To use Slack Channel, add a <a href=\"#List/Slack%20Webhook%20URL/List\">Slack Webhook URL</a>.", "Để sử dụng Kênh Slack, hãy thêm <a href=\"#List/Slack%20Webhook%20URL/List\">Slack Webhook URL</a>." },
        { "View Switcher", "Bộ chuyển đổi chế độ xem" },
        { "With Letter head", "Có Letterhead" },
        { "clear", "xóa" },
        { "{0}: Cannot set Amend without Cancel", "{0}: Không thể đặt hiệu chỉnh (Amend) mà không Hủy (Cancel)" },
        { "{0}: Cannot set Assign Amend if not Submittable", "{0}: Không thể chỉ định hiệu chỉnh nếu không thể Gửi (Submittable)" },
        { "{0}: Cannot set Assign Submit if not Submittable", "{0}: Không thể chỉ định Gửi nếu không thể Gửi (Submittable)" },
        { "{0}: Cannot set Cancel without Submit", "{0}: Không thể Hủy (Cancel) mà không Gửi (Submit)" },
        { "{0}: Cannot set Import without Create", "{0}: Không thể Import mà không Tạo (Create)" },
        { "{0}: Cannot set Submit, Cancel, Amend without Write", "{0}: Không thể Gửi, Hủy, Hiệu chỉnh mà không có quyền Ghi (Write)" },
        { "{0}: Cannot set import as {1} is not importable", "{0}: Không thể đặt nhập vì {1} không thể Import (not importable)" },

        // --- HRMS Missing ---
        { "Advance Account is mandatory. Please set the <a href=\"/app/company/{0}#default_employee_advance_account\" target=\"_blank\">Default Employee Advance Account</a> in the Company record {0} and submit this document.", "Tài khoản tạm ứng là bắt buộc. Vui lòng thiết lập <a href=\"/app/company/{0}#default_employee_advance_account\" target=\"_blank\">Tài khoản tạm ứng nhân viên mặc định</a> trong bản ghi Công ty {0} và gửi tài liệu này." },
        { "Advance Paid (Company Currency)", "Tạm ứng đã thanh toán (Tiền tệ công ty)" },
        { "Allow Leave Application After (Working Days)", "Cho phép nộp Đơn nghỉ phép sau (Số ngày làm việc)" },
        { "Base & Variable", "Lương cơ bản & Lương biến đổi" },
        { "Choose how the hourly overtime amount is calculated:\n<ol style=\"padding-left:15px;\"><li>Fixed Hourly Rate: A fixed, manually entered hourly rate.</li>\n<li>Salary Component-Based:\n\n(Sum of selected component amounts) ÷ (Payment Days) ÷ (Standard Daily Hours)</li></ol>", "Chọn cách tính số tiền tăng ca theo giờ:\n<ol style=\"padding-left:15px;\"><li>Đơn giá giờ cố định: Đơn giá giờ cố định được nhập thủ công.</li>\n<li>Dựa trên thành phần lương:\n\n(Tổng số tiền thành phần được chọn) ÷ (Số ngày thanh toán) ÷ (Số giờ làm việc tiêu chuẩn hàng ngày)</li></ol>" },
        { "Cleared", "Đã đối soát" },
        { "Employee Journey", "Hành trình nhân viên" },
        { "Enter Interview Round", "Nhập vòng phỏng vấn" },
        { "HR Dashboard", "Dashboard nhân sự" },
        { "Interview Round", "Vòng phỏng vấn" },
        { "Interview Round {0} is only applicable for the Designation {1}", "Vòng phỏng vấn {0} chỉ áp dụng cho Chức vụ {1}" },
        { "Interview Round {0} is only for Designation {1}. Job Applicant has applied for the role {2}", "Vòng phỏng vấn {0} chỉ dành cho Chức vụ {1}. Ứng viên đã ứng tuyển vào Role {2}" },
        { "Is Optional Leave", "Là nghỉ phép tự chọn" },
        { "Job Applicants are not allowed to appear twice for the same Interview round. Interview {0} already scheduled for Job Applicant {1}", "Ứng viên không được phép phỏng vấn hai lần cho cùng một vòng. Phỏng vấn {0} đã được lên lịch cho Ứng viên {1}" },
        { "Job Application Route", "Quy trình ứng tuyển công việc" },
        { "Leave and Expense Claim Settings", "Cài đặt nghỉ phép và đề nghị thanh toán chi phí" },
        { "Minimum working days required since Date of Joining to apply for this leave", "Số ngày làm việc tối thiểu kể từ Ngày gia nhập để được xin nghỉ phép này" },
        { "No attendance records to create", "Không có bản ghi chấm công nào để tạo" },
        { "Others", "Khác" },
        { "Payroll Dashboard", "Dashboard bảng lương" },
        { "Please check if employee is on leave or attendance with the same status exists for selected day(s).", "Vui lòng kiểm tra xem nhân viên có đang nghỉ phép hoặc dữ liệu chấm công cùng trạng thái đã tồn tại cho (các) ngày được chọn hay không." },
        { "Posted On", "Được đăng vào" },
        { "Promotion", "Thăng tiến" },
        { "Resume Attachment", "Đính kèm CV" },
        { "Round Name", "Tên vòng phỏng vấn" },
        { "Select Interview Round First", "Chọn vòng phỏng vấn trước" },
        { "Source and Rating", "Nguồn và Đánh giá" },
        { "This method is only meant for developer mode", "Phương thức này chỉ dành cho chế độ nhà phát triển (developer mode)" },
        { "Unlink Payment", "Hủy liên kết thanh toán" },
        { "Uploading...", "Đang tải lên..." },
        { "Variable", "Biến đổi" },
        { "{0} applicable after {1} working days", "{0} áp dụng sau {1} ngày làm việc" },
        { "{} ", "{} " },
        { "{} Accepted", "{} Đã chấp nhận" },
        { "{} Active", "{} Đang hoạt động" },
        { "{} Draft", "{} Nháp" },
        { "{} Unclaimed", "{} Chưa được nhận" }
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

    public static void Merge(string potPath, string poPath, string outputPath)
    {
        List<string> poHeaders;
        List<PoEntry> poEntries = Parse(poPath, out poHeaders);
        
        Dictionary<string, string> translations = new Dictionary<string, string>();
        foreach (var entry in poEntries)
        {
            if (string.IsNullOrEmpty(entry.MsgId)) continue;
            translations[entry.MsgId] = entry.MsgStr;
        }

        List<string> potHeaders;
        List<PoEntry> potEntries = Parse(potPath, out potHeaders);

        int matchedCount = 0;
        int customMatchedCount = 0;
        int glossaryMatchedCount = 0;
        int missingCount = 0;

        foreach (var entry in potEntries)
        {
            if (string.IsNullOrEmpty(entry.MsgId)) continue;

            if (translations.ContainsKey(entry.MsgId) && !string.IsNullOrEmpty(translations[entry.MsgId]))
            {
                entry.MsgStr = translations[entry.MsgId];
                matchedCount++;
            }
            else if (customDict.ContainsKey(entry.MsgId))
            {
                entry.MsgStr = customDict[entry.MsgId];
                customMatchedCount++;
            }
            else if (dictExact.ContainsKey(entry.MsgId))
            {
                entry.MsgStr = dictExact[entry.MsgId];
                glossaryMatchedCount++;
            }
            else
            {
                entry.MsgStr = "";
                missingCount++;
            }
        }

        using (StreamWriter sw = new StreamWriter(outputPath, false, Encoding.UTF8))
        {
            foreach (var line in poHeaders)
            {
                sw.WriteLine(line);
            }
            foreach (var entry in potEntries)
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

        Console.WriteLine(string.Format("Merged {0} (POT: {1} entries). Matched: {2}, Custom: {3}, Glossary Fallback: {4}, Missing: {5}", 
            Path.GetFileName(poPath), potEntries.Count, matchedCount, customMatchedCount, glossaryMatchedCount, missingCount));
    }
}
