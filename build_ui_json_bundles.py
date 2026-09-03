#!/usr/bin/env python3
import ast, html, json, re, sys
from pathlib import Path

ROOT=Path(__file__).resolve().parent
WORK=ROOT/'.ui-export-work'
OUT=ROOT/'ui_json'
PO_FILES=['frappe_vi_v2.po','erpnext_vi_v2.po','hrms_vi_v2.po','crm_vi_v2.po','insights_vi_v2.po']

MANUAL={
'Active RQ Worker':'RQ Worker đang hoạt động','Background Job Activity':'Hoạt động tác vụ nền','Email Activity':'Hoạt động email','Notifications By Type':'Thông báo theo loại','Scheduled Jobs':'Tác vụ theo lịch','Total System Users':'Tổng người dùng hệ thống','Total Website Users':'Tổng người dùng trang web','Webpage Views':'Lượt xem trang web',
'Accounts Payable Ageing':'Tuổi nợ phải trả','Accounts Receivable Ageing':'Tuổi nợ phải thu','Active Customers':'Khách hàng đang hoạt động','Active Suppliers':'Nhà cung cấp đang hoạt động','Annual Purchase':'Mua hàng theo năm','Annual Sales':'Bán hàng theo năm','Average Sales Order Value':'Giá trị đơn bán hàng trung bình','Delivery Trends':'Xu hướng giao hàng','Incoming Bills (Purchase Invoice)':'Hóa đơn mua hàng (đầu vào)','Incoming Leads':'Khách hàng tiềm năng mới','Item Shortage Summary':'Tổng hợp thiếu hụt mặt hàng','Item-wise Annual Sales':'Doanh số năm theo mặt hàng','Location-wise Asset Value':'Giá trị tài sản theo địa điểm','Material Request Analysis':'Phân tích yêu cầu vật tư','Oldest Items':'Mặt hàng tồn lâu nhất','Open Opportunity':'Cơ hội kinh doanh đang mở','Opportunities via Campaigns':'Cơ hội kinh doanh theo chiến dịch','Opportunity Trends':'Xu hướng cơ hội kinh doanh','Outgoing Bills (Sales Invoice)':'Hóa đơn bán hàng (đầu ra)','Sales Orders Count':'Số lượng đơn bán hàng','Territory Wise Opportunity Count':'Số cơ hội kinh doanh theo khu vực bán hàng','Territory Wise Sales':'Doanh số theo khu vực bán hàng','Timesheet Working Hours':'Giờ làm việc theo bảng chấm công','Top Customers':'Khách hàng hàng đầu','Top Suppliers':'Nhà cung cấp hàng đầu','Total Incoming Bills':'Tổng hóa đơn mua hàng','Total Incoming Payment':'Tổng tiền thu','Total Outgoing Bills':'Tổng hóa đơn bán hàng','Total Outgoing Payment':'Tổng tiền chi','Warehouse wise Stock Value':'Giá trị tồn kho theo kho',
'Accepted Job Applicants':'Ứng viên được chấp nhận','Active Employees':'Nhân viên đang làm việc','Applicant-to-Hire Percentage':'Tỷ lệ ứng viên được tuyển','Approved Claims (This Month)':'Đề nghị thanh toán đã duyệt (tháng này)','Claims by Type':'Đề nghị thanh toán theo loại','Department Wise Employee Count':'Số nhân viên theo phòng ban','Department Wise Salary(Last Month)':'Lương tháng trước theo phòng ban','Department wise Expense Claims':'Đề nghị thanh toán theo phòng ban','Department wise Timesheet Hours':'Giờ làm việc theo phòng ban','Designation Wise Employee Count':'Số nhân viên theo chức vụ','Designation Wise Openings':'Vị trí tuyển dụng theo chức vụ','Designation Wise Salary(Last Month)':'Lương tháng trước theo chức vụ','Early Exit (This Month)':'Ra ca sớm (tháng này)','Employee Advance Status':'Trạng thái tạm ứng nhân viên','Employee Exits (This Year)':'Nhân viên nghỉ việc (năm nay)','Employee Lifecycle':'Vòng đời nhân viên','Employees Joining (This Quarter)':'Nhân viên vào làm (quý này)','Employees Relieving (This Quarter)':'Nhân viên nghỉ việc (quý này)','Employees by Age':'Nhân viên theo độ tuổi','Employees by Branch':'Nhân viên theo chi nhánh','Employees by Grade':'Nhân viên theo cấp bậc','Employees by Type':'Nhân viên theo loại','Expense Claims (This Month)':'Đề nghị thanh toán (tháng này)','Gender Diversity Ratio':'Tỷ lệ đa dạng giới tính','HR Setup':'Thiết lập nhân sự','Hiring vs Attrition Count':'Tuyển dụng và nghỉ việc','Holidays in this month':'Ngày nghỉ trong tháng này','Human Resource':'Nhân sự','Job Applicant Pipeline':'Quy trình ứng viên','Job Applicants by Country':'Ứng viên theo quốc gia','Job Application Frequency':'Tần suất ứng tuyển','Job Application Status':'Trạng thái đơn ứng tuyển','Job Offer Acceptance Rate':'Tỷ lệ chấp nhận đề nghị tuyển dụng','Job Offer Status':'Trạng thái đề nghị tuyển dụng','Job Offers (This Month)':'Đề nghị tuyển dụng (tháng này)','Late Entry (This Month)':'Vào ca muộn (tháng này)','New Hires (This Year)':'Nhân viên mới (năm nay)','Number of Employees on Leave (This Month)':'Số nhân viên nghỉ phép (tháng này)','Number of Employees on Leave (Today)':'Số nhân viên nghỉ phép (hôm nay)','Onboardings (This Month)':'Tiếp nhận nhân viên mới (tháng này)','Promotions (This Month)':'Thăng chức (tháng này)','Rejected Claims (This Month)':'Đề nghị thanh toán bị từ chối (tháng này)','Rejected Job Applicants':'Ứng viên bị từ chối','Separations (This Month)':'Nghỉ việc (tháng này)','Shift Assignment Breakup':'Phân bổ phân ca','Timesheet Activity Breakup':'Phân bổ hoạt động bảng chấm công','Total Absent (This Month)':'Tổng lượt vắng mặt (tháng này)','Total Applicants (This Month)':'Tổng ứng viên (tháng này)','Total Declaration Submitted':'Tổng tờ khai đã nộp','Total Incentive Given(Last month)':'Tổng tiền thưởng tháng trước','Total Outgoing Salary(Last month)':'Tổng lương chi tháng trước','Total Present (This Month)':'Tổng lượt có mặt (tháng này)','Total Salary Structure':'Tổng số cấu trúc lương','Training Type':'Loại đào tạo','Trainings (This Month)':'Đào tạo (tháng này)','Transfers (This Month)':'Điều chuyển (tháng này)','Y-O-Y Promotions':'Thăng chức theo năm','Y-O-Y Transfers':'Điều chuyển theo năm',
'Masters & Reports':'Danh mục & Báo cáo','Transactions & Reports':'Giao dịch & Báo cáo','Subcontracting Inward and Outward':'Nhận gia công và thuê gia công','PORTAL':'CỔNG THÔNG TIN','This module is scheduled for deprecation and will be completely removed in version 17, please use Frappe CRM instead.':'Phân hệ này dự kiến ngừng hỗ trợ và sẽ bị gỡ hoàn toàn ở phiên bản 17; vui lòng sử dụng Frappe CRM thay thế.','This module is scheduled for deprecation and will be completely removed in version 17, please use Frappe Helpdesk instead.':'Phân hệ này dự kiến ngừng hỗ trợ và sẽ bị gỡ hoàn toàn ở phiên bản 17; vui lòng sử dụng Frappe Helpdesk thay thế.'
}

MANUAL.update({'Average Order Values': 'Giá trị đơn hàng trung bình', 'Open Work Orders': 'Lệnh sản xuất đang mở', 'Purchase Orders to Bill': 'Đơn mua hàng chờ lập hóa đơn', 'Sales Orders to Bill': 'Đơn bán hàng chờ lập hóa đơn', 'Sales Orders to Deliver': 'Đơn bán hàng chờ giao', 'Time to Fill': 'Thời gian tuyển đủ vị trí', 'Outgoing Salary': 'Lương chi trả'})

def po_map():
    out={}
    for fn in PO_FILES:
        lines=(ROOT/fn).read_text(encoding='utf-8').splitlines(); i=0
        while i<len(lines):
            if not lines[i].startswith('msgid '): i+=1; continue
            mid=ast.literal_eval(lines[i][6:]); i+=1
            while i<len(lines) and lines[i].startswith('"'): mid+=ast.literal_eval(lines[i]); i+=1
            if i>=len(lines) or not lines[i].startswith('msgstr '): continue
            ms=ast.literal_eval(lines[i][7:]); i+=1
            while i<len(lines) and lines[i].startswith('"'): ms+=ast.literal_eval(lines[i]); i+=1
            if mid and ms: out[mid]=ms
    return out

PO=po_map()
def tr(s): return MANUAL.get(s,PO.get(s,s))

def translate_header(raw):
    plain=html.unescape(re.sub(r'<[^>]+>','',raw)).strip()
    new=tr(plain)
    return raw.replace(plain,new) if plain!=new and plain in raw else raw.replace(plain.replace('&','&amp;'),new)

def translate_doc(d):
    dt=d.get('doctype')
    if dt=='Dashboard Chart' and d.get('chart_name'): d['chart_name']=tr(d['chart_name'])
    elif dt=='Number Card' and d.get('label'): d['label']=tr(d['label'])
    elif dt=='Dashboard' and d.get('dashboard_name'): d['dashboard_name']=tr(d['dashboard_name'])
    elif dt=='Workspace':
        if d.get('label'): d['label']=tr(d['label'])
        for k in ('charts','links','shortcuts','quick_lists'):
            for x in d.get(k) or []:
                if x.get('label'): x['label']=tr(x['label'])
        try: blocks=json.loads(d.get('content') or '[]')
        except Exception: blocks=[]
        changed=False
        for b in blocks:
            if b.get('type')=='header' and (b.get('data') or {}).get('text'):
                old=b['data']['text']; new=translate_header(old)
                if new!=old: b['data']['text']=new; changed=True
        if changed: d['content']=json.dumps(blocks,ensure_ascii=False,separators=(',',':'))
    return d

def main():
    own=json.loads((WORK/'ownership.json').read_text())
    exports={
        'Dashboard Chart':json.loads((WORK/'dashboard_chart.json').read_text()),
        'Number Card':json.loads((WORK/'number_card.json').read_text()),
        'Dashboard':json.loads((WORK/'dashboard.json').read_text()),
        'Workspace':json.loads((WORK/'workspace.json').read_text()),
    }
    bydt={dt:{d['name']:d for d in docs} for dt,docs in exports.items()}
    OUT.mkdir(exist_ok=True)
    counts={}
    for app in ('frappe','erpnext','hrms','crm','insights'):
        docs=[]
        for dt,names in own[app].items():
            for name in names:
                src=bydt.get(dt,{}).get(name)
                if src is None: continue
                docs.append(translate_doc(json.loads(json.dumps(src))))
        path=OUT/f'{app}_vi_ui_v3.json'
        path.write_text(json.dumps(docs,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        counts[app]=len(docs)
    print(json.dumps(counts,ensure_ascii=False))

if __name__=='__main__': main()
