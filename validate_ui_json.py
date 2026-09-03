#!/usr/bin/env python3
import json, sys
from pathlib import Path

ROOT=Path(__file__).resolve().parent/'ui_json'
EXPECTED={'frappe':20,'erpnext':107,'hrms':80,'crm':1,'insights':0}
ALLOWED={'Dashboard Chart','Number Card','Dashboard','Workspace'}
HRMS_EXPECTED={
 'Claims by Type':'Đề nghị thanh toán theo loại',
 'Employee Advance Status':'Trạng thái tạm ứng nhân viên',
 'Department wise Expense Claims':'Đề nghị thanh toán theo phòng ban',
 'Department Wise Salary(Last Month)':'Lương tháng trước theo phòng ban',
 'Designation Wise Salary(Last Month)':'Lương tháng trước theo chức vụ',
}

def main():
    errors=[]
    for app,count in EXPECTED.items():
        p=ROOT/f'{app}_vi_ui_v3.json'
        if not p.exists(): errors.append(f'missing {p.name}'); continue
        try: docs=json.loads(p.read_text(encoding='utf-8'))
        except Exception as e: errors.append(f'{p.name}: invalid JSON: {e}'); continue
        if not isinstance(docs,list): errors.append(f'{p.name}: root must be a JSON list'); continue
        if len(docs)!=count: errors.append(f'{p.name}: expected {count} docs, got {len(docs)}')
        seen=set()
        for d in docs:
            if not isinstance(d,dict): errors.append(f'{p.name}: non-object document'); continue
            dt,name=d.get('doctype'),d.get('name')
            if dt not in ALLOWED: errors.append(f'{p.name}:{name}: unsupported doctype {dt!r}')
            if not name: errors.append(f'{p.name}: document missing stable name'); continue
            key=(dt,name)
            if key in seen: errors.append(f'{p.name}: duplicate {key}')
            seen.add(key)
            field={'Dashboard Chart':'chart_name','Number Card':'label','Dashboard':'dashboard_name','Workspace':'label'}.get(dt)
            if field and not d.get(field): errors.append(f'{p.name}:{name}: missing display field {field}')
        if app=='hrms':
            index={(d.get('doctype'),d.get('name')):d for d in docs if isinstance(d,dict)}
            for name,vi in HRMS_EXPECTED.items():
                got=index.get(('Dashboard Chart',name),{}).get('chart_name')
                if got!=vi: errors.append(f'{p.name}:{name}: expected {vi!r}, got {got!r}')
        print(f'{p.name}: {len(docs)} docs')
    if errors:
        print('\nFailures:')
        for e in errors: print('-',e)
        return 1
    print('PASS: UI JSON bundles are structurally valid')
    return 0
if __name__=='__main__': raise SystemExit(main())
