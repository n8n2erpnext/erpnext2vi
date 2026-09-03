#!/usr/bin/env python3
import argparse
import json
import os
import sys
from pathlib import Path

APP_FILES = {
    "frappe": "frappe_vi_ui_v3.json",
    "erpnext": "erpnext_vi_ui_v3.json",
    "hrms": "hrms_vi_ui_v3.json",
    "crm": "crm_vi_ui_v3.json",
    "insights": "insights_vi_ui_v3.json",
}

TOP_FIELDS = {
    "Dashboard Chart": ("chart_name",),
    "Number Card": ("label",),
    "Dashboard": ("dashboard_name",),
    "Workspace": ("label", "content"),
}

WORKSPACE_CHILD_TABLES = ("charts", "links", "shortcuts", "quick_lists")
IGNORE_CHILD_KEYS = {
    "label", "name", "doctype", "parent", "parenttype", "parentfield",
    "idx", "creation", "modified", "owner", "modified_by", "docstatus",
}


def parse_args():
    p = argparse.ArgumentParser(description="Apply Vietnamese UI JSON overrides safely")
    p.add_argument("--bench-path", required=True)
    p.add_argument("--site", required=True)
    p.add_argument("--bundle-dir", required=True)
    p.add_argument("--apps", default="frappe,erpnext,hrms,crm,insights")
    p.add_argument("--doctypes", default="Dashboard Chart,Number Card,Dashboard")
    p.add_argument("--dry-run", action="store_true")
    return p.parse_args()


def load_docs(bundle_dir: Path, apps):
    docs = []
    for app in apps:
        fn = APP_FILES.get(app)
        if not fn:
            raise SystemExit(f"unknown app: {app}")
        path = bundle_dir / fn
        if not path.exists():
            raise SystemExit(f"missing bundle: {path}")
        data = json.loads(path.read_text(encoding="utf-8"))
        if not isinstance(data, list):
            raise SystemExit(f"bundle must be a JSON list: {path}")
        docs.extend((app, d) for d in data)
    return docs


def child_matches(current, desired):
    current_dict = current.as_dict()
    for key, value in desired.items():
        if key in IGNORE_CHILD_KEYS:
            continue
        if current_dict.get(key) != value:
            return False
    return True


def resolve_child(current_rows, desired, index):
    if index < len(current_rows) and child_matches(current_rows[index], desired):
        return current_rows[index]
    matches = [row for row in current_rows if child_matches(row, desired)]
    if len(matches) == 1:
        return matches[0]
    raise RuntimeError(
        f"cannot safely match workspace child row at index {index}: "
        f"{json.dumps(desired, ensure_ascii=False)}"
    )


def update_field(frappe, doctype, name, field, old, new, dry_run):
    if old == new:
        return 0
    print(f"{doctype}::{name} {field}: {old!r} -> {new!r}")
    if not dry_run:
        frappe.db.set_value(doctype, name, field, new, update_modified=False)
    return 1


def apply_workspace_children(frappe, current, desired, dry_run):
    changes = 0
    for table in WORKSPACE_CHILD_TABLES:
        wanted = desired.get(table) or []
        existing = list(getattr(current, table, []) or [])
        if len(wanted) != len(existing):
            raise RuntimeError(
                f"Workspace::{current.name} {table} length mismatch: "
                f"bundle={len(wanted)} site={len(existing)}"
            )
        for idx, wanted_row in enumerate(wanted):
            if "label" not in wanted_row:
                continue
            row = resolve_child(existing, wanted_row, idx)
            changes += update_field(
                frappe, row.doctype, row.name, "label",
                row.get("label"), wanted_row.get("label"), dry_run,
            )
    return changes


def apply_doc(frappe, app, desired, dry_run):
    doctype = desired.get("doctype")
    name = desired.get("name")
    if doctype not in TOP_FIELDS or not name:
        raise RuntimeError(f"unsupported or unnamed document in {app}: {doctype}::{name}")
    if not frappe.db.exists(doctype, name):
        raise RuntimeError(f"missing site document for {app}: {doctype}::{name}")
    current = frappe.get_doc(doctype, name)
    changes = 0
    for field in TOP_FIELDS[doctype]:
        if field in desired:
            changes += update_field(
                frappe, doctype, name, field,
                current.get(field), desired.get(field), dry_run,
            )
    if doctype == "Workspace":
        changes += apply_workspace_children(frappe, current, desired, dry_run)
    return changes


def main():
    args = parse_args()
    bench = Path(args.bench_path).resolve()
    sites = bench / "sites"
    bundle_dir = Path(args.bundle_dir).resolve()
    apps = [x.strip() for x in args.apps.split(",") if x.strip()]
    docs = load_docs(bundle_dir, apps)
    allowed_doctypes = {x.strip() for x in args.doctypes.split(",") if x.strip()}
    docs = [(app, d) for app, d in docs if d.get("doctype") in allowed_doctypes]

    sys.path.insert(0, str(bench / "apps" / "frappe"))
    os.chdir(sites)
    import frappe

    frappe.init(site=args.site, sites_path=str(sites))
    frappe.connect()
    try:
        changes = 0
        for app, desired in docs:
            changes += apply_doc(frappe, app, desired, args.dry_run)
        if args.dry_run:
            frappe.db.rollback()
        else:
            frappe.db.commit()
            frappe.clear_cache()
        print(f"DONE docs={len(docs)} changes={changes} dry_run={args.dry_run}")
    except Exception:
        frappe.db.rollback()
        raise
    finally:
        frappe.destroy()


if __name__ == "__main__":
    main()
