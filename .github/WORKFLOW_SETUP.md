# GitHub Workflows Setup for AmpSharp

This document describes the GitHub Actions workflows configured for AmpSharp.

## Overview

AmpSharp uses GitHub Actions for continuous integration (CI) and automated releases. The workflows have been adapted from the Needlr project and customized for AmpSharp.

## Files Modified/Created

### Workflows
- `.github/workflows/ci.yml` - Continuous Integration
- `.github/workflows/release.yml` - Automated Release Publishing

### Version Management
- `version.json` - Nerdbank.GitVersioning configuration
- `scripts/release.ps1` - PowerShell release automation script

### Documentation
- `RELEASING.md` - Complete release process documentation
- `.github/WORKFLOW_SETUP.md` - This file

### Project Configuration
- `Directory.Packages.props` - Added `Nerdbank.GitVersioning` package
- `src/NexusLabs.AmpSharp/NexusLabs.AmpSharp.csproj` - Added NBGV reference, removed hardcoded version

## Key Changes from Needlr

### Project Naming
- ✅ `NexusLabs.Needlr` → `NexusLabs.AmpSharp`
- ✅ `src/NexusLabs.Needlr.slnx` → `ampsharp.slnx`

### .NET Version
- ✅ `.NET 9.0.x` → `.NET 10.0.x`

### Solution Structure
- ✅ Solution file is now in root directory instead of `src/` subdirectory
- ✅ Updated all restore/build/test commands to use `ampsharp.slnx`

### Package Output
- ✅ Packs `NexusLabs.AmpSharp*.csproj` projects
- ✅ Publishes to NuGet.org and GitHub Packages
- ✅ Creates GitHub Releases with package artifacts

## Workflows

### CI Workflow (ci.yml)

**Trigger**: Push or Pull Request to `main` branch

**Steps**:
1. Checkout code with full history
2. Setup .NET 10.0.x
3. Restore dependencies
4. Build in Release configuration
5. Run tests

**Purpose**: Ensures code quality and that all tests pass before merging.

**Badge**: Add to README.md:
```markdown
[![CI](https://github.com/nexus-labs/ampsharp/actions/workflows/ci.yml/badge.svg)](https://github.com/nexus-labs/ampsharp/actions/workflows/ci.yml)
```

### Release Workflow (release.yml)

**Trigger**: Push of version tag (e.g., `v0.1.0`, `v1.0.0-alpha.1`)

**Steps**:
1. Checkout with full history and tags
2. Setup .NET 10.0.x
3. Install NBGV CLI
4. Validate tag format and prepare version
5. Restore dependencies
6. Build in Release with PublicRelease flag
7. Pack NuGet packages to `artifacts/packages/`
8. Push to NuGet.org (requires `NUGET_API_KEY` secret)
9. Push to GitHub Packages (uses automatic `GITHUB_TOKEN`)
10. Create GitHub Release with package files

**Required Secrets**:
- `NUGET_API_KEY`: API key for NuGet.org (must be configured in repository settings)

## Version Management

AmpSharp uses [Nerdbank.GitVersioning (NBGV)](https://github.com/dotnet/Nerdbank.GitVersioning) for automatic semantic versioning.

### Configuration (version.json)

```json
{
  "version": "0.1",
  "publicReleaseRefSpec": [
    "^refs/heads/main$",
    "^refs/tags/v\\d+\\.\\d+(\\.\\d+)?(-.*)?$"
  ]
}
```

### How It Works

- **Base Version**: `0.1` (major.minor)
- **Patch**: Auto-calculated from git height (commits since last version tag)
- **Pre-release**: Automatically added for non-release builds
- **Public Release**: Triggered by pushing version tags matching `v*` pattern

### Version Examples

| Tag | Resulting Version | Notes |
|-----|-------------------|-------|
| `v0.1.0` | `0.1.0` | Stable release |
| `v1.0.0-alpha.1` | `1.0.0-alpha.1` | Alpha pre-release |
| `v1.0.0-beta.1` | `1.0.0-beta.1` | Beta pre-release |
| `v1.0.0-rc.1` | `1.0.0-rc.1` | Release candidate |
| (no tag) | `0.1.0-alpha.0.N+<hash>` | Development build |

## Release Process

### Quick Start

```powershell
# Stable release
.\scripts\release.ps1 1.0.0

# Pre-release
.\scripts\release.ps1 -Prerelease alpha
.\scripts\release.ps1 -Prerelease beta -Base 1.0.0
.\scripts\release.ps1 -Prerelease rc -Base 1.0.0

# Dry run (test without changes)
.\scripts\release.ps1 1.0.0 --DryRun
```

### What Happens

1. **Local** (via script):
   - Updates `version.json`
   - Commits the change
   - Creates git tag (e.g., `v1.0.0`)
   - Pushes to GitHub with tags

2. **GitHub Actions** (automatic):
   - Detects new tag
   - Builds and tests
   - Packs NuGet packages
   - Publishes to NuGet.org
   - Publishes to GitHub Packages
   - Creates GitHub Release

## Repository Setup Checklist

- [ ] **NuGet.org API Key**
  1. Go to [NuGet.org](https://www.nuget.org/) → Account → API Keys
  2. Create new API key with "Push new packages and package versions" permission
  3. Add to GitHub repository: Settings → Secrets → Actions → New repository secret
  4. Name: `NUGET_API_KEY`

- [ ] **GitHub Packages** (automatic)
  - No setup needed; uses built-in `GITHUB_TOKEN`

- [ ] **Branch Protection** (recommended)
  - Protect `main` branch
  - Require PR reviews
  - Require status checks (CI workflow) to pass

- [ ] **Add Badges to README.md**
  ```markdown
  [![CI](https://github.com/nexus-labs/ampsharp/actions/workflows/ci.yml/badge.svg)](https://github.com/nexus-labs/ampsharp/actions/workflows/ci.yml)
  [![NuGet](https://img.shields.io/nuget/v/NexusLabs.AmpSharp.svg)](https://www.nuget.org/packages/NexusLabs.AmpSharp)
  [![License](https://img.shields.io/github/license/nexus-labs/ampsharp)](LICENSE)
  ```

## Testing Workflows Locally

### Simulate CI Build

```bash
# Restore
dotnet restore ampsharp.slnx

# Build
dotnet build ampsharp.slnx --configuration Release --no-restore

# Test
dotnet test ampsharp.slnx --configuration Release --no-build --verbosity normal
```

### Simulate Release Build

```bash
# Build with PublicRelease flag
dotnet build ampsharp.slnx --configuration Release -p:PublicRelease=true

# Pack packages
mkdir -p artifacts/packages
dotnet pack src/NexusLabs.AmpSharp/NexusLabs.AmpSharp.csproj \
  --configuration Release \
  --no-build \
  -p:PublicRelease=true \
  --output artifacts/packages

# List packages
ls -la artifacts/packages
```

## Troubleshooting

### Workflow Not Triggering

**Problem**: Pushed tag but release workflow didn't run

**Solutions**:
- Check tag format matches `v*` pattern (e.g., `v1.0.0`, not `1.0.0`)
- Verify workflows are enabled: Repository Settings → Actions → General
- Check workflow runs: Actions tab → Filter by workflow

### NuGet Push Fails

**Problem**: "401 Unauthorized" or "API key invalid"

**Solutions**:
- Verify `NUGET_API_KEY` secret exists and is valid
- Check API key permissions on NuGet.org
- API keys expire - regenerate if needed

### Version Conflict

**Problem**: "Package version already exists"

**Solutions**:
- Use a new version number (can't overwrite existing versions on NuGet.org)
- For testing, use pre-release versions (e.g., `-alpha.1`, `-alpha.2`)
- Delete failed tag and try again with incremented version

### Build Fails in GitHub Actions

**Problem**: Build works locally but fails in CI

**Solutions**:
- Check .NET version matches (`10.0.x`)
- Verify solution path is correct (`ampsharp.slnx` in root)
- Check for missing files or incorrect paths
- Review workflow logs in Actions tab

## Maintenance

### Updating Workflows

When modifying workflows:
1. Edit `.github/workflows/*.yml` files
2. Test changes with dry-run or in a branch
3. Merge to `main` after verification

### Updating NBGV Configuration

To change version scheme:
1. Edit `version.json`
2. Commit changes
3. NBGV will use new configuration immediately

### Updating Dependencies

Version management dependencies:
```bash
# Update NBGV
dotnet tool update -g nbgv

# Update in Directory.Packages.props
# Change: <PackageVersion Include="Nerdbank.GitVersioning" Version="X.Y.Z" />
```

## Reference Links

- [Nerdbank.GitVersioning Docs](https://github.com/dotnet/Nerdbank.GitVersioning/blob/master/doc/index.md)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [NuGet Publishing Guide](https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [Semantic Versioning](https://semver.org/)

## Summary

The workflow setup provides:
- ✅ Automated testing on every push/PR
- ✅ Automated releases via git tags
- ✅ Automatic versioning with NBGV
- ✅ Multi-platform package publishing (NuGet.org + GitHub)
- ✅ GitHub Releases with artifacts
- ✅ Simple release process via PowerShell script

All workflows are production-ready and follow industry best practices for .NET package publishing.
