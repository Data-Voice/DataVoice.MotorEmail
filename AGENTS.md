# AGENTS.md

## What this repo is
Legacy .NET Framework 4.6.2 WinForms app ("Motor Email V2.0"). It polls POP3/IMAP mailboxes with EAGetMail (password or OAuth2), inserts messages into a MySQL DB through stored procedures, saves bodies/attachments to a file share, and reports failures to an external "ApiErrorNotifier" API. One repo is deployed per customer with different `app.config` values.

No tests, no CI, no lint/format config, no README. Verification = compile.

## Build
Old-style csproj + `packages.config`; use Visual Studio 2022 MSBuild (not `dotnet build`):

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" DataVoice.MotorEmail.sln /p:Configuration=Debug /p:Platform="Mixed Platforms"
```

- Main app is **x86** (`Debug|x86`, `Release|x86`); libraries are AnyCPU.
- NuGet packages are gitignored (`**/packages/*`) and resolved from `..\packages\...`. There are now `packages.config` files in Main, Entidad and Negocio (EAGetMail 5.2.5.9, MySql.Data 9.5.0, MSAL, plus transitive System.*/BouncyCastle/Protobuf/K4os). Restore/open in VS or builds fail on missing DLLs.
- `app.config` files (Main plus Entidad/Negocio) carry assembly binding redirects (System.Memory, Unsafe, Pipelines, etc.); keep them in sync when package versions change.

## Layout / dependency direction
- `DataVoice.MotorEmail` — WinForms entrypoint (`Program.cs` → `MainForm.cs`). `Program.Main` installs global exception handlers that POST to `http://localhost:29139/api/ApiErrorNotifier`; `MainForm.SendErrorToAPI` (~line 1148) posts to `https://appt.datavoice.com.mx/APIErrorMotores/api/ApiErrorNotifier`. Failures reaching the API are swallowed.
- `MainForm.ObtenerCorreos` (~line 626) is the polling core. Lines ~119–442 keep the commented-out legacy ActiveUp/POP3 implementation; don't "clean" it up.
- `DataVoice.MotorEmail.Negocio` — static facade over data access; hardcodes user `"G18"` and vista `6`.
- `DataVoice.MotorEmail.Entidad` — ADO.NET → MySQL. Every DB call is a stored procedure: `SpConCuentasEmail`, `SpConEmail` (vistas 7/8/9), `SpConEmailUnico`, `SpInsEmail` (now takes `p_FechaUltimoRegistro`), `SpUpdEmailToken`. No schema/migrations in this repo: the DB must already match the parameters the code sends.
- `DataVoice.MotorEmail.Models` — POCOs (`Email`, `CuentaEmail`, `CuentasEmail`, `FechaUltimoRegistro`).
- `DataVoice.MotorEmail.Setup` — legacy Visual Studio Installer project (.vdproj). Not built by the solution build; requires VS + Installer Projects extension. It packages the Main app's `obj\x86\Debug` output, so build Debug x86 before building the installer.

## Runtime gotchas
- Config comes only from `app.config` via `ConfigurationManager.AppSettings`; copied to `bin\...\DataVoice.MotorEmail.exe.config` at build, so edit + rebuild.
- Customer switching = uncomment the customer's `appSettings` block (`DVEmpresas`, `UbicacionArchivos`, `DireccionArchivos`, `NameFile`, plus OAuth keys `scopeOffice365`/`urlTokenOffice`/`TiempoToken`/`DiasMaximo`/`LicenseCodeEAGetMail`) and comment the previous one. All tenants stay as commented blocks. Active block is `ECOBICI`.
- Mail window: IMAP selects INBOX and filters server-side with `GetMailInfosParam.DateRange.SINCE = FechaUltimoRegistro - 1s` (fallback: last 5 days if null/MinValue). POP3 has no date search: it downloads every message and filters client-side. `DiasMaximo` is still a per-message guard after download.
- `FechaUltimoRegistro` is read from the DB (`SpConCuentasEmail`) and sent on insert (`SpInsEmail`), but the app never updates it (`fechaMasReciente` is assigned nowhere); `UltimoIndice`/`indiceInterno` drives the stored index.
- OAuth2 (Office 365) is client-credentials: token posted to `urlTokenOffice`, cached in DB via `SpUpdEmailToken`, refreshed when older than `TiempoToken` minutes. Non-OAuth uses password auth with SSL.
- Accounts appear as **unchecked** tree nodes at startup; the auto-poll timer only processes checked nodes. `TiempoTimerEmail` (seconds) controls the poll interval once a run finishes.
- `saveLOG`/`saveLOGAsync` write to `C:\Temporales\LogEmail\` and do **not** create the directory.
- Attachments/bodies are saved under `UbicacionArchivos` + base64(`Idmail`) + `\`; `DireccionArchivos` is the URL prefix stored in the DB.
- `ObtenerCorreos` still does EAGetMail/disk work on the UI thread (only DB calls are truly async), and `BtnCancelar` is dead code (`BackgroundWorker` never runs).

## Style
- Domain names and code are Spanish; match existing naming. No comments unless already present.
- `app.config` contains hardcoded legacy DB credentials and endpoints. Never add new secrets.
