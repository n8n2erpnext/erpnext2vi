# Hospitality Core — Vietnamese Localization

`hospitality_core_vi_v2.po` là catalog tiếng Việt cho **Hospitality Core** trên ERPNext/Frappe v16, tập trung vào nghiệp vụ hotel/resort/PMS.

## Phạm vi

Catalog chuẩn hóa các thuật ngữ Front Desk, Guest, Reservation, Room, Check-in, Check-out, Folio, Master Folio, Housekeeping, Minibar, Rate Plan, Corporate/Group Booking, City Ledger, Night Audit, POS bridge, room move, split billing và các báo cáo doanh thu/lưu trú.

Một số thuật ngữ được giữ nguyên vì đã phổ biến trong ngành: `ADR`, `RevPAR`, `POS`, `Folio` và các mã kỹ thuật liên quan.

## Ngữ cảnh dịch

| English | Tiếng Việt |
| --- | --- |
| Guest | Khách lưu trú |
| Reservation | Đặt phòng |
| Room | Phòng |
| Housekeeping | Buồng phòng |
| Rate Plan | Bảng giá lưu trú |
| Master Folio | Folio tổng |
| Night Audit | Kiểm toán cuối ngày |
| Front Desk | Lễ tân |

Không dùng bản dịch hotel để ép lên Healthcare. Ví dụ `Room` trong Hospitality là **Phòng**, còn không gian chăm sóc y tế có thể là Phòng bệnh/Giường bệnh/Đơn vị dịch vụ y tế tùy workflow.

## Dùng cho cơ sở chăm sóc có lưu trú

Hospitality Core có thể làm lớp quản lý lưu trú/dịch vụ cho trung tâm điều dưỡng, phục hồi chức năng hoặc clinic có lưu trú: phòng/giường, check-in/out, housekeeping, dịch vụ, folio và billing. Hồ sơ bệnh án, y lệnh, xét nghiệm, thủ thuật và các dữ liệu lâm sàng vẫn thuộc Healthcare.

## Cài đặt

```bash
cp hospitality_core_vi_v2.po apps/hospitality_core/hospitality_core/locale/vi.po
bench compile-po-to-mo
bench --site [site-name] clear-cache
```

Sau khi deploy, hard refresh trình duyệt hoặc đăng xuất/đăng nhập lại.

## Lưu ý tương thích

Bản catalog này được kiểm thử với Hospitality Core nhánh `main` phiên bản app `0.0.1` trên Frappe 16.17 / ERPNext 16.16. Hospitality Core có override một số controller nghiệp vụ ERPNext nên cần test reservation → check-in → folio/POS → housekeeping → checkout → invoice/night audit trước khi đưa vào production.
