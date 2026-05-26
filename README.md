# ERPNext / Frappe Vietnamese Localization Guide & Translation Catalogs

Repository này chứa các tệp dịch thuật đã được chuẩn hóa phiên bản 2 (`*_vi_v2.po`) cho hệ sinh thái **Frappe/ERPNext v16**. Các bản dịch được tối ưu hóa đặc biệt theo đặc thù vận hành của các doanh nghiệp vừa và nhỏ (SME) tại Việt Nam, đảm bảo tính trực quan, tự nhiên và chuyên nghiệp.

---

## 1. Triết Lý & Nguyên Tắc Dịch Thuật

Bản dịch tuân thủ triết lý: **Không dịch word-by-word máy móc mà dịch theo ngữ cảnh và thói quen sử dụng thực tế của người dùng.**

### A. Những từ giữ nguyên ở Tiếng Anh (Technical / Operational Terms)
Các thuật ngữ kỹ thuật, hạ tầng hoặc giao diện đã quá phổ biến với người vận hành sẽ được giữ nguyên để tránh tạo cảm giác gượng gạo:
* **Hạ tầng & Hệ thống**: `ERP`, `CRM`, `POS`, `SLA`, `API`, `Zero Trust`, `Edge Gateway`, `Docker`, `Container`, `Stack`, `Deploy`, `MCP`, `Redis`, `PostgreSQL`, `NetBird`, `ERPNext`, `Frappe`.
* **Giao diện & UI**: `Dashboard`, `Workflow`, `Sync`, `Log`, `Serial`, `Batch`, `Import`, `Export`, `Filter`, `Theme`, `Plugin`, `Widget`, `Webhook`, `Queue`, `Cache`, `AI Agent`, `Audit`.
* **Xác thực & Bảo mật**: `Role`, `Permission`, `Session`, `Token`.

### B. Những từ dịch Việt hóa (Sát với SME Việt Nam)
Các thuật ngữ nghiệp vụ và giao diện điều hướng được dịch sang tiếng Việt tự nhiên và chuẩn văn phong doanh nghiệp:

| Phân hệ / Module | Thuật ngữ gốc | Bản dịch chuẩn Việt hóa |
| :--- | :--- | :--- |
| **System UI & Navigation** | Desktop | Tổng quan |
| | Workspace | Khu làm việc |
| | Reload | Làm mới |
| | Global Defaults | Thiết lập chung |
| | User Settings | Thiết lập tài khoản |
| | Assign To | Giao việc |
| | Attachments | Tài liệu đính kèm |
| | Activity | Nhật ký hoạt động |
| | Timeline | Lịch sử |
| | Rename | Đổi mã |
| | Customize | Tuỳ biến |
| | Getting Started | Hướng dẫn nhanh |
| **CRM** | Lead / Prospect | Khách hàng tiềm năng / Khách tiềm năng |
| | Deal / Opportunity | Cơ hội bán hàng / Cơ hội kinh doanh |
| | Contact / Note / Campaign | Liên hệ / Ghi chú / Chiến dịch |
| | Pipeline / Stage / Lost Reason | Quy trình bán hàng / Giai đoạn / Lý do thất bại |
| **HR / HRMS** | Employee / Attendance / Shift | Nhân viên / Chấm công / Ca làm |
| | Leave Application | Đơn nghỉ phép |
| | Payroll / Salary Slip | Bảng lương / Phiếu lương |
| | Expense Claim | Đề nghị thanh toán |
| | Department / Designation | Phòng ban / Chức vụ |
| | Holiday List | Lịch nghỉ |
| | Check In / Check Out | Ghi nhận vào ca / Ghi nhận ra ca |
| **Kho / Stock** | Stock Entry | Phiếu kho |
| | Material Request | Yêu cầu vật tư |
| | Delivery Note / Purchase Receipt | Phiếu giao hàng / Phiếu nhập hàng |
| | Pick List | Danh sách lấy hàng |
| | Item / Item Group | Mặt hàng / Nhóm mặt hàng |
| | Warehouse | Kho |
| | Reorder Level | Mức tồn tối thiểu |
| | Stock Ledger / Stock Aging | Sổ kho / Tuổi tồn kho |
| | Stock Reconciliation / UOM | Đối soát tồn kho / Đơn vị tính |
| **Buying** | Supplier | Nhà cung cấp |
| | Request for Quotation | Yêu cầu báo giá |
| | Supplier Quotation | Báo giá nhà cung cấp |
| | Purchase Order / Purchase Invoice | Đơn mua hàng / Hoá đơn mua hàng |
| | Landed Cost Voucher | Chi phí nhập hàng |
| **Selling** | Customer / Quotation | Khách hàng / Báo giá |
| | Sales Order / Sales Invoice | Đơn bán hàng / Hoá đơn bán hàng |
| | Pricing Rule / Territory | Chính sách giá / Khu vực bán hàng |
| **Accounting** | Journal Entry / Payment Entry | Bút toán / Phiếu thanh toán |
| | Fiscal Year / Cost Center | Năm tài chính / Trung tâm chi phí |
| | Chart of Accounts | Hệ thống tài khoản |
| | General Ledger / Outstanding Amount | Sổ cái / Công nợ còn lại |
| | Receivable / Payable | Công nợ phải thu / Công nợ phải trả |
| **Manufacturing** | BOM | Định mức nguyên vật liệu |
| | Work Order / Job Card | Lệnh sản xuất / Phiếu công đoạn |
| | Production Plan / Routing | Kế hoạch sản xuất / Quy trình sản xuất |
| | Subcontracting | Gia công ngoài / Gia công |
| **Helpdesk & CMS** | Ticket / Issue / Resolution | Phiếu hỗ trợ / Sự cố / Hướng xử lý |
| | Escalation | Chuyển cấp xử lý |
| | Blog Post / Web Page / Landing Page | Bài viết / Trang nội dung / Trang giới thiệu |

---

## 2. Danh Sách Các File Dịch Phiên Bản 2 (v2)

| Tệp Dịch gốc (v2) | Ứng Dụng tương ứng | Đường Dẫn cài đặt trong App (trên VPS) |
| :--- | :--- | :--- |
| **`frappe_vi_v2.po`** | Frappe Framework | `apps/frappe/frappe/locale/vi.po` |
| **`erpnext_vi_v2.po`** | ERPNext | `apps/erpnext/erpnext/locale/vi.po` |
| **`hrms_vi_v2.po`** | HRMS (Nhân sự) | `apps/hrms/hrms/locale/vi.po` |
| **`crm_vi_v2.po`** | CRM (Khách hàng) | `apps/crm/crm/locale/vi.po` |
| **`insights_vi_v2.po`** | Insights (Báo cáo) | `apps/insights/insights/locale/vi.po` |

---

## 3. Hướng Dẫn Cài Đặt File Dịch Vào Frappe / ERPNext

### Bước 1: Sao chép file dịch `.po` vào đúng vị trí
Sao chép các file bản dịch `_v2` trong repository này vào các thư mục locale tương ứng của từng app (lưu ý đổi tên tệp đích thành `vi.po`):

```bash
cp erpnext_vi_v2.po apps/erpnext/erpnext/locale/vi.po
cp frappe_vi_v2.po apps/frappe/frappe/locale/vi.po
cp hrms_vi_v2.po apps/hrms/hrms/locale/vi.po
cp crm_vi_v2.po apps/crm/crm/locale/vi.po
cp insights_vi_v2.po apps/insights/insights/locale/vi.po
```

### Bước 2: Biên dịch các file `.po` sang `.mo` (Quan trọng)
Frappe v16 đọc bản dịch từ file nhị phân compiled `.mo` để tối ưu hiệu năng. Bạn cần chạy lệnh biên dịch:
```bash
bench compile-po-to-mo
```

### Bước 3: Di trú Cơ sở dữ liệu và Xóa cache
Nhiều nhãn DocType, Sidebar và Workspace được nạp trực tiếp vào cơ sở dữ liệu. Cần chạy lệnh migrate để cập nhật các nhãn này sang tiếng Việt, sau đó clear cache:
```bash
bench --site [ten-site-cua-ban] migrate
bench --site [ten-site-cua-ban] clear-cache
```

*Lưu ý: Sau khi thực hiện xong, bạn hãy thực hiện **Hard Refresh (Ctrl + F5 / Ctrl + Shift + R)** hoặc đăng xuất và đăng nhập lại trên trình duyệt để nạp file dịch thuật JS mới.*

---

## 4. Công Cụ Hỗ Trợ & Xác Thực (Validation Suite)

Repository này đi kèm với các công cụ tự động phục vụ việc cập nhật và kiểm tra lỗi định dạng file:
1. **`Translator.cs`**: Chương trình xử lý tự động để sửa các bản dịch máy thô, chuẩn hóa thuật ngữ tiếng Anh theo bộ quy tắc.
2. **`MergePo.cs`**: Công cụ đồng bộ hóa file `.pot` tiếng Anh mới của hệ thống với bản dịch hiện tại, tích hợp các cơ chế fallback thuật ngữ.
3. **`validate_translations.ps1`**: Bộ kiểm tra tự động trước khi commit. Chạy lệnh sau để kiểm tra lỗi biến truyền, cú pháp Jinja/JS và phát hiện các từ dịch thô bị cấm:
   ```powershell
   powershell -ExecutionPolicy Bypass -File validate_translations.ps1
   ```
