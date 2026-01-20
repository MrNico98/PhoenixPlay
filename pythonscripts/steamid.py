import requests
import json
import re
import base64
import os
from concurrent.futures import ThreadPoolExecutor, as_completed

# ----------------------------
# CONFIG
# ----------------------------

GITHUB_TOKEN = os.getenv("GITHUB_TOKEN")
OWNER = "MrNico98"
REPO = "PhoenixPlay"
BRANCH = "main"
BASE_PATH = "IDapp"

SOURCES = {
    "SteamRip": {
        "url": "https://hydralinks.pages.dev/sources/steamrip.json",
        "output": "steamIDSteamRip.json"
    },
    "FitGirl": {
        "url": "https://hydralinks.pages.dev/sources/fitgirl.json",
        "output": "steamIDFitGirl.json"
    },
    "OnlineFix": {
        "url": "https://hydralinks.pages.dev/sources/onlinefix.json",
        "output": "steamIDOnlineFix.json"
    }
}

# ----------------------------
# UTIL
# ----------------------------

def fetch_json(url, session):
    r = session.get(url, timeout=30)
    r.raise_for_status()
    return r.json()

def clean_title(title):
    title = re.sub(r"Free Download.*", "", title, flags=re.IGNORECASE)
    title = re.sub(r"\(v.*?\)", "", title, flags=re.IGNORECASE)
    title = re.sub(r"\[.*?\]", "", title)
    return title.strip()

# ----------------------------
# STEAM SEARCH
# ----------------------------

def find_steam_appid(session, game_title):
    query = clean_title(game_title)
    url = f"https://store.steampowered.com/search/results/?term={requests.utils.quote(query)}&category1=998"

    try:
        html = session.get(url, timeout=20).text
        match = re.search(r"/app/(\d+)/", html)
        if match:
            return match.group(1)
    except:
        pass

    return None

# ----------------------------
# GITHUB API
# ----------------------------

def github_headers():
    return {
        "Authorization": f"token {GITHUB_TOKEN}",
        "Accept": "application/vnd.github+json"
    }

def get_github_file(path):
    url = f"https://api.github.com/repos/{OWNER}/{REPO}/contents/{path}"
    r = requests.get(url, headers=github_headers())

    if r.status_code == 200:
        data = r.json()
        content = base64.b64decode(data["content"]).decode("utf-8")
        return json.loads(content), data["sha"]

    return None, None

def upload_to_github(path, new_data, message):
    url = f"https://api.github.com/repos/{OWNER}/{REPO}/contents/{path}"

    old_data, sha = get_github_file(path)

    if old_data:
        existing_titles = {g["title"].strip().lower() for g in old_data}
        existing_appids = {str(g["steam_appid"]) for g in old_data if g["steam_appid"]}

        filtered = [
            g for g in new_data
            if g["title"].strip().lower() not in existing_titles
            and (not g["steam_appid"] or str(g["steam_appid"]) not in existing_appids)
        ]

        if not filtered:
            print("ℹ️ Nessun nuovo gioco da aggiungere.")
            return

        final_data = old_data + filtered
        print(f"➕ Aggiunti {len(filtered)} nuovi giochi.")

    else:
        final_data = new_data
        print("📄 File non esistente → creato nuovo.")

    final_data = sorted(final_data, key=lambda x: x["title"].lower())

    encoded = base64.b64encode(
        json.dumps(final_data, ensure_ascii=False, indent=2).encode("utf-8")
    ).decode("utf-8")

    payload = {
        "message": message,
        "content": encoded,
        "branch": BRANCH
    }

    if sha:
        payload["sha"] = sha

    res = requests.put(url, headers=github_headers(), json=payload)
    res.raise_for_status()

# ----------------------------
# MAIN PROCESS
# ----------------------------

def process_source(name, url, output_file):
    print(f"\n==============================")
    print(f"📦 Fonte: {name}")
    print(f"==============================")

    session = requests.Session()
    session.headers.update({"User-Agent": "Mozilla/5.0"})

    data = fetch_json(url, session)
    games = [item["title"] for item in data.get("downloads", [])]

    results = []

    with ThreadPoolExecutor(max_workers=8) as executor:
        futures = {
            executor.submit(find_steam_appid, session, title): title
            for title in games
        }

        for i, future in enumerate(as_completed(futures), 1):
            title = futures[future]
            appid = future.result()
            print(f"🔎 [{i}/{len(games)}] {title} → {appid}")

            results.append({
                "title": title,
                "steam_appid": appid
            })

    github_path = f"{BASE_PATH}/{output_file}"
    upload_to_github(github_path, results, f"Update {output_file}")

    print(f"\n✅ Sync completato: {github_path}")

def main():
    if not GITHUB_TOKEN or "METTI_QUI" in GITHUB_TOKEN:
        print("❌ Devi impostare un GITHUB_TOKEN valido.")
        return

    for name, info in SOURCES.items():
        process_source(name, info["url"], info["output"])

    print("\n🎉 Tutte le fonti sono state sincronizzate.")

if __name__ == "__main__":
    main()
