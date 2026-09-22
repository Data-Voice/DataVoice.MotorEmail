# AGENTS.md

## What this repo is
Legacy .NET Framework 4.6.2 WinForms app ("Motor Email V2.0"). It polls POP3/IMAP mailboxes with EAGetMail (password or OAuth2), inserts messages into a MySQL DB through stored procedures, and saves bodies/attachments to a file share. One repo is deployed per customer with different `app.config` values.

There are no tests, no CI, no lint/format config, and no README. Verification = compile.

## Build
Old-style csproj + `packages.config`; use Visual Studio 2022 MSBuild (not `dotnet build`):

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" DataVoice.MotorEmail.sln /p:Configuration=Debug /p:Platform="Mixed Platforms"
```

- Main app is **x86** (`Debug|x86`, `Release|x86`); libraries are AnyCPU.
- NuGet packages (EAGetMail, Microsoft.Identity.Client, Microsoft.IdentityModel.Abstractions) are gitignored (`**/packages/*`). Restore/open in VS to recreate `packages/`; these are the Main project's only package refs.
- **Broken reference:** `DataVoice.MotorEmail.Entidad.csproj` points MySql.Data at `..\..\..\DataVoice\Desarrollo\Complementos\...`, which does not exist. Clean builds fail with `CS0246 ... 'MySql'`. The working copy only builds because `bin\Debug\MySql.Data.dll` and/or a machine-local copy is found. A valid 8.0.28 copy exists at `D:\DataVoice\Desarrollo\Complementos\EnsambladosVersionados\Ensamblados\MySql_v8.0.28\MySql.Data.dll`; fix the HintPath if a clean build fails, and flag it rather than silently working around it.

## Layout / dependency direction
- `DataVoice.MotorEmail` — WinForms entrypoint (`Program.cs` → `MainForm.cs`). All mail retrieval lives in `MainForm.ObtenerCorreos` (~line 611). Lines ~119–442 are a large commented-out legacy ActiveUp/POP3 implementation; the active code below it replaces that path.
- `DataVoice.MotorEmail.Negocio` — static facade over data access; hardcodes user `"G18"` and vista `6`.
- `DataVoice.MotorEmail.Entidad` — ADO.NET → MySQL. Every DB call is a stored procedure: `SpConCuentasEmail`, `SpConEmail` (vistas 7/8/9), `SpConEmailUnico`, `SpInsEmail`, `SpUpdEmailToken`. No schema/migrations in this repo.
- `DataVoice.MotorEmail.Models` — POCOs (`Email`, `CuentaEmail`, `CuentasEmail`).
- `DataVoice.MotorEmail.Setup` — legacy Visual Studio Installer project (.vdproj). Not built by the solution build; requires VS + Installer Projects extension. It packages the Main app's `obj\x86\Release` output, so build Release x86 before building the installer.

## Runtime gotchas
- Config comes only from `app.config` via `ConfigurationManager.AppSettings`; it is copied to `bin\...\DataVoice.MotorEmail.exe.config` at build, so edit + rebuild.
- Customer switching = uncomment the customer's `appSettings` block (`DVEmpresas`, `UbicacionArchivos`, `DireccionArchivos`, `NameFile`, plus OAuth keys `scopeOffice365`/`urlTokenOffice`/`TiempoToken`/`DiasMaximo`/`LicenseCodeEAGetMail`) and comment the previous one. All tenants are kept in the file as commented blocks. Current active block is renamed `ECOBICI`.
- OAuth2 (Office 365) is client-credentials: token is posted to `urlTokenOffice` and cached in DB via `SpUpdEmailToken`; refreshed when older than `TiempoToken` minutes. Non-OAuth accounts use password auth with SSL.
- Accounts appear as **unchecked** tree nodes at startup; the auto-poll timer only processes checked nodes. `TiempoTimerEmail` (seconds) controls the poll interval once a run finishes.
- `UltimoIndice` (per account, from DB) determines the first message index to process; `DiasMaximo` filters out older mail.
- `saveLOG`/`saveLOGAsync` write to `C:\Temporales\LogEmail\` and do **not** create the directory.
- Attachments/bodies are saved under `UbicacionArchivos` + base64(`Idmail`) + `\`; `DireccionArchivos` is the URL prefix stored in the DB.

## Style
- Domain names and code are Spanish; match existing naming. No comments unless already present.
- `app.config` contains hardcoded legacy DB credentials and endpoints. Never add new secrets; these values are already tracked.
