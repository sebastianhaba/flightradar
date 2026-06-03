import { dotnet } from './_framework/dotnet.js'

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create({
        environmentVariables: {
            HUB_URL: `${globalThis.location.origin}/hubs/radar`
        }
    });

await dotnetRuntime.runMain();
