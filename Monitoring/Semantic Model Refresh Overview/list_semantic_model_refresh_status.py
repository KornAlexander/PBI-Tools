# Tenant-wide semantic model refresh + schedule overview
# Run inside a Fabric notebook (Python). Requires: semantic-link, semantic-link-labs.
#
# Output columns:
#   workspace_name, workspace_id, dataset_name, dataset_id, configured_by,
#   schedule_enabled, schedule_paused, schedule_days, schedule_times, schedule_timezone,
#   last_refresh_status, last_refresh_end, last_refresh_type, last_refresh_error
#
# Notes:
# - "schedule_paused" = schedule exists (days/times configured) but enabled=False.
# - Requires Fabric Admin OR membership/permission on each workspace you want to inspect.
# - For tenant-wide coverage as a non-admin, restrict to workspaces you can access.

# %pip install semantic-link-labs --quiet

import sempy.fabric as fabric
import sempy_labs as labs
import pandas as pd
from sempy.fabric.exceptions import FabricHTTPException

client = fabric.FabricRestClient()
rows = []

workspaces = fabric.list_workspaces()
# Optional: skip personal "My workspace" rows
workspaces = workspaces[workspaces["Type"] != "PersonalGroup"]

print(f"Scanning {len(workspaces)} workspaces...")

for _, ws in workspaces.iterrows():
    ws_id, ws_name = ws["Id"], ws["Name"]
    try:
        datasets = fabric.list_datasets(workspace=ws_id)
    except Exception as e:
        print(f"[skip workspace] {ws_name}: {e}")
        continue

    # Column names vary across sempy versions and casing: match case-insensitively
    cols_lower = {c.lower(): c for c in datasets.columns}
    id_col = next((cols_lower[k] for k in ("dataset id", "id", "datasetid") if k in cols_lower), None)
    name_col = next((cols_lower[k] for k in ("dataset name", "name", "datasetname") if k in cols_lower), None)
    cfg_col = next((cols_lower[k] for k in ("configured by", "configuredby") if k in cols_lower), None)
    if id_col is None or name_col is None:
        if len(datasets) == 0:
            continue  # no datasets in this workspace
        print(f"[skip workspace] {ws_name}: unexpected dataset columns {list(datasets.columns)}")
        continue

    for _, ds in datasets.iterrows():
        ds_id, ds_name = ds[id_col], ds[name_col]
        configured_by = ds[cfg_col] if cfg_col else ""

        # --- refresh schedule (Power BI REST) ---
        sched_enabled = sched_days = sched_times = sched_tz = None
        schedule_paused = None
        try:
            r = client.get(f"/v1.0/myorg/groups/{ws_id}/datasets/{ds_id}/refreshSchedule")
            if r.status_code == 200:
                s = r.json()
                sched_enabled = s.get("enabled")
                sched_days = ",".join(s.get("days", []) or [])
                sched_times = ",".join(s.get("times", []) or [])
                sched_tz = s.get("localTimeZoneId")
                has_schedule = bool(sched_days or sched_times)
                schedule_paused = has_schedule and (sched_enabled is False)
        except FabricHTTPException:
            pass
        except Exception:
            pass

        # --- last refresh ---
        last_status = last_end = last_type = last_error = None
        try:
            hist = labs.list_semantic_model_refreshes(
                dataset=ds_id, workspace=ws_id
            )
            if hist is not None and len(hist) > 0:
                latest = hist.sort_values("End Time", ascending=False).iloc[0]
                last_status = latest.get("Status")
                last_end = latest.get("End Time")
                last_type = latest.get("Refresh Type")
                # error column name varies; pick first that exists
                for col in ("Service Exception Json", "Messages", "Error"):
                    if col in latest and pd.notna(latest.get(col)):
                        last_error = str(latest.get(col))[:500]
                        break
        except Exception as e:
            last_error = f"history-error: {e}"

        rows.append({
            "workspace_name": ws_name,
            "workspace_id": ws_id,
            "dataset_name": ds_name,
            "dataset_id": ds_id,
            "configured_by": configured_by,
            "schedule_enabled": sched_enabled,
            "schedule_paused": schedule_paused,
            "schedule_days": sched_days,
            "schedule_times": sched_times,
            "schedule_timezone": sched_tz,
            "last_refresh_status": last_status,
            "last_refresh_end": last_end,
            "last_refresh_type": last_type,
            "last_refresh_error": last_error,
        })

expected_cols = [
    "workspace_name", "workspace_id", "dataset_name", "dataset_id", "configured_by",
    "schedule_enabled", "schedule_paused", "schedule_days", "schedule_times", "schedule_timezone",
    "last_refresh_status", "last_refresh_end", "last_refresh_type", "last_refresh_error",
]
df = pd.DataFrame(rows, columns=expected_cols)

if df.empty:
    print("\nNo datasets collected. Check workspace permissions above.")
else:
    print("\n=== Failed last refresh ===")
    print(df[df["last_refresh_status"] == "Failed"]
          [["workspace_name", "dataset_name", "last_refresh_end", "last_refresh_error"]]
          .to_string(index=False))

    print("\n=== Paused schedules ===")
    print(df[df["schedule_paused"] == True]
          [["workspace_name", "dataset_name", "schedule_days", "schedule_times"]]
          .to_string(index=False))

    print("\n=== Counts ===")
    print(df["last_refresh_status"].value_counts(dropna=False))

# Display full table (in a notebook)
try:
    display(df)  # noqa: F821
except NameError:
    print(df.to_string(index=False))

# Optional: persist to lakehouse
# spark.createDataFrame(df).write.mode("overwrite").saveAsTable("semantic_model_refresh_overview")
