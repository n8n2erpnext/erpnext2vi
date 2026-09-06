# Frappe Health / Healthcare — Vietnamese Localization

`healthcare_vi_v2.po` là catalog tiếng Việt dành cho **Frappe Health / Marley Healthcare v16**. File này chỉ phục vụ lớp **giao diện + workflow y tế**, không phải một bộ dữ liệu y khoa Việt Nam đóng cứng.

## Phạm vi

Catalog chuẩn hóa các khái niệm vận hành thường gặp như Bệnh nhân, Lịch hẹn, Lượt khám bệnh, Nhập viện, Xuất viện, Giường bệnh, Điều dưỡng, Chỉ định dịch vụ, Thủ thuật y khoa, Xét nghiệm, Bệnh phẩm, Dấu hiệu sinh tồn, Y lệnh thuốc, Phục hồi chức năng, Vật lý trị liệu, Bảo hiểm và các thiết lập cơ sở y tế.

Một số mapping chủ đích theo ngữ cảnh Việt Nam:

| English | Tiếng Việt |
| --- | --- |
| Patient | Bệnh nhân |
| Patient Encounter | Lượt khám bệnh |
| Appointment | Lịch hẹn |
| Admission / Discharge | Nhập viện / Xuất viện |
| Practitioner | Nhân viên y tế |
| Clinical Procedure | Thủ thuật y khoa |
| Lab Test | Xét nghiệm |
| Specimen | Bệnh phẩm |
| Vital Signs | Dấu hiệu sinh tồn |
| Medication Order | Y lệnh thuốc |
| Service Request | Chỉ định dịch vụ |
| Triage | Phân loại cấp cứu |
| Rehabilitation | Phục hồi chức năng |
| Physiotherapy | Vật lý trị liệu |

## Những gì cố ý không đóng gói vào file dịch

Tên thuốc, hoạt chất, nhóm dược lý, bệnh, mã ICD, mã thủ thuật, tên xét nghiệm, đơn vị xét nghiệm, MRI/X-ray/CT, danh mục vật tư và các master y khoa của từng cơ sở **không nên hardcode vào translation catalog**. Chúng là master/config/import data và phải được triển khai theo bệnh viện/phòng khám cụ thể.

Các thuật ngữ quốc tế đã thành chuẩn sử dụng như MRI, X-ray, CT, ICD có thể giữ nguyên khi phù hợp.

## Cài đặt

```bash
cp healthcare_vi_v2.po apps/healthcare/healthcare/locale/vi.po
bench compile-po-to-mo
bench --site [site-name] clear-cache
```

Sau khi deploy, hard refresh trình duyệt hoặc đăng xuất/đăng nhập lại.

## Ranh giới với Hospitality

Healthcare quản lý **hồ sơ và workflow lâm sàng**. Hospitality Core quản lý **lưu trú, phòng, folio, housekeeping, rate plan, front desk và night audit**. Với cơ sở điều dưỡng/phục hồi chức năng có lưu trú dài ngày, hai app có thể dùng cùng nhau nhưng không nên gộp nghĩa nghiệp vụ của chúng vào một catalog.
