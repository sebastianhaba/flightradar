import { dotnet } from './_framework/dotnet.js'

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .create();

const splash = document.getElementById('flightradar-splash');
if (splash) splash.classList.add('splash-close');

await dotnetRuntime.runMain();
