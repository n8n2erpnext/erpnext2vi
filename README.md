# ERPNext Ecosystem Localization Guide (English → Vietnamese)

Repository này chứa các tệp dịch thuật (`.po`) đã được chuẩn hóa ngữ cảnh doanh nghiệp, hạ tầng DevOps và hệ thống ERPNext dành cho các doanh nghiệp vừa và nhỏ (SME).

---

## 1. Nguyên Tắc Dịch Thuật & Chuẩn Hóa Ngữ Cảnh

Để đảm bảo tính nhất quán trên toàn bộ hệ sinh thái, các bản dịch tuân thủ nghiêm ngặt các nguyên tắc sau:

### Giữ nguyên các thuật ngữ kỹ thuật toàn cầu (English technical terms)
Tránh việc dịch thô/dịch máy gây khó hiểu cho người vận hành (operator).
- `hàng đợi` $\rightarrow$ **`queue`** (ví dụ: `đưa vào queue`, `queue email`)
- `máy chủ` $\rightarrow$ **`server`** (ví dụ: `server xác thực`, `phía server`)
- `tên miền` $\rightarrow$ **`domain`** (ví dụ: `domain email`, `domain đang hoạt động`)
- `móc web` $\rightarrow$ **`webhook`** (ví dụ: `webhook chấm công`)
- `bảng điều khiển` (trong ngữ cảnh IT/Dashboard) $\rightarrow$ **`dashboard` / `console`** (ví dụ: `dashboard trạm làm việc`, `console trình duyệt`)
- Các thuật ngữ DevOps khác: `Docker`, `container`, `API`, `topology`, `cluster`, `proxy`, `gateway`, `token`, `session`, `pipeline`, `edge`, `node`, `audit`, `replay`, `offline`, `online`, `realtime`, `Zero Trust`, `NetBird`, `PostgreSQL`, `Redis`, `SQLite`, `n8n`.

### Sửa đổi các thuật ngữ Nghiệp vụ & ERP dịch sai (Business & ERP)
- **Subcontracting (Gia công ngoài)**: Thay thế hoàn toàn các bản dịch máy sai lệch từ `"ký gửi" / "ký gửi phụ"` sang đúng nghĩa nghiệp vụ ERP:
  - `Job Worker` $\rightarrow$ **`Đơn vị gia công`** (thay vì "Công nhân ký gửi")
  - `Subcontracting Order` $\rightarrow$ **`Đơn đặt gia công`**
  - `Subcontracting Inward Order` $\rightarrow$ **`Đơn nhận gia công`**
  - `Subcontracting Receipt` $\rightarrow$ **`Biên nhận gia công`**
  - `Subcontracted Item` $\rightarrow$ **`Mặt hàng gia công ngoài`**
- **Common Party**: Sửa đổi `"Đảng chung"` $\rightarrow$ **`Đối tác chung`** (đối tác vừa là khách hàng vừa là nhà cung cấp).
- **Ledger Health**: Sửa đổi `"Sức khỏe sổ cái"` $\rightarrow$ **`Trạng thái sổ cái`** / **`Giám sát trạng thái`** để phù hợp với ngữ cảnh kỹ thuật.
- **Reconciliation**: Sử dụng **`Đối soát`**.
- **Audit log**: Sử dụng **`Nhật ký kiểm toán`**.

---

## 2. Danh Sách Các File Dịch & Module Tương Ứng

| Tệp Dịch | Module/Ứng Dụng Frappe | Đường Dẫn Cài Đặt Trong App |
| :--- | :--- | :--- |
| **`frappe_vi.po`** | Khung phát triển Frappe Framework | `apps/frappe/frappe/locale/vi.po` |
| **`erpnext_vi.po`** | Lõi ERPNext (Mua/Bán/Kho/Kế toán...) | `apps/erpnext/erpnext/locale/vi.po` |
| **`hrms_vi.po`** | Module Nhân sự & Chấm công (HRMS) | `apps/hrms/hrms/locale/vi.po` |
| **`crm_vi.po`** | Module Quản lý quan hệ khách hàng (CRM) | `apps/crm/crm/locale/vi.po` |
| **`helpdesk_vi.po`** | Module Hỗ trợ kỹ thuật (Helpdesk) | `apps/helpdesk/helpdesk/locale/vi.po` |
| **`insights_vi.po`** | Module Báo cáo thông minh (Insights) | `apps/insights/insights/locale/vi.po` |

---

## 3. Hướng Dẫn Cài Đặt File Dịch Vào Frappe / ERPNext

### Bước 1: Sao chép tệp dịch vào đúng module tương ứng
Sao chép đè các file `.po` trong repository này vào các thư mục locale tương ứng của từng app trong thư mục cài đặt ERPNext của bạn:

```bash
# Ví dụ sao chép cho module erpnext và frappe
cp erpnext_vi.po apps/erpnext/erpnext/locale/vi.po
cp frappe_vi.po apps/frappe/frappe/locale/vi.po
cp hrms_vi.po apps/hrms/hrms/locale/vi.po
cp crm_vi.po apps/crm/crm/locale/vi.po
cp helpdesk_vi.po apps/helpdesk/helpdesk/locale/vi.po
cp insights_vi.po apps/insights/insights/locale/vi.po
```

### Bước 2: Xóa bộ nhớ cache của hệ thống
Frappe lưu trữ bản dịch trong cache để tăng tốc độ tải trang. Bạn cần xóa cache để áp dụng bản dịch mới:

```bash
bench --site [ten-site-cua-ban] clear-cache
```

### Bước 3: Khởi động lại dịch vụ Bench
Khởi động lại các tiến độ nền để tải lại cấu trúc dịch thuật mới:

```bash
bench restart
```
*(Nếu chạy bằng Docker/Production, hãy reload hoặc restart các container tương ứng).*

---

## 4. Công Cụ Xác Thực Dịch Thuật (Validation Suite)

Để kiểm tra chất lượng file dịch trước khi commit hoặc đưa vào sử dụng, repository cung cấp sẵn một kịch bản xác thực tự động thông qua **`validate_translations.ps1`**. Kịch bản thực hiện 5 bài kiểm tra cốt lõi:

1. **Placeholder Validation (Kiểm tra tham số)**:
   Xác thực xem toàn bộ các tham số như `{0}`, `{name}` hoặc các biến định dạng dạng `%s`, `%d` trong `msgid` có được giữ nguyên và xuất hiện đầy đủ trong `msgstr` hay không.
2. **Jinja Validation (Kiểm tra thẻ Jinja)**:
   Đảm bảo các thẻ biểu thức Jinja `{{ ... }}` và các thẻ cấu trúc điều khiển `{% ... %}` (như `{% if comments %}`) không bị dịch sai cú pháp (ví dụ tránh lỗi dịch sai thành `{% nếu nhận xét %}`).
3. **JS Template Validation (Kiểm tra JS Template)**:
   Xác minh các biểu thức JavaScript template string dạng `${type}` được giữ nguyên ký tự biến để tránh phát sinh lỗi `ReferenceError` khi chạy trên giao diện trình duyệt.
4. **Forbidden Literal Translation Detection (Phát hiện dịch thô bị cấm)**:
   Tự động cảnh báo khi phát hiện các từ dịch thô/dịch máy bị cấm như `"hàng đợi"`, `"máy chủ"`, `"móc web"`, `"Đảng chung"` và đề xuất thay bằng thuật ngữ chuẩn.
5. **msgfmt Compile Test (Kiểm tra biên dịch gettext)**:
   Tự động phát hiện và gọi công cụ biên dịch chuẩn gettext `msgfmt` để biên dịch thử các tệp `.po` thành `.mo`, đảm bảo không có lỗi cú pháp định dạng file nào trước khi hệ thống Frappe nạp file.

### Cách chạy kiểm tra:
Trong PowerShell, chạy lệnh sau tại thư mục chứa repository:
```powershell
powershell -ExecutionPolicy Bypass -File validate_translations.ps1
```
