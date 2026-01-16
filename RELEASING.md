# Releasing AmpSharp

This document describes the automated release process for AmpSharp using GitHub Actions and Nerdbank.GitVersioning (NBGV).

## Prerequisites

### For Maintainers
1. **NBGV CLI**: Install globally for local version management
   ```bash
   dotnet tool install -g nbgv
   ```

2. **GitHub Secrets**: The repository must have the following secrets configured:
   - `NUGET_API_KEY`: API key for pushing to NuGet.org
   - `GITHUB_TOKEN`: Automatically provided by GitHub Actions (no setup needed)

## Automated Workflows

### CI Workflow (`ci.yml`)
**Trigger**: Push or Pull Request to `main` branch

**Actions**:
- Restores dependencies
- Builds solution in Release configuration
- Runs all tests

**Purpose**: Ensures code quality and that tests pass before merging.

### Release Workflow (`release.yml`)
**Trigger**: Push of a version tag (e.g., `v0.1.0`, `v1.0.0-alpha.1`)

**Actions**:
1. Validates tag format
2. Builds solution in Release configuration
3. Packs `NexusLabs.AmpSharp` NuGet package
4. Pushes to NuGet.org
5. Pushes to GitHub Packages
6. Creates GitHub Release with package artifacts

## Version Management with NBGV

The project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) for automatic version calculation.

### Version Configuration (`version.json`)
```json
{
  "version": "0.1",
  "publicReleaseRefSpec": [
    "^refs/heads/main$",
    "^refs/tags/v\\d+\\.\\d+(\\.\\d+)?(-.*)?$"
  ]
}
```

**How it works**:
- **Base version**: `0.1` (major.minor)
- **Patch version**: Auto-incremented based on git height (commits since last tag)
- **Pre-release**: Automatically added for non-public releases
- **Public releases**: Triggered by version tags on main branch

## Creating a Release

### Option 1: Using the Release Script (Recommended)

#### Stable Release
```powershell
# Release version 1.0.0
.\scripts\release.ps1 1.0.0

# Or let NBGV calculate the next version
.\scripts\release.ps1 1.0.0
```

#### Pre-release (Alpha, Beta, RC)
```powershell
# Create alpha pre-release (auto-increments: 0.1.0-alpha.1, 0.1.0-alpha.2, etc.)
.\scripts\release.ps1 -Prerelease alpha

# Create beta pre-release with specific base version
.\scripts\release.ps1 -Prerelease beta -Base 1.0.0

# Create release candidate
.\scripts\release.ps1 -Prerelease rc -Base 1.0.0
```

#### Dry Run
```powershell
# See what would happen without making changes
.\scripts\release.ps1 1.0.0 --DryRun
.\scripts\release.ps1 -Prerelease alpha --DryRun
```

**What the script does**:
1. Validates working tree is clean
2. Sets version in `version.json` using NBGV
3. Commits version change
4. Creates and pushes git tag (e.g., `v1.0.0`)
5. GitHub Actions automatically picks up the tag and releases

### Option 2: Manual Tag Creation

If you prefer to tag manually:

```bash
# Ensure working tree is clean
git status

# Set version using NBGV
nbgv set-version 1.0.0

# Commit the version change
git commit -am "chore: bump version to 1.0.0"

# Create and push tag
nbgv tag
git push origin HEAD --tags
```

## Version Numbering

AmpSharp follows [Semantic Versioning 2.0.0](https://semver.org/):

- **MAJOR** version: Incompatible API changes
- **MINOR** version: New functionality (backwards-compatible)
- **PATCH** version: Bug fixes (backwards-compatible)

### Pre-release Labels
- `-alpha.N`: Early development, unstable
- `-beta.N`: Feature-complete, but may have bugs
- `-rc.N`: Release candidate, ready for production testing

### Examples
- `0.1.0`: Initial development release
- `1.0.0-alpha.1`: First alpha of v1.0.0
- `1.0.0-beta.1`: First beta of v1.0.0
- `1.0.0-rc.1`: First release candidate of v1.0.0
- `1.0.0`: Stable v1.0.0 release
- `1.1.0`: Minor update with new features
- `1.1.1`: Patch release with bug fixes

## Package Publishing

### NuGet.org
Packages are automatically published to [NuGet.org](https://www.nuget.org/packages/NexusLabs.AmpSharp) when a version tag is pushed.

**Installation**:
```bash
dotnet add package NexusLabs.AmpSharp
```

### GitHub Packages
Packages are also published to GitHub Packages for development/testing.

**Installation** (requires GitHub authentication):
```bash
dotnet add package NexusLabs.AmpSharp --source https://nuget.pkg.github.com/nexus-labs/index.json
```

## Troubleshooting

### Release Failed
Check the GitHub Actions logs:
1. Go to **Actions** tab in GitHub
2. Click on the failed workflow run
3. Review logs for each step

Common issues:
- **NUGET_API_KEY not set**: Add the secret in repository settings
- **Tag format invalid**: Use `vX.Y.Z` format (e.g., `v1.0.0`)
- **Version conflict**: Version already exists on NuGet.org (use `--skip-duplicate` or increment version)

### Clean Failed Release
If a release failed partway through:

```bash
# Delete local tag
git tag -d v1.0.0

# Delete remote tag (if pushed)
git push origin :refs/tags/v1.0.0

# Reset version.json if needed
git checkout version.json

# Try again
.\scripts\release.ps1 1.0.0
```

### Testing Releases Locally
Build and pack locally without publishing:

```bash
# Build
dotnet build ampsharp.slnx --configuration Release

# Pack (creates .nupkg in artifacts/packages)
mkdir -p artifacts/packages
dotnet pack src/NexusLabs.AmpSharp/NexusLabs.AmpSharp.csproj `
  --configuration Release `
  --output artifacts/packages

# Test installation locally
dotnet add package NexusLabs.AmpSharp --source ./artifacts/packages
```

## Release Checklist

Before releasing a new version:

- [ ] All tests pass locally (`dotnet test ampsharp.slnx`)
- [ ] README.md is up to date
- [ ] CHANGELOG.md has entry for this version (if maintaining one)
- [ ] Breaking changes are documented
- [ ] PR is merged to `main`
- [ ] Working tree is clean
- [ ] Run release script or create tag
- [ ] Verify GitHub Actions workflow completes successfully
- [ ] Verify package appears on NuGet.org
- [ ] Test installation: `dotnet add package NexusLabs.AmpSharp`

## Release Schedule

AmpSharp follows a flexible release schedule:

- **Alpha releases**: As needed for testing new features
- **Beta releases**: When feature-complete for a version
- **RC releases**: 1-2 weeks before stable release
- **Stable releases**: When ready and tested
- **Patch releases**: As needed for critical bugs

## Support

For questions about releases:
- Open an issue: [GitHub Issues](https://github.com/nexus-labs/ampsharp/issues)
- Contact: Nexus Software Labs
