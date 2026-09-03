#!/usr/bin/env python3
"""Semantic/format QA for ERPNext/Frappe Vietnamese PO files.

No external dependencies.  The checks are intentionally source-aware: a Vietnamese
fragment is rejected only when the English msgid proves the ERP concept involved.
"""
from __future__ import annotations

import ast
import collections
import glob
import re
import sys
from pathlib import Path

PO_FILES = sorted(glob.glob("*_vi_v2.po"))

# Identical English keys should compose consistently across apps.
# Explicit exceptions are only for genuinely context-ambiguous upstream keys.
CROSS_APP_CONTEXT_COLLISIONS = {
    "{0} M": {
        "crm_vi_v2.po": "{0} Phút",
        "frappe_vi_v2.po": "{0} tháng",
    },
}

FORMAT_TOKEN = re.compile(r"(?<!\{)\{(?:\d+|[A-Za-z_][\w.]*|)\}(?!\})|%\([^)]+\)[#0 +\-]*\d*(?:\.\d+)?[diouxXeEfFgGcrs]|(?<!%)%[sdif]")
JINJA_TOKEN = re.compile(r"\{\{.*?\}\}|\{%.*?%\}", re.S)
JS_TEMPLATE = re.compile(r"\$\{.*?\}", re.S)
HTML_TAG = re.compile(r"</?([A-Za-z][A-Za-z0-9:-]*)\b")

SOURCE_RULES = [
    ("Holiday List", ["danh sách ngày lễ", "danh sách ngày nghỉ"], "Lịch nghỉ"),
    ("Pick List", ["danh sách chọn"], "Danh sách lấy hàng"),
    ("Price List", ["danh sách giá"], "Bảng giá"),
    ("Price List Rate", ["tỷ giá bảng giá", "tỷ giá danh sách giá"], "Đơn giá bảng giá"),
    ("Sales Order", ["đơn hàng bán", "đơn đặt hàng"], "Đơn bán hàng"),
    ("Purchase Order", ["đơn đặt hàng", "đơn hàng mua", "purchase order"], "Đơn mua hàng"),
    ("Purchase Receipt", ["biên nhận mua hàng", "biên lai mua hàng", "phiếu nhận hàng mua", "phiếu nhận mua"], "Phiếu nhập hàng"),
    ("Stock Entry", ["bút toán kho", "bút toán tồn kho", "mục kho", "mục hàng tồn kho", "mục nhập kho", "mục nhập tồn kho", "bảng nhập kho"], "Phiếu kho"),
    ("Stock Ledger", ["sổ cái tồn kho", "sổ tồn kho", "sổ cái chứng khoán"], "Sổ kho"),
    ("Supplier Quotation", ["báo giá từ nhà cung cấp"], "Báo giá nhà cung cấp"),
    ("Landed Cost", ["chi phí đã đáp tàu", "chi phí hạ cánh", "chi phí hạ tầng"], "Chi phí nhập hàng"),
    ("Pricing Rule", ["quy tắc định giá", "quy tắc giá", "pricing rule"], "Chính sách giá"),
    ("Expense Claim", ["claim chi phí", "yêu cầu hoàn chi"], "Đề nghị thanh toán"),
    ("Subcontract", ["ký gửi", "giao việc ngoài", "gia công phụ", "công nhân việc", "phụ thuộc vào"], "Gia công"),
    ("Sub-contract", ["ký gửi", "giao việc ngoài", "gia công phụ", "công nhân việc", "phụ thuộc vào"], "Gia công"),
    ("Valuation Rate", ["tỷ giá định giá", "tỷ lệ định giá"], "Đơn giá định giá"),
    ("Posting Date", ["ngày đăng"], "Ngày hạch toán"),
    ("Posting Datetime", ["ngày giờ đăng"], "Thời điểm hạch toán"),
    ("Posting Time", ["thời gian đăng", "giờ đăng"], "Giờ hạch toán"),
    ("Work Order", ["đơn hàng công việc", "đơn đặt hàng công việc", "work order"], "Lệnh sản xuất"),
    ("Payment Entry", ["mục thanh toán", "payment entry"], "Phiếu thanh toán"),
    ("Journal Entry", ["mục nhật ký", "nhật ký kế toán", "journal entry"], "Bút toán"),
]

FRAPPE_ALLOWED_UNCHANGED_EXACT = {
    "&lt;head&gt; HTML", "API", "Ar", "Arial", "B", "BCC", "Beta", "Bot", "C5E", "CC", "CMD", "CSS", "CSV", "Cache", "Comm10E", "Cr", "Cron", "DLE",
    "Dashboard", "DocField", "DocPerm", "DocShare", "Domain", "Dr", "ESC", "Email", "Excel", "Facebook", "Fax", "Filter Meta", "Frappe", "Frappe Mail", "GMail",
    "GNU Affero General Public License", "Gantt", "GitHub", "Google", "Google Calendar", "Google Drive", "HTML", "Helvetica", "Helvetica Neue", "InnoDB", "Instagram", "Import",
    "JS", "JSON", "JavaScript", "Javascript", "Jinja", "Kanban", "Kh", "L", "LinkedIn", "M", "Madam", "Meta", "Miss", "Mr", "Mrs", "Ms", "Mx", "MyISAM",
    "Nomatim", "OAuth", "OAuth Bearer Token", "OAuth Client", "OpenLDAP", "Outlook.com", "PDF", "PID", "Python", "Re:", "Robots.txt", "Role", "SQL", "Security.txt", "Skype",
    "SparkPost", "StartTLS", "Sync", "T", "Tab", "Theme", "Token", "UID", "UIDNEXT", "UIDVALIDITY", "URL", "UUID", "Verdana", "Webhook", "Webhook URL",
    "Websocket", "Workflow", "XLSX", "Yahoo Mail", "Yandex.Mail", "YouTube", "after_insert", "chrome", "d", "emacs", "email", "facebook", "h", "linkedin", "m",
    "nonce", "old_parent", "on_cancel", "on_change", "on_submit", "on_trash", "on_update", "on_update_after_submit", "s", "s256", "twitter", "vim", "vscode",
    "wkhtmltopdf", "workflow_transition",
}

FRAPPE_ALLOWED_UNCHANGED_PATTERNS = [
    re.compile(r"[AB]\d{1,2}"),
    re.compile(r"HH:mm(?::ss)?"),
    re.compile(r"(?:dd-mm-yyyy|mm-dd-yyyy|yyyy-mm-dd)"),
    re.compile(r"Fw: \{0\}|Re: \{0\}"),
    re.compile(r"[^@\s]+@[^@\s]+\.[^@\s]+"),
    re.compile(r"\{0\} \$\{.*\}"),
    re.compile(r"\{0\}: \{1\} vs \{2\}"),
]

def frappe_unchanged_is_allowed(text: str) -> bool:
    if text in FRAPPE_ALLOWED_UNCHANGED_EXACT:
        return True
    return any(pattern.fullmatch(text) for pattern in FRAPPE_ALLOWED_UNCHANGED_PATTERNS)


def unchanged_english_is_allowed(path: str, text: str) -> bool:
    name = Path(path).name
    if name == "frappe_vi_v2.po":
        return frappe_unchanged_is_allowed(text)
    return text in APP_ALLOWED_UNCHANGED_EXACT.get(name, set())

# Reviewed exact-English entries intentionally preserved per app.
# Any new untranslated English UI string must be explicitly reviewed before being added.
APP_ALLOWED_UNCHANGED_EXACT = {
    'crm_vi_v2.po': {
        '%John%', '<b>META</b>', 'BCC', 'CC', 'CSV',
        'Dashboard', 'ERPNext', 'Email', 'Excel', 'Exotel',
        'Facebook', 'Favicon', 'Frappe CRM', 'GMT+5:30', 'Import',
        'JSON', 'John Doe', 'John, Jane, Doe', 'Kanban', 'Role',
        'SLA', 'Theme', 'TwiML SID', 'Twilio', 'WhatsApp',
        'exchangerate-api', 'exchangerate.host', 'fawazahmed-exchange-api', 'frankfurter.app', 'john@doe.com',
        'kanban',
    },
    'erpnext_vi_v2.po': {
        '<div class="text-muted text-center">{0}</div>', '<li>{}</li>', 'A - B', 'A - C', 'A+',
        'A-', 'AB+', 'AB-', 'ACC-PINV-.YYYY.-', 'Abampere',
        'Ampere', 'Are', 'Arshin', 'B+', 'B-',
        'BFS', 'BOM 1', 'BOM 2', 'Barleycorn', 'Biot',
        'Btu (It)', 'Btu (Th)', 'Bushel (UK)', 'Bushel (US Dry Level)', 'CODE-39',
        'CRM', 'Caballeria', 'Calibre', 'Calorie (It)', 'Calorie (Th)',
        'Carat', 'Celsius', 'Cental', 'Centiarea', 'Coulomb',
        'Cr', 'D - E', 'DFS', 'Decigram/Litre', 'Decilitre',
        'Decimeter', 'Diesel', 'Dram', 'Dyne', 'EAN',
        'EAN-13', 'EAN-8', 'ERPNext', 'Email:', 'Ems(Pica)',
        'Erg', 'FIFO', 'Fahrenheit', 'Faraday', 'Fathom',
        'Fluid Ounce (UK)', 'Fluid Ounce (US)', 'Foot', 'Foot Of Water', 'Furlong',
        'G - D', 'GS1', 'GTIN', 'GTIN-14', 'Gallon (UK)',
        'Gallon Dry (US)', 'Gallon Liquid (US)', 'Gamma', 'Gauss', 'Grain',
        'Grain/Cubic Foot', 'Grain/Gallon (UK)', 'Grain/Gallon (US)', 'Gram', 'Gram-Force',
        'Gram/Cubic Centimeter', 'Gram/Cubic Meter', 'Gram/Cubic Millimeter', 'Gram/Litre', 'H - F',
        'Hand', 'Hectopascal', 'Hertz', 'I - J', 'I - K',
        'IBAN', 'IRS 1099', 'ISBN', 'ISBN-10', 'ISBN-13',
        'ISSN', 'Id', 'Inch', 'Inch Pound-Force', 'Inches Of Mercury',
        'Incoterm', 'JAN', 'Joule', 'Kelvin', 'Kg',
        'Kiloampere', 'Kilocalorie', 'Kilocoulomb', 'Kilogram-Force', 'Kilohertz',
        'Kilojoule', 'Kilopascal', 'Kilopond', 'Kilopound-Force', 'Kilowatt',
        'Kip', 'Knot', 'LIFO', 'MPS', 'Megacoulomb',
        'Megahertz', 'Megajoule', 'Megawatt', 'Microbar', 'Microgram',
        'Milibar', 'Milliampere', 'Millicoulomb', 'Milligram', 'Millihertz',
        'Nanocoulomb', 'Nanohertz', 'Newton', 'O+', 'O-',
        'PCV', 'PIN', 'POS', 'PZN', 'Pascal',
        'Pond', 'Pood', 'Pound', 'Pound-Force', 'Pound/Gallon (UK)',
        'Pound/Gallon (US)', 'Poundal', 'Psi/1000 Feet', 'Rgt', 'Rod',
        'Sazhen', 'Serial / Batch', 'Serial No', 'Serial No / Batch', 'Stone',
        'Tesla', 'Torr', 'UAE VAT 201', 'UPC', 'UPC-A',
        'Vara', 'Versta', 'Video', 'Vimeo', 'Volt-Ampere',
        'Watt', 'WhatsApp', 'Yard', 'exchangerate.host', 'frankfurter.dev',
        'lft', 'rgt',
    },
    'hrms_vi_v2.po': {
        '<hr>', "<table class='table table-bordered'><tr><th>{0}</th><th>{1}</th></tr>", 'Frappe HR', 'HRMS', 'IFSC',
        'Internet', 'KRA', 'KRAs', 'MICR', 'Taxi',
    },
    'insights_vi_v2.po': {
        '1:N', 'BigQuery', 'BigQuery Dataset ID', 'BigQuery Project ID', 'CSV',
        'ClickHouse', 'Cron', 'DuckDB', 'Email', 'Excel',
        'Frappe Insights', 'HTTP Headers', 'Import', 'JSON', 'MariaDB',
        'N:1', 'N:N', 'Notebook', 'Pivot', 'PostgreSQL',
        'REST API', 'SQL', 'SQLite', 'Schema', 'Sync',
        'Telegram', 'Unpivot', 'nodes', 'sort_order',
    },
}

# Runtime composition grammar for Frappe fragments that receive labels from other apps.
FRAPPE_COMPOSITION_EXPECTED = {
    "{0} Calendar": "Lịch {0}",
    "{0} Chart": "Biểu đồ {0}",
    "{0} Dashboard": "Dashboard {0}",
    "{0} Fields": "{0} trường",       # {0} is a count
    "{0} List": "Danh sách {0}",      # {0} is a DocType/object label
    "{0} List View Settings": "Thiết lập giao diện danh sách {0}",
    "{0} Map": "Bản đồ {0}",
    "{0} Name": "Tên {0}",
    "{0} Report": "Báo cáo {0}",
    "{0} Reports": "{0} báo cáo",     # {0} is a count
    "{0} Settings": "Cài đặt {0}",
    "{0} Tree": "Cây {0}",
    "New {0}": "{0} mới",
    "New {0} Created": "Đã tạo {0} mới",
    "Go to {0} List": "Đi tới danh sách {0}",
    "Create a new {0}": "Tạo {0}",
    "{0} record deleted": "Đã xóa {0} bản ghi",
    "{0} records deleted": "Đã xóa {0} bản ghi",
    "{0} records will be exported": "Sẽ Export {0} bản ghi",
    "{0} items selected": "Đã chọn {0} mục",
    "{0} values selected": "Đã chọn {0} giá trị",
}

# App/domain-specific semantic regressions that simple placeholder checks cannot catch.
APP_SEMANTIC_RULES = {
    "crm_vi_v2.po": [
        (r"(?<!\w)Deals?(?!\w)", [r"(?<!\w)deals?(?!\w)"], "Cơ hội bán hàng"),
        (r"(?<!\w)Leads?(?!\w)", [r"(?<!\w)leads?(?!\w)"], "Khách hàng tiềm năng"),
        (r"(?<!\w)Website(?!\w)", [r"(?<!\w)website(?!\w)"], "trang web"),
        (r"Form Script", [r"Form Script"], "tập lệnh biểu mẫu"),
        (r"Access Token", [r"Access Token", r"Access token"], "Token truy cập"),
    ],
    "erpnext_vi_v2.po": [
        (r"(?<!\w)Dunning(?!\w)", [r"(?<!\w)Dunning(?!\w)", r"đòi nợ"], "Nhắc nợ"),
        (r"(?<!\w)Lead(?!\w)", [r"(?<!\w)Lead(?!\w)"], "Khách hàng tiềm năng"),
    ],
    "hrms_vi_v2.po": [
        (r"(?<!\w)Designation(?!\w)", [r"chức danh"], "Chức vụ"),
        (r"(?<!\w)Attendance(?!\w)", [r"điểm danh"], "Chấm công"),
        (r"Employee Check-?in", [r"(?<!\w)check-?ins?(?!\w)"], "Ghi nhận chấm công nhân viên"),
        (r"(?<!\w)Payroll Entry(?!\w)", [r"mục nhập lương"], "Bảng lương"),
    ],
    "insights_vi_v2.po": [
        (r"(?<!\w)Workbooks?(?!\w)", [r"(?<!\w)workbooks?(?!\w)"], "Sổ làm việc"),
    ],
}

# Cross-cutting source concepts: these English terms are intentionally kept or translated
# consistently no matter which app supplies the string.
CROSS_SOURCE_RULES = [
    (r"(?<!\w)Subject(?!\w)", [r"(?<!\w)theme(?!\w)"], "Chủ đề"),
    (r"(?<!\w)Dashboards?(?!\w)", [r"trang tổng quan", r"bảng điều khiển"], "Dashboard"),
]

FRAPPE_SOURCE_RULES = [
    ("Web Form", ["web form", "web forms"], "Biểu mẫu web"),
    ("Onboarding", ["onboarding"], "giới thiệu"),
    ("ToDo", ["todo"], "Việc cần làm"),
    ("Blog Post", ["blog post"], "Bài viết"),
    ("Round Robin", ["round robin"], "Luân phiên"),
    ("Support Password", ["support"], "mật khẩu hỗ trợ"),
    ("Auth Server Metadata", ["metadata auth server", "auth server metadata"], "siêu dữ liệu Server xác thực"),
    ("Website", ["website"], "trang web"),
    ("Menu", [" menu", "menu "], "Trình đơn"),
    ("Sidebar", ["sidebar"], "Thanh bên"),
    ("Timeline", ["timeline"], "Lịch sử"),
    ("Child Table", ["child table"], "Bảng con"),
]

FRAPPE_FORBIDDEN = [
    "social login key",
    "authorise api access",
    "module defs",
    "pull emails",
    "pulling emails",
    "clipboard",
    "navbar",
    "server scripts",
    "chế độ developer",
]

FORBIDDEN_GLOBAL = [
    "đơn mua hàng hàng",
    "hoá đơn mua hàng hàng",
    "phiếu nhập hàng hàng",
    "báo giá nhà cung cấp cấp",
    "serialal",
    "tài liệu trẻ em",
    "mục gl",
    "chi phí landed",
    "phiếu nhập hàng mua",
]


def decode_po_literal(text: str) -> str:
    try:
        return ast.literal_eval(text[text.index('"'):])
    except Exception as exc:
        raise ValueError(f"invalid PO string: {text.rstrip()} ({exc})")


def validate_po_structure(path: str):
    raw = Path(path).read_bytes()
    errors = []
    if raw.startswith(b"\xef\xbb\xbf"):
        errors.append("UTF-8 BOM is not allowed; Babel PO compiler warns on it")
    try:
        text = raw.decode("utf-8")
    except UnicodeDecodeError as exc:
        return [f"invalid UTF-8: {exc}"]
    lines = text.splitlines()
    msgid_directives = sum(1 for line in lines if line.startswith("msgid "))
    msgstr_directives = sum(1 for line in lines if line.startswith("msgstr "))
    if msgid_directives != msgstr_directives:
        errors.append(f"unbalanced PO directives: msgid={msgid_directives}, msgstr={msgstr_directives}")
    first = next((i for i, line in enumerate(lines) if line.strip() and not line.startswith("#")), None)
    if first is None or lines[first] != 'msgid ""':
        errors.append('PO header must start with msgid ""')
    elif first + 1 >= len(lines) or lines[first + 1] != 'msgstr ""':
        errors.append('PO header msgid "" must be followed by msgstr ""')
    return errors


def parse_po(path: str):
    entries = []
    lines = Path(path).read_text(encoding="utf-8").splitlines()
    i = 0
    while i < len(lines):
        if not lines[i].startswith("msgid "):
            i += 1
            continue
        line_no = i + 1
        msgid = decode_po_literal(lines[i])
        i += 1
        while i < len(lines) and lines[i].startswith('"'):
            msgid += decode_po_literal(lines[i])
            i += 1
        if i >= len(lines) or not lines[i].startswith("msgstr "):
            raise ValueError(f"{path}:{line_no}: msgid without msgstr")
        msgstr = decode_po_literal(lines[i])
        i += 1
        while i < len(lines) and lines[i].startswith('"'):
            msgstr += decode_po_literal(lines[i])
            i += 1
        if msgid:
            entries.append((line_no, msgid, msgstr))
    return entries


def multiset(pattern, text):
    return collections.Counter(pattern.findall(text))


def validate_entry(path: str, line: int, msgid: str, msgstr: str):
    errors = []
    if not msgid or not msgid.strip():
        return errors
    if not msgstr:
        errors.append("empty translation")
        return errors

    if msgid == msgstr and re.search(r"[A-Za-z]", msgid):
        if not unchanged_english_is_allowed(path, msgid):
            errors.append(f"untranslated English entry is not in the approved {Path(path).name} allowlist")

    for label, pattern in (
        ("format placeholders", FORMAT_TOKEN),
        ("JS template tokens", JS_TEMPLATE),
    ):
        a, b = multiset(pattern, msgid), multiset(pattern, msgstr)
        if a != b:
            errors.append(f"{label} differ: EN={dict(a)} VI={dict(b)}")

    # Jinja expressions may contain translatable string literals (e.g. default labels).
    # Preserve the number/type of Jinja blocks without requiring literal text equality.
    en_jinja = [token[:2] for token in JINJA_TOKEN.findall(msgid)]
    vi_jinja = [token[:2] for token in JINJA_TOKEN.findall(msgstr)]
    if collections.Counter(en_jinja) != collections.Counter(vi_jinja):
        errors.append("Jinja block structure differs")

    # Tag names may repeat; attributes/text may be translated, tag structure must remain.
    en_tags = collections.Counter(x.lower() for x in HTML_TAG.findall(msgid))
    vi_tags = collections.Counter(x.lower() for x in HTML_TAG.findall(msgstr))
    if en_tags != vi_tags:
        errors.append(f"HTML tags differ: EN={dict(en_tags)} VI={dict(vi_tags)}")

    low_id, low_vi = msgid.lower(), msgstr.lower()

    app_name = Path(path).name
    if app_name == "frappe_vi_v2.po" and msgid in FRAPPE_COMPOSITION_EXPECTED:
        expected = FRAPPE_COMPOSITION_EXPECTED[msgid]
        if msgstr != expected:
            errors.append(f"Frappe composition drift: expected {expected!r}, got {msgstr!r}")

    for source_rx, bad_regexes, expected in CROSS_SOURCE_RULES:
        if re.search(source_rx, msgid, re.I):
            for bad_rx in bad_regexes:
                if re.search(bad_rx, msgstr, re.I):
                    errors.append(f"cross-source semantic drift (expected concept: {expected})")

    for source_rx, bad_regexes, expected in APP_SEMANTIC_RULES.get(app_name, []):
        if re.search(source_rx, msgid, re.I):
            for bad_rx in bad_regexes:
                if re.search(bad_rx, msgstr, re.I):
                    errors.append(f"{app_name} semantic drift (expected concept: {expected})")

    for source_term, bad_forms, expected in SOURCE_RULES:
        source_pattern = r"(?<!\w)" + re.escape(source_term.lower()) + r"(?!\w)"
        if re.search(source_pattern, low_id):
            for bad in bad_forms:
                if bad in low_vi:
                    errors.append(f"'{source_term}' still uses '{bad}' (expected concept: {expected})")

    if "tỷ giá" in low_vi and "rate" in low_id:
        is_fx = (
            "exchange" in low_id
            or "conversion rate" in low_id
            or ("currenc" in low_id and "convert" in low_id)
        )
        if not is_fx:
            errors.append("non-currency Rate is translated as 'tỷ giá'")

    if Path(path).name == "frappe_vi_v2.po":
        for source_term, bad_forms, expected in FRAPPE_SOURCE_RULES:
            if source_term.lower() in low_id:
                for bad in bad_forms:
                    if bad in low_vi:
                        errors.append(f"Frappe UI '{source_term}' still uses '{bad}' (expected concept: {expected})")
        for bad in FRAPPE_FORBIDDEN:
            if bad in low_vi:
                errors.append(f"Frappe UI mixed-language residue: '{bad}'")

    for bad in FORBIDDEN_GLOBAL:
        if bad in low_vi:
            errors.append(f"duplicate/malformed phrase: '{bad}'")
    return errors


def main() -> int:
    if not PO_FILES:
        print("No *_vi_v2.po files found", file=sys.stderr)
        return 2
    total = 0
    failures = []
    parsed = {path: parse_po(path) for path in PO_FILES}
    for path in PO_FILES:
        structure_errors = validate_po_structure(path)
        if structure_errors:
            failures.append((path, 1, "<PO header>", structure_errors))
        entries = parsed[path]
        total += len(entries)
        count = len(structure_errors)
        for line, msgid, msgstr in entries:
            errs = validate_entry(path, line, msgid, msgstr)
            if errs:
                count += len(errs)
                failures.append((path, line, msgid, errs))
        print(f"{path}: {len(entries)} entries, {count} semantic/format errors")

    # Cross-app composition gate: identical msgids should not change wording by app.
    shared = collections.defaultdict(list)
    for path, entries in parsed.items():
        for line, msgid, msgstr in entries:
            if msgid:
                shared[msgid].append((Path(path).name, line, msgstr))
    cross_errors = 0
    for msgid, rows in shared.items():
        apps = {app for app, _, _ in rows}
        if len(apps) < 2:
            continue
        if msgid in CROSS_APP_CONTEXT_COLLISIONS:
            expected = CROSS_APP_CONTEXT_COLLISIONS[msgid]
            for app, line, msgstr in rows:
                if app not in expected:
                    failures.append((app, line, msgid, ["ambiguous cross-app msgid used by an unreviewed app"]))
                    cross_errors += 1
                elif msgstr != expected[app]:
                    failures.append((app, line, msgid, [f"context collision drift: expected {expected[app]!r}, got {msgstr!r}"]))
                    cross_errors += 1
            continue
        variants = collections.defaultdict(list)
        for app, line, msgstr in rows:
            variants[msgstr].append((app, line))
        if len(variants) > 1:
            detail = "; ".join(f"{text!r} @ {locs}" for text, locs in variants.items())
            app, line, _ = rows[0]
            failures.append((app, line, msgid, [f"cross-app translation conflict: {detail}"]))
            cross_errors += 1
    print(f"Cross-app composition: {len(shared)} unique msgids checked, {cross_errors} conflicts")

    print(f"Checked {total} entries across {len(PO_FILES)} files")
    if failures:
        print("\nFailures:")
        for path, line, msgid, errs in failures[:200]:
            print(f"- {path}:{line}: {msgid[:180]}")
            for err in errs:
                print(f"    {err}")
        if len(failures) > 200:
            print(f"... {len(failures) - 200} more entries omitted")
        return 1
    print("PASS: semantic glossary and format invariants are clean")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
