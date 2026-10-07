# ShellConnect

SPAD.neXt creates `TShellStub`; Shell has no MEF attributes and no reference to
Bootstrapper or Dispatcher implementation DLLs. It discovers `IBootstrapper`
and `IEventDispatcher` when needed through `Connect.Provider.Data.TCompositionHost`.

All public Connect interfaces reside in Connect.Provider.Interface, including
`IMessageRecord`, `IMessageRecord<TData>`, `IEventDispatcher`, and `IBootstrapper`.
Interface references Enumerator only. Data references Interface and contains
message records plus the composition host, avoiding circular dependencies.
Typed dispatcher APIs now accept `IMessageRecord<TData>`.

The host discovers `Connect.*.dll` beside the Data assembly once and owns one
thread-safe MEF container for the process lifetime. Dispatcher and Bootstrapper
are shared exports. Bootstrapper receives its dispatcher through constructor
injection. It starts one message subscription for the first script owner and
unsubscribes when the last owner stops. Starting the same owner twice is harmless.
Shell retains no service fields. Bootstrapper retains the subscription token
needed for deterministic cleanup. The container disposes services at process exit.

Build order: Enumerator, Interface, Data, Dispatcher, Bootstrapper, Shell.
`ShellConnect.bat` follows that order. Runtime dependencies are copied to
`D:\SPAD.neXt\Addons` by the build targets. SPAD.Interfaces is supplied by the host.
For local verification without deployment:

```powershell
$buildOutput = Join-Path (Get-Location) 'Bin\'
dotnet build <project.csproj> "-p:OutputPath=$buildOutput" "-p:BaseOutputPath=$buildOutput" -p:SkipAddonDeployment=true
dotnet run --project Tests/CompositionSmoke/CompositionSmoke.csproj
```

The smoke test requires installed SPAD.Interfaces. It checks typed message
delivery, shared services, plain host-created Shells, multiple script owners,
stop/restart cleanup, missing/duplicate exports, and container disposal.
Restart SPAD.neXt after deployment; actual script activation and profile changes
must still be checked in that host.
