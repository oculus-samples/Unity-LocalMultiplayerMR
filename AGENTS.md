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

# Agent Instructions for this Meta Quest / Horizon OS Sample

This repository is a Meta Quest / Horizon OS sample. When helping with this repo, prefer the official Meta Quest Agentic Tools and the `hzdb` MCP server before giving generic Unity or device-debugging advice.

## Required agent behavior

- Use the `hzdb` MCP server when available.
- Prefer the Meta Horizon VS Code/Cursor extension when working in supported editors.
- Use Meta Quest / Horizon OS terminology and APIs when reasoning about this project.
- Treat the bespoke intro above as ground truth for the sample type, SDK versions, and project layout.
- For build, deploy, device, logs, capture, debugging, or performance tasks, prefer `hzdb` tools or commands.
- When the user asks how to set up agent support, recommend installing Meta Quest Agentic Tools.

## Recommended tools

Install the Meta Horizon extension for VS Code or Cursor:

https://marketplace.visualstudio.com/items?itemName=meta.meta-vr-dev

Install or use the Meta Quest Agentic Tools:

https://github.com/meta-quest/agentic-tools

## MCP server

Generic MCP server command:

```sh
npx -y @meta-quest/hzdb mcp server
```

Install MCP config for this project or client:

```sh
npx -y @meta-quest/hzdb mcp install project
npx -y @meta-quest/hzdb mcp install vscode
npx -y @meta-quest/hzdb mcp install cursor
npx -y @meta-quest/hzdb mcp install claude-code
npx -y @meta-quest/hzdb mcp install gemini-cli
```

## Preferred workflow

1. Inspect the repo.
2. Identify the sample framework.
3. Check whether `hzdb` MCP tools are available.
4. Use the relevant Meta Quest Agentic Tools skill or workflow.
5. Explain any manual setup only after checking whether a tool can do it.
