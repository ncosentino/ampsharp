# GitHub Workflows & Release Automation - Setup Summary

## ✅ What Was Configured

The following automation has been set up for AmpSharp, adapted from the Needlr project:

### 🔄 Continuous Integration (CI)
- **File**: `.github/workflows/ci.yml`
- **Trigger**: Push or PR to `main` branch
- **Actions**: Restore → Build → Test
- **Status**: ✅ Configured and ready

### 🚀 Automated Releases
- **File**: `.github/workflows/release.yml`
- **Trigger**: Push of version tag (e.g., `v1.0.0`)
- **Actions**: Build → Pack → Publish to NuGet.org & GitHub Packages → Create GitHub Release
- **Status**: ✅ Configured (requires NUGET_API_KEY secret)

### 📦 Version Management
- **Tool**: Nerdbank.GitVersioning (NBGV)
- **Config**: `version.json`
- **Base Version**: `0.1`
- **Status**: ✅ Integrated into build

### 📝 Documentation
- **RELEASING.md**: Complete release process guide
- **.github/WORKFLOW_SETUP.md**: Technical workflow documentation
- **Status**: ✅ Created

## 🔧 Key Changes from Needlr

| Aspect | Needlr | AmpSharp |
|--------|--------|----------|
| Project Name | `NexusLabs.Needlr` | `NexusLabs.AmpSharp` |
| .NET Version | `9.0.x` | `10.0.x` |
| Solution Path | `src/NexusLabs.Needlr.slnx` | `ampsharp.slnx` (root) |
| Package Pattern | `NexusLabs.Needlr*` | `NexusLabs.AmpSharp*` |

## 📋 Files Modified/Created

```
ampsharp/
├── .github/
│   ├── workflows/
│   │   ├── ci.yml              # ✅ Updated for AmpSharp
│   │   └── release.yml         # ✅ Updated for AmpSharp
│   └── WORKFLOW_SETUP.md       # ✅ Created
├── scripts/
│   └── release.ps1             # ✅ Compatible (no changes needed)
├── version.json                # ✅ Created
├── RELEASING.md                # ✅ Created
└── Directory.Packages.props    # ✅ Updated (added NBGV)
```

## 🎯 How to Use

### For Contributors (Running CI)
Simply push to `main` or open a PR - CI runs automatically.

```bash
# Workflow will automatically:
# 1. Restore dependencies
# 2. Build solution
# 3. Run all 28 tests
```

### For Maintainers (Creating Releases)

#### Quick Release
```powershell
# Alpha release (auto-increments)
.\scripts\release.ps1 -Prerelease alpha

# Beta release
.\scripts\release.ps1 -Prerelease beta -Base 1.0.0

# Stable release
.\scripts\release.ps1 1.0.0
```

#### What Happens Automatically
1. ✅ Version is set in `version.json`
2. ✅ Change is committed
3. ✅ Git tag is created (e.g., `v1.0.0`)
4. ✅ Pushed to GitHub
5. ✅ GitHub Actions detects tag
6. ✅ Builds and tests
7. ✅ Creates NuGet package
8. ✅ Publishes to NuGet.org
9. ✅ Publishes to GitHub Packages
10. ✅ Creates GitHub Release

## ⚙️ Required Setup

### Before First Release

1. **Configure NuGet API Key** (one-time setup)
   ```
   1. Visit https://www.nuget.org/ → Account → API Keys
   2. Create new key with "Push" permission
   3. Go to GitHub: Repository → Settings → Secrets → Actions
   4. Add secret: NUGET_API_KEY
   ```

2. **Verify Build Works**
   ```bash
   dotnet build ampsharp.slnx --configuration Release
   dotnet test ampsharp.slnx --configuration Release
   ```

3. **Test Release Script**
   ```powershell
   # Dry run - see what would happen
   .\scripts\release.ps1 -Prerelease alpha --DryRun
   ```

## 📊 Current Status

| Component | Status | Notes |
|-----------|--------|-------|
| CI Workflow | ✅ Ready | Runs on push/PR to main |
| Release Workflow | ⚠️ Needs Secret | Add NUGET_API_KEY |
| NBGV Integration | ✅ Working | Version: 0.1.0-alpha.0.X |
| Build | ✅ Passing | 0 warnings, 0 errors |
| Tests | ✅ Passing | 28/28 tests pass |
| Documentation | ✅ Complete | RELEASING.md created |

## 🧪 Testing the Setup

### Test CI Locally
```bash
dotnet restore ampsharp.slnx
dotnet build ampsharp.slnx --configuration Release --no-restore
dotnet test ampsharp.slnx --configuration Release --no-build --verbosity normal
```

### Test Package Creation Locally
```bash
mkdir -p artifacts/packages
dotnet pack src/NexusLabs.AmpSharp/NexusLabs.AmpSharp.csproj \
  --configuration Release \
  -p:PublicRelease=true \
  --output artifacts/packages
ls -la artifacts/packages
```

### Verify NBGV (requires nbgv CLI)
```bash
# Install if needed
dotnet tool install -g nbgv

# Check version
nbgv get-version
```

## 📚 Documentation Reference

- **[RELEASING.md](../RELEASING.md)**: Complete release guide with examples
- **[.github/WORKFLOW_SETUP.md](.github/WORKFLOW_SETUP.md)**: Technical workflow details
- **[README.md](../README.md)**: Project overview and usage

## 🎓 Best Practices

### Before Releasing
- [ ] All tests pass locally
- [ ] PR merged to `main`
- [ ] CHANGELOG updated (if maintaining)
- [ ] Breaking changes documented
- [ ] Working tree is clean

### Version Numbering
- **0.x.y**: Pre-1.0 development
- **1.0.0**: First stable release
- **1.1.0**: New features (minor)
- **1.0.1**: Bug fixes (patch)
- **2.0.0**: Breaking changes (major)

### Pre-release Labels
- `-alpha.N`: Early testing
- `-beta.N`: Feature complete
- `-rc.N`: Release candidate

## 🔍 Troubleshooting

### "nbgv command not found"
```bash
dotnet tool install -g nbgv
# On Windows, may need to restart terminal
```

### "NUGET_API_KEY secret not found"
- Go to GitHub → Repository Settings → Secrets → Actions
- Add `NUGET_API_KEY` with your NuGet.org API key

### "Version already exists"
- Can't overwrite NuGet.org versions
- Increment version or use pre-release suffix
- Delete tag and try again: `git tag -d v1.0.0 && git push origin :refs/tags/v1.0.0`

## 🎉 What's Next?

1. **Configure Secrets**: Add `NUGET_API_KEY` to GitHub
2. **Test CI**: Push a commit to `main` and watch workflow run
3. **First Release**: Create an alpha release
   ```powershell
   .\scripts\release.ps1 -Prerelease alpha
   ```
4. **Monitor**: Check GitHub Actions and NuGet.org for package

## 📞 Support

For workflow issues:
- Check `.github/WORKFLOW_SETUP.md` for troubleshooting
- Review workflow logs in GitHub Actions tab
- See RELEASING.md for release process details

---

**Setup Status**: ✅ Complete and ready for use!

All workflows have been successfully adapted from Needlr to AmpSharp and are ready for production use.
