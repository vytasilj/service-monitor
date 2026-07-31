# Service Monitor

A Windows tray application for monitoring Kubernetes deployments and HTTP health-check endpoints, built to replace manually checking Lens/kubectl/Grafana throughout the day. Runs quietly in the system tray, changes color when something needs attention, and keeps a local history of status changes — no cloud dependency, everything stays on your machine.

![CI](https://github.com/vytasilj/service-monitor/actions/workflows/ci.yml/badge.svg)

## Features

- System tray icon that reflects overall health at a glance (green / amber / red / gray), with balloon notifications on state changes
- Kubernetes deployment monitoring via the official .NET client, using your existing kubeconfig
- Per-namespace configuration: watch every deployment in a namespace (e.g. production) or only specific ones by name (e.g. a subset of staging)
- HTTP health-check monitoring for arbitrary endpoints, independent of Kubernetes
- Local, persistent history of status *transitions* (not every individual check) stored in SQLite — a clean timeline of what changed and when
- Sortable and filterable status views (by name and by state)
- Fully local: configuration and history live in `%AppData%\ServiceMonitor`, nothing is sent anywhere except the Kubernetes API and the HTTP endpoints you configure

## Tech stack

- .NET 10 / WPF
- CommunityToolkit.Mvvm (MVVM with source-generated observable properties and commands)
- Microsoft.Extensions.Hosting (dependency injection + `BackgroundService` for the polling loop)
- Official Kubernetes .NET client (`KubernetesClient`)
- Entity Framework Core + SQLite (local status history)
- xUnit (unit tests for health evaluation, filtering, and history transition logic)
- GitHub Actions CI (Windows runner, with test result reporting via `dorny/test-reporter`)

## Getting started

**Prerequisites:** .NET 10 SDK, Windows, a valid kubeconfig (if you want to monitor a cluster)

```bash
git clone https://github.com/vytasilj/service-monitor.git
cd service-monitor/ServiceMonitor.App
dotnet run
```

On first launch, a sample configuration file is created automatically at:
%AppData%\ServiceMonitor\config.json

Edit it to point at your own namespaces, deployments, and HTTP endpoints, then restart the app. Example:

```json
{
  "KubeConfigPath": "C:\\Users\\you\\.kube\\config",
  "PollIntervalSeconds": 30,
  "Namespaces": [
    { "Namespace": "production", "WatchAllDeployments": true, "SpecificDeployments": [] },
    { "Namespace": "staging", "WatchAllDeployments": false, "SpecificDeployments": ["api"] }
  ],
  "HttpEndpoints": [
    { "Name": "Public website", "Url": "https://example.com/health", "TimeoutSeconds": 5 }
  ]
}
```

## Running tests

```bash
dotnet test
```

## Roadmap

- Installer with automatic updates (planned: Velopack, distributed via GitHub Releases)