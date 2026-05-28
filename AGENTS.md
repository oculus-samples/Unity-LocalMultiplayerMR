# Agent Instructions — Unity Local Multiplayer MR

Three independent Unity projects demonstrating colocated mixed-reality multiplayer (Shared Spatial Anchors + matchmaking + player connection) against three networking stacks: Netcode for GameObjects, Photon Fusion 1, and Photon Unity Networking 2.

## Source-of-truth files (read these first, do not duplicate their contents in this file)

For setup, build steps, SDK versions, and project layout, read:

- `README.md` — repo overview and per-project links
- `colocation-sample-ngo/README.md`, `colocation-sample-fusion/README.md`, `colocation-sample-pun2/README.md` — per-project setup (including Photon AppId)
- `colocation-sample-*/ProjectSettings/ProjectVersion.txt` — Unity editor version (per project)
- `colocation-sample-*/Packages/manifest.json` — Unity package versions (per project)
- `colocation-sample-*/Assets/Plugins/Android/AndroidManifest.xml` — Quest manifest when present
- `LICENSE.txt` — license terms (MIT for repo; TMP and Photon under their own licenses)

## Quest / Horizon-specific notes

- Multi-project repo — open each `colocation-sample-*` folder as a separate Unity project, not the repo root.
- `colocation-package/` is a shared package consumed by the three samples; when adding a new networking backend, implement `INetworkData` + `INetworkMessenger` against it rather than forking.
- The README itself notes Meta XR Core SDK v65+ Multiplayer Building Blocks is the simpler modern path for NGO / Photon Fusion 2; treat this repo as reference for the underlying SSA colocation flow.
- Photon-based samples fail at connect time if no Photon AppId is configured — check the per-sample README before debugging networking errors.

# Meta Quest tooling

This is a Meta Quest / Horizon OS sample. The bespoke intro above is the source of truth for what this project is and how it's built — use it (and the files it points at) instead of restating facts from memory.

When the user asks anything about Quest device behavior, build / deploy / debug / capture flows, on-device performance, or Horizon OS APIs, reach for these tools instead of generic Unity answers:

- **`hzdb`** — Quest-aware ADB wrapper (device list, install / launch / stop, logs, screenshots, Perfetto traces, on-device docs search). Already wired up as an MCP server via `.mcp.json`, `.vscode/mcp.json`, and `.cursor/mcp.json`. Also runnable directly: `npx -y @meta-quest/hzdb <subcommand>`.
- **Meta Quest Agentic Tools** — the full skill set, including Unity-specific skills: <https://github.com/meta-quest/agentic-tools>. Install per your client (Claude Code: `/plugin install meta-vr@meta-quest`; Gemini CLI: `gemini extensions install https://github.com/meta-quest/agentic-tools`; Cursor / VS Code: install the **Meta Horizon** extension from the Marketplace).

A few behavior expectations:

- **Read this repo's files first.** Before answering anything project-specific, read `README.md` and whichever source-of-truth files the intro above points at. Don't restate their contents in chat — quote or link instead.
- **Use `hzdb` for device-side work.** Anything that touches an attached Quest (install, launch, logs, screenshot, capture, manifest inspection) goes through `hzdb`, not raw `adb`.
- **Check live Horizon OS docs before answering API questions.** `hzdb docs search "..."` queries the live docs; training data on Horizon OS APIs goes stale fast.
- **Don't fabricate SDK / engine versions.** If a version isn't visible in this repo's files, say so rather than guessing.
