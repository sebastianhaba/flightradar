import { dotnet } from './_framework/dotnet.js'

const url = new URL(globalThis.location.href);
url.searchParams.set('hub', `${globalThis.location.origin}/hubs/radar`);
globalThis.history.replaceState(null, '', url);

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

await dotnetRuntime.runMain();
