Release process
===============

Use the provided PowerShell script to bump version, publish a single-file self-contained release for win-x64, and store it under `Releases\<version>`.

Examples:

- Bump patch and publish:

  ```powershell
  .\scripts\release.ps1
  ```

- Bump minor and publish:

  ```powershell
  .\scripts\release.ps1 -Part minor
  ```

- Publish a specific version:

  ```powershell
  .\scripts\release.ps1 -Version 1.2.3.0
  ```

Notes:
- Requires .NET SDK and PowerShell.
- The script updates `version.txt` in the repo root. Ensure the repo is clean or commit changes as needed.
- The build uses `/p:PublishSingleFile=true` and `--self-contained true` for win-x64.
