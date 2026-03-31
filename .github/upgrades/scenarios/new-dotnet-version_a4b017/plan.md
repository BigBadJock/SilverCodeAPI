# .NET 10 Upgrade Plan for SilverCodeAPI

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Plans](#project-by-project-plans)
  - [Core.Common.DataModels](#corecommondatamodels)
  - [Core.Common.Contracts](#corecommoncontracts)
  - [Core.Common](#corecommon)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Description
This plan details the upgrade of the SilverCodeAPI solution from .NET 9 to .NET 10 (Long Term Support). The solution consists of 3 class library projects providing common infrastructure for data access patterns, including repository pattern and unit of work implementations.

### Scope

**Projects to Upgrade**: 3 projects
- Core.Common.DataModels
- Core.Common.Contracts  
- Core.Common

**Current State**: All projects currently target .NET 9.0

**Target State**: All projects will target .NET 10.0

### Discovered Metrics

| Metric | Value |
| :--- | :--- |
| Total Projects | 3 |
| Total Lines of Code | 1,376 |
| Total Code Files | 50 |
| Dependency Depth | 2 levels |
| NuGet Packages Requiring Updates | 4 |
| Security Vulnerabilities | 2 packages |
| API Breaking Changes | 0 detected |

### Complexity Assessment

**Classification**: **Simple Solution**

**Rationale**:
- Small solution with only 3 projects
- Shallow dependency structure (2 levels)
- All projects currently on .NET 9.0 (no framework mixing)
- No API breaking changes detected
- All projects marked as Low complexity
- Clean dependency graph with no circular dependencies

### Critical Issues

**Security Vulnerabilities** (Must Address):
1. `Microsoft.AspNetCore.Identity` 2.3.0 → Requires update to 2.3.9
2. `Microsoft.AspNetCore.Mvc` 2.3.0 → Package contains security vulnerability

These packages are severely outdated (from .NET Core 2.x era) and contain known security vulnerabilities. They must be updated or removed as part of this upgrade.

### Selected Strategy

**All-At-Once Strategy** - All projects upgraded simultaneously in single coordinated operation.

**Rationale**:
- Small solution (3 projects) - ideal for atomic upgrade
- All projects on same framework version (net9.0)
- Simple, linear dependency structure
- Low overall complexity
- All packages have .NET 10 compatible versions available
- Enables fastest completion time
- Single comprehensive testing phase

### Iteration Strategy

**Fast Batch Approach** (2-3 detail iterations):
- All projects will be detailed in batched iterations due to low complexity
- Focus on security vulnerability remediation
- Single atomic upgrade operation across all projects

### Remaining Iterations
- **Phase 2**: Foundation (3 iterations) - Dependency analysis, strategy details, stubs
- **Phase 3**: Dynamic details (2 iterations) - Batch all project details, final sections
- **Total Expected Iterations**: 8 iterations

---

## Migration Strategy

### Approach Selection

**Selected Approach**: **All-At-Once Strategy**

All projects in the solution will be upgraded simultaneously in a single coordinated operation. All project files are updated to .NET 10.0, all package references updated, and the entire solution built and validated as one atomic change.

### Justification

**Why All-At-Once is Appropriate**:

✅ **Small Solution**: 3 projects (well under the 30-project threshold)
✅ **Simple Dependencies**: Linear dependency chain with no cycles
✅ **Uniform Framework**: All projects currently on .NET 9.0
✅ **Low Complexity**: Total 1,376 LOC, all projects marked Low complexity
✅ **Package Compatibility**: All required packages have .NET 10 versions
✅ **No Breaking Changes**: Assessment detected 0 API breaking changes
✅ **Clean History**: All projects already on modern .NET (not mixed .NET Framework)

**Advantages for This Solution**:
- **Fastest Completion**: Single upgrade operation vs. multiple phases
- **No Multi-Targeting**: Avoid complexity of supporting multiple frameworks
- **Atomic Validation**: Test entire solution in target state once
- **Simple Coordination**: All developers work with same framework simultaneously
- **Clean Package Resolution**: No version conflicts from multi-targeting

### All-At-Once Strategy Principles

**Simultaneity**:
- Update all 3 project files to `<TargetFramework>net10.0</TargetFramework>` in single operation
- Update all package references across all projects simultaneously
- Single `dotnet restore` and `dotnet build` for entire solution
- Fix all compilation errors in same operation
- No intermediate working states

**Single Atomic Operation**:
```
Update Projects → Update Packages → Restore → Build → Fix Errors → Verify → Test
```

All steps execute in sequence without intermediate commits until solution builds successfully.

### Dependency-Based Ordering Principles

While all projects update simultaneously, understanding dependency order helps with:

1. **Validation Sequence**: Test leaf nodes (DataModels) before dependent nodes (Contracts, Common)
2. **Troubleshooting**: If issues arise, check dependencies before dependants
3. **Smoke Testing**: Verify foundation projects work before higher-level projects

**Order for Validation**:
1. Core.Common.DataModels (leaf - no dependencies)
2. Core.Common.Contracts (depends on DataModels)
3. Core.Common (depends on Contracts)

### Execution Approach

**Parallel vs. Sequential**: 
- **File Updates**: Parallel - all .csproj files updated simultaneously
- **Package Updates**: Parallel - all PackageReferences updated simultaneously
- **Build**: Sequential - dependency order naturally enforced by MSBuild
- **Validation**: Sequential - leaf to root for smoke testing
- **Testing**: Comprehensive - entire solution tested as unit

### Phase Definition

**Phase 0: Preparation** (if needed)
- Verify .NET 10 SDK installed
- Verify current branch is `upgrade-to-NET10`
- Ensure clean working directory

**Phase 1: Atomic Upgrade** (Single Coordinated Operation)
- Update all project TargetFramework properties
- Update all package references
- Restore dependencies
- Build entire solution
- Address compilation errors
- Verify 0 build errors

**Phase 2: Validation**
- Run all tests (if test projects exist)
- Verify package security vulnerabilities resolved
- Confirm no dependency conflicts

### Risk Mitigation for All-At-Once

**Mitigation Strategies**:
1. **Pre-Upgrade Backup**: Working on feature branch `upgrade-to-NET10`
2. **Comprehensive Package List**: All package updates identified in assessment
3. **Security Focus**: Priority on resolving vulnerable packages
4. **Zero Tolerance**: Must achieve 0 build errors before proceeding
5. **Rollback Plan**: Git branch allows instant rollback to `master`

### Expected Timeline

**Single Atomic Operation**:
- Project file updates: Low complexity (3 files)
- Package reference updates: Low complexity (4 packages)
- Build and error resolution: Low complexity (0 breaking changes detected)
- Validation: Low complexity (no test projects identified)

**Overall Complexity**: Low - suitable for completion in single work session

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The solution has a clean, linear dependency structure with no circular dependencies:

```
Core.Common.DataModels (leaf - no dependencies)
    ↑
Core.Common.Contracts (depends on DataModels)
    ↑
Core.Common (depends on Contracts)
```

**Dependency Characteristics**:
- **Depth**: 2 levels
- **Leaf Nodes**: 1 (Core.Common.DataModels)
- **Root Nodes**: 1 (Core.Common - no dependants)
- **Circular Dependencies**: None
- **Total Dependencies**: 2 project references

### Project Groupings by Migration Phase

Given the All-At-Once strategy, all projects will be upgraded simultaneously in a single atomic operation. However, for understanding the dependency order:

**Phase 1: Atomic Upgrade** (All Projects Together)
1. Core.Common.DataModels (236 LOC, 0 dependencies)
2. Core.Common.Contracts (253 LOC, depends on DataModels)
3. Core.Common (887 LOC, depends on Contracts)

While all projects update simultaneously, testing validation will follow dependency order to ensure leaf nodes work before dependent nodes.

### Critical Path Identification

**Critical Path**: Core.Common.DataModels → Core.Common.Contracts → Core.Common

Since this is an All-At-Once migration, all projects change together, but the critical path helps identify:
- Where to focus initial validation (DataModels first)
- Order of smoke testing (leaf to root)
- Dependency resolution sequence during build

### Package Dependency Insights

**Shared Packages** (across multiple projects):
- `Microsoft.EntityFrameworkCore` 9.0.1 → 10.0.5
  - Used by: Core.Common.Contracts, Core.Common
  - Impact: 2 projects

- `REST-Parser` 1.2.5 (compatible)
  - Used by: Core.Common.Contracts, Core.Common
  - Impact: 2 projects

**Project-Specific Packages**:
- Core.Common.DataModels: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 9.0.1 → 10.0.5
- Core.Common: Multiple packages (see project details)

### Migration Order Justification

**All-At-Once Approach Rationale**:
1. **Small Scale**: Only 3 projects with clear dependencies
2. **Uniform State**: All on .NET 9.0, all targeting .NET 10.0
3. **No Multi-Targeting**: No need for intermediate netstandard compatibility
4. **Package Alignment**: All required packages have .NET 10 versions
5. **Testing Efficiency**: Single comprehensive test pass vs. multiple phases
6. **Minimal Risk**: No breaking API changes detected

---

## Project-by-Project Plans

### Core.Common.DataModels

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary (SDK-style)
- Lines of Code: 253
- Files: 18
- Dependencies: 0 project references (leaf node)
- Dependants: 1 (Core.Common.Contracts)
- Packages: 1
- APIs Analyzed: 213 (all compatible)
- Risk Level: 🟢 Low

**Target State**:
- Target Framework: net10.0
- Package Count: 1 (same)

#### Migration Steps

**1. Prerequisites**
- ✅ .NET 10 SDK installed
- ✅ Working on `upgrade-to-NET10` branch
- ✅ Core.Common.DataModels is leaf node (no dependencies to migrate first)

**2. Framework Update**
Update project file: `Core.Common.DataModels\Core.Common.DataModels.csproj`

Change:
```xml
<TargetFramework>net9.0</TargetFramework>
```

To:
```xml
<TargetFramework>net10.0</TargetFramework>
```

**3. Package Updates**

| Package | Current Version | Target Version | Reason |
| :--- | :---: | :---: | :--- |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 9.0.1 | 10.0.5 | Framework alignment, recommended upgrade |

**Update in .csproj**:
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.5" />
```

**4. Expected Breaking Changes**

**Assessment Result**: 0 breaking changes detected

**Entity Framework Core 9 → 10 Considerations**:
- Review EF Core 10 release notes for behavioral changes
- Check identity schema compatibility
- Verify migration compatibility if using EF migrations

**Low Risk Areas**:
- Identity data models (IdentityUser, IdentityRole, etc.)
- DbContext configurations
- Entity relationships

**5. Code Modifications**

**Expected**: No code changes required

**If Issues Arise**:
- Check for EF Core obsolete API warnings
- Review identity entity base class changes
- Verify IdentityDbContext configuration

**6. Testing Strategy**

**Build Validation**:
- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] No package dependency conflicts

**Functional Validation**:
- [ ] Identity entities still properly configured
- [ ] No breaking changes in IdentityDbContext
- [ ] Package vulnerability resolved (verify EF Core 10.0.5)

**Integration Validation**:
- [ ] Core.Common.Contracts can reference updated DataModels
- [ ] No breaking changes for dependent projects

**7. Validation Checklist**

- [ ] TargetFramework updated to net10.0
- [ ] Microsoft.AspNetCore.Identity.EntityFrameworkCore updated to 10.0.5
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet build` produces 0 warnings
- [ ] No NuGet package conflicts
- [ ] All 213 APIs remain compatible (per assessment)
- [ ] Dependent projects (Contracts) still build

---

### Core.Common.Contracts

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary (SDK-style)
- Lines of Code: 236
- Files: 16
- Dependencies: 1 project reference (Core.Common.DataModels)
- Dependants: 1 (Core.Common)
- Packages: 2
- APIs Analyzed: 67 (all compatible)
- Risk Level: 🟢 Low

**Target State**:
- Target Framework: net10.0
- Package Count: 2 (same)

#### Migration Steps

**1. Prerequisites**
- ✅ .NET 10 SDK installed
- ✅ Working on `upgrade-to-NET10` branch
- ✅ Core.Common.DataModels updated to net10.0 (dependency)

**2. Framework Update**
Update project file: `Core.Common.Contracts\Core.Common.Contracts.csproj`

Change:
```xml
<TargetFramework>net9.0</TargetFramework>
```

To:
```xml
<TargetFramework>net10.0</TargetFramework>
```

**3. Package Updates**

| Package | Current Version | Target Version | Reason |
| :--- | :---: | :---: | :--- |
| Microsoft.EntityFrameworkCore | 9.0.1 | 10.0.5 | Framework alignment, recommended upgrade |
| REST-Parser | 1.2.5 | 1.2.5 | Compatible, no update needed |

**Update in .csproj**:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.5" />
```

**4. Expected Breaking Changes**

**Assessment Result**: 0 breaking changes detected

**Entity Framework Core 9 → 10 Considerations**:
- Review EF Core 10 release notes
- Check DbContext configurations
- Verify LINQ query translations
- Review change tracking behavior changes

**Potential Areas** (low probability):
- IRepository interface using EF Core types
- DbContext factory patterns
- Query expression trees

**5. Code Modifications**

**Expected**: No code changes required

**If Issues Arise**:
- Check for EF Core obsolete API warnings in interfaces
- Review IRepository<T> implementations using EF Core
- Verify DbContext-related interfaces
- Check REST-Parser compatibility with .NET 10

**Areas to Review** (contracts/interfaces):
- IRepository, IReadRepository
- IUnitOfWork
- IDatabaseFactory
- Any interfaces using EF Core types

**6. Testing Strategy**

**Build Validation**:
- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] No package dependency conflicts
- [ ] Core.Common.DataModels reference resolves correctly

**Functional Validation**:
- [ ] Repository interfaces compile successfully
- [ ] EF Core types in interfaces still valid
- [ ] REST-Parser integration intact

**Integration Validation**:
- [ ] Core.Common can reference updated Contracts
- [ ] DataModels → Contracts reference chain works
- [ ] No breaking changes for dependent projects

**7. Validation Checklist**

- [ ] TargetFramework updated to net10.0
- [ ] Microsoft.EntityFrameworkCore updated to 10.0.5
- [ ] REST-Parser remains at 1.2.5 (compatible)
- [ ] ProjectReference to Core.Common.DataModels intact
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet build` produces 0 warnings
- [ ] No NuGet package conflicts
- [ ] All 67 APIs remain compatible (per assessment)
- [ ] Dependent projects (Core.Common) still build

---

### Core.Common

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary (SDK-style)
- Lines of Code: 887
- Files: 16
- Dependencies: 1 project reference (Core.Common.Contracts)
- Dependants: 0 (root node)
- Packages: 7
- APIs Analyzed: 1,130 (all compatible)
- Risk Level: 🟡 Medium (due to security-vulnerable packages)

**Target State**:
- Target Framework: net10.0
- Package Count: 7 (same, but 2 require security updates)

#### Migration Steps

**1. Prerequisites**
- ✅ .NET 10 SDK installed
- ✅ Working on `upgrade-to-NET10` branch
- ✅ Core.Common.Contracts updated to net10.0 (dependency)
- ✅ Core.Common.DataModels updated to net10.0 (transitive dependency)

**2. Framework Update**
Update project file: `Core.Common\Core.Common.csproj`

Change:
```xml
<TargetFramework>net9.0</TargetFramework>
```

To:
```xml
<TargetFramework>net10.0</TargetFramework>
```

**3. Package Updates**

**CRITICAL - Security Vulnerabilities** (Priority 1):

| Package | Current | Target | Reason | Notes |
| :--- | :---: | :---: | :--- | :--- |
| Microsoft.AspNetCore.Identity | 2.3.0 | 2.3.9 | **Security vulnerability** | 🔴 From .NET Core 2.x era (~2018), outdated |
| Microsoft.AspNetCore.Mvc | 2.3.0 | **ASSESS** | **Security vulnerability** | 🔴 May not be needed in class library |

**Standard Updates** (Priority 2):

| Package | Current | Target | Reason |
| :--- | :---: | :---: | :--- |
| Microsoft.EntityFrameworkCore | 9.0.1 | 10.0.5 | Framework alignment |
| Newtonsoft.Json | 13.0.3 | 13.0.4 | Recommended patch update |

**Compatible - Optional Updates** (Priority 3):

| Package | Current | Available | Status |
| :--- | :---: | :---: | :--- |
| System.IdentityModel.Tokens.Jwt | 8.3.1 | 8.17.0 | Compatible, update recommended |
| Ardalis.GuardClauses | 5.0.0 | - | Compatible, no update needed |
| REST-Parser | 1.2.5 | - | Compatible, no update needed |

**Recommended .csproj Updates**:
```xml
<!-- Security Critical Updates -->
<PackageReference Include="Microsoft.AspNetCore.Identity" Version="2.3.9" />
<!-- OR ASSESS IF NEEDED - class libraries typically don't need AspNetCore packages -->

<!-- Framework Alignment -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.5" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.17.0" />

<!-- Keep as-is (compatible) -->
<PackageReference Include="Ardalis.GuardClauses" Version="5.0.0" />
<PackageReference Include="REST-Parser" Version="1.2.5" />
```

**⚠️ IMPORTANT ASSESSMENT REQUIRED**:
Before updating `Microsoft.AspNetCore.Mvc` and `Microsoft.AspNetCore.Identity`:
1. **Determine if Core.Common actually uses these packages**
   - Core.Common is a class library providing repository/data service patterns
   - ASP.NET Core MVC and Identity are typically needed in web applications, not class libraries
   - These may be legacy dependencies from earlier project structure

2. **If NOT used**: **REMOVE** these packages entirely (safest option)
3. **If USED**: Update to .NET 10 compatible versions:
   - Microsoft.AspNetCore.Identity → Consider updating to 10.0.x range
   - Microsoft.AspNetCore.Mvc → Consider updating to 10.0.x range

**4. Expected Breaking Changes**

**Assessment Result**: 0 breaking changes detected

**However, given package age (2.3.0 from 2018), potential issues exist**:

**Microsoft.AspNetCore.Identity 2.3.0 → 2.3.9** (if keeping):
- Stay within 2.3.x range: Low risk
- But package is 6+ years old, from .NET Core 2.x era
- **Consider**: Why does a class library need AspNetCore.Identity?
- **Risk**: API may not be compatible with .NET 10

**Microsoft.AspNetCore.Mvc 2.3.0** (if keeping):
- Marked as "Compatible" but contains security vulnerability
- From .NET Core 2.x era
- **Consider**: Why does a class library need AspNetCore.Mvc?
- **Likely candidate for REMOVAL**

**Entity Framework Core 9 → 10**:
- Low risk, minor version update
- Check for behavioral changes in:
  - LINQ query translation
  - Change tracking
  - DbContext lifecycle
  - Repository base class implementations

**System.IdentityModel.Tokens.Jwt 8.3.1 → 8.17.0**:
- Patch/minor update within v8
- Low risk
- Check JWT token service implementations

**5. Code Modifications**

**Expected**: Minimal to no code changes

**Areas Requiring Review**:

**BaseRepository / BaseDataService Classes**:
- Check for EF Core obsolete API usage
- Verify DbContext factory patterns
- Review LINQ queries for translation changes
- Check change tracking behavior

**Token Services** (if using JWT):
- Verify System.IdentityModel.Tokens.Jwt 8.17.0 compatibility
- Check token generation/validation logic
- Review claims handling

**If AspNetCore Packages REMOVED**:
- Remove any using statements for AspNetCore.Identity or AspNetCore.Mvc
- Remove any types from these namespaces
- Likely impact: NONE (if not actually used)

**If AspNetCore Packages KEPT**:
- Update to .NET 10 compatible versions (10.0.x)
- Check for breaking changes in Identity API
- Check for breaking changes in MVC API
- Review authentication/authorization flows

**Potential Code Pattern Changes**:
```csharp
// Check for patterns like:
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// If found and not needed, remove references
```

**6. Testing Strategy**

**Build Validation**:
- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] No package dependency conflicts
- [ ] Core.Common.Contracts reference resolves correctly

**Security Validation** (CRITICAL):
- [ ] Microsoft.AspNetCore.Identity security vulnerability resolved
- [ ] Microsoft.AspNetCore.Mvc security vulnerability resolved
- [ ] Run `dotnet list package --vulnerable` to verify no vulnerabilities

**Functional Validation**:
- [ ] Repository base classes compile successfully
- [ ] Data service base classes compile successfully
- [ ] Token service implementations work (if using JWT)
- [ ] EF Core DbContext factory patterns intact
- [ ] Guard clause validations still function

**Package Assessment Validation**:
- [ ] Confirmed AspNetCore.Identity usage or removed package
- [ ] Confirmed AspNetCore.Mvc usage or removed package
- [ ] All package versions appropriate for .NET 10

**API Compatibility**:
- [ ] All 1,130 APIs remain compatible (per assessment)
- [ ] No breaking API changes in updated packages
- [ ] Repository pattern interfaces unchanged
- [ ] Data service pattern interfaces unchanged

**7. Validation Checklist**

- [ ] TargetFramework updated to net10.0
- [ ] **SECURITY**: Microsoft.AspNetCore.Identity vulnerability addressed (updated or removed)
- [ ] **SECURITY**: Microsoft.AspNetCore.Mvc vulnerability addressed (updated or removed)
- [ ] Microsoft.EntityFrameworkCore updated to 10.0.5
- [ ] Newtonsoft.Json updated to 13.0.4
- [ ] System.IdentityModel.Tokens.Jwt updated to 8.17.0 (recommended)
- [ ] Ardalis.GuardClauses remains at 5.0.0
- [ ] REST-Parser remains at 1.2.5
- [ ] ProjectReference to Core.Common.Contracts intact
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet build` produces 0 warnings
- [ ] `dotnet list package --vulnerable` shows 0 vulnerabilities
- [ ] No NuGet package conflicts
- [ ] All repository base classes function correctly
- [ ] All data service base classes function correctly

#### Special Considerations

**AspNetCore Package Assessment**:

This is a **class library** providing common infrastructure (repository pattern, data services). Typically, class libraries do NOT need:
- Microsoft.AspNetCore.Identity (for web authentication)
- Microsoft.AspNetCore.Mvc (for web MVC framework)

**Investigation Required**:
1. Search codebase for actual usage of AspNetCore.Identity types
2. Search codebase for actual usage of AspNetCore.Mvc types
3. If no usage found → **REMOVE packages** (safest, eliminates security risk)
4. If usage found → Understand why a class library needs web framework packages

**Recommended Approach**:
- Attempt build with AspNetCore packages removed
- If build succeeds → Keep them removed
- If build fails → Assess minimal package needed and update to .NET 10 version

---

## Risk Management

### High-Level Assessment

**Overall Risk Level**: **Low to Medium**

The upgrade presents low technical risk due to small scope and lack of breaking changes, but medium risk exists due to outdated security-vulnerable packages requiring significant version jumps.

### Risk Table

| Project/Area | Risk Level | Description | Mitigation |
| :--- | :---: | :--- | :--- |
| Core.Common.DataModels | 🟢 Low | Simple upgrade, 1 package update, no dependencies | Test after upgrade, verify EF Core compatibility |
| Core.Common.Contracts | 🟢 Low | Simple upgrade, 1 package update, 1 project dependency | Verify DataModels reference works, test EF Core |
| Core.Common | 🟡 Medium | 7 packages including 2 with security vulnerabilities | Prioritize security packages, extensive testing, verify API compatibility |
| Microsoft.AspNetCore.Identity | 🔴 High | Version 2.3.0 → 2.3.9 (security vulnerability) | Update immediately, test authentication/authorization flows |
| Microsoft.AspNetCore.Mvc | 🔴 High | Version 2.3.0 (security vulnerability, may need removal) | Assess if needed for class library, consider removal |
| All-At-Once Strategy | 🟡 Medium | All projects change simultaneously, larger test surface | Feature branch isolation, comprehensive validation |

### Security Vulnerabilities

**Critical Security Issues** (Must Address):

1. **Microsoft.AspNetCore.Identity 2.3.0**
   - **Severity**: Security vulnerability
   - **Current**: 2.3.0 (from .NET Core 2.x era, ~2018)
   - **Recommended**: 2.3.9 (assessment recommendation)
   - **Remediation**: Update to recommended version immediately
   - **Impact**: Used in Core.Common project
   - **Concern**: 5+ year old package with known vulnerabilities

2. **Microsoft.AspNetCore.Mvc 2.3.0**
   - **Severity**: Security vulnerability (per assessment)
   - **Current**: 2.3.0 (from .NET Core 2.x era, ~2018)
   - **Assessment**: Marked as "Compatible" but flagged for security
   - **Remediation Options**:
     - Option A: Update to latest compatible version
     - Option B: **Remove if not needed** (Core.Common is a class library, may not need MVC)
   - **Impact**: Used in Core.Common project
   - **Recommendation**: Assess necessity first; class libraries typically don't need AspNetCore.Mvc

### Package Update Risks

**Moderate Risk Updates**:

| Package | Current | Target | Risk | Mitigation |
| :--- | :---: | :---: | :--- | :--- |
| Microsoft.EntityFrameworkCore | 9.0.1 | 10.0.5 | 🟡 Medium | Minor version jump, review EF Core 10 breaking changes, test migrations |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 9.0.1 | 10.0.5 | 🟡 Medium | Aligns with EF Core version, test identity schema |
| Newtonsoft.Json | 13.0.3 | 13.0.4 | 🟢 Low | Patch update only |

**Low Risk (Compatible)**:
- Ardalis.GuardClauses 5.0.0 - No update needed
- REST-Parser 1.2.5 - No update needed
- System.IdentityModel.Tokens.Jwt 8.3.1 - Compatible (though 8.17.0 available)

### Contingency Plans

**If Build Fails After Framework Update**:
1. Review compilation errors for patterns
2. Check for obsolete API usage in error messages
3. Consult .NET 10 breaking changes documentation
4. Fix errors incrementally, rebuild after each fix
5. If unresolvable, rollback via Git: `git checkout master`

**If Security Package Updates Cause Issues**:
1. **For Microsoft.AspNetCore.Identity**:
   - Try intermediate versions (2.3.5, 2.3.7) if 2.3.9 fails
   - Review identity schema compatibility
   - Check if authentication flows still work

2. **For Microsoft.AspNetCore.Mvc**:
   - Assess if Core.Common truly needs this package
   - Class libraries typically don't require MVC
   - Consider removing entirely if not used
   - If needed, update to .NET 10 compatible version

**If Package Conflicts Arise**:
1. Run `dotnet restore --force` to clear cache
2. Check for transitive dependency conflicts
3. Explicitly specify versions in Directory.Packages.props if needed
4. Use `dotnet list package --include-transitive` to diagnose

**Rollback Strategy**:
- All work on `upgrade-to-NET10` branch
- Master branch remains stable
- Instant rollback: `git checkout master`
- No production impact during upgrade

### Unknown Risks

**Items Requiring Investigation**:
- ⚠️ Purpose of AspNetCore.Mvc in class library (Core.Common)
- ⚠️ Purpose of AspNetCore.Identity in class library (Core.Common)
- ⚠️ Whether REST-Parser 1.2.5 fully compatible with .NET 10 (marked compatible but may need testing)
- ⚠️ Behavioral changes in EF Core 10 vs. EF Core 9

---

## Testing & Validation Strategy

### Overview

Since no test projects were identified in the assessment, validation will focus on build success, package vulnerability resolution, and smoke testing of core functionality.

### Multi-Level Testing Approach

#### Phase 1: Per-Project Build Validation

After each project update in the atomic upgrade, verify:

**Core.Common.DataModels**:
- [ ] `dotnet restore Core.Common.DataModels\Core.Common.DataModels.csproj` succeeds
- [ ] `dotnet build Core.Common.DataModels\Core.Common.DataModels.csproj` succeeds
- [ ] 0 build errors
- [ ] 0 build warnings
- [ ] No package conflicts

**Core.Common.Contracts**:
- [ ] `dotnet restore Core.Common.Contracts\Core.Common.Contracts.csproj` succeeds
- [ ] `dotnet build Core.Common.Contracts\Core.Common.Contracts.csproj` succeeds
- [ ] 0 build errors
- [ ] 0 build warnings
- [ ] No package conflicts
- [ ] DataModels reference resolves correctly

**Core.Common**:
- [ ] `dotnet restore Core.Common\Core.Common.csproj` succeeds
- [ ] `dotnet build Core.Common\Core.Common.csproj` succeeds
- [ ] 0 build errors
- [ ] 0 build warnings
- [ ] No package conflicts
- [ ] Contracts reference resolves correctly

#### Phase 2: Solution-Wide Validation

After all projects updated in atomic operation:

**Build Validation**:
- [ ] `dotnet restore SilverCodeAPI.sln` succeeds
- [ ] `dotnet build SilverCodeAPI.sln` succeeds
- [ ] All 3 projects build successfully
- [ ] 0 errors across entire solution
- [ ] 0 warnings across entire solution

**Package Validation**:
- [ ] `dotnet list package` shows all packages at expected versions
- [ ] `dotnet list package --vulnerable` shows **0 vulnerabilities**
- [ ] `dotnet list package --outdated` checked for awareness
- [ ] No package version conflicts

**Dependency Validation**:
- [ ] Project references resolve correctly
- [ ] Transitive dependencies align properly
- [ ] No circular dependency issues

#### Phase 3: Smoke Testing

**Compilation Smoke Tests**:
- [ ] All interfaces compile (IRepository, IReadRepository, IUnitOfWork, etc.)
- [ ] All base classes compile (BaseRepository, BaseDataService, etc.)
- [ ] All data models compile (IdentityUser extensions, etc.)

**Security Vulnerability Verification**:
- [ ] Microsoft.AspNetCore.Identity vulnerability **RESOLVED** (updated or removed)
- [ ] Microsoft.AspNetCore.Mvc vulnerability **RESOLVED** (updated or removed)
- [ ] Run vulnerability scan confirms 0 critical/high/medium vulnerabilities

**Framework Compatibility**:
- [ ] All projects target net10.0
- [ ] No net9.0 references remain
- [ ] Package versions compatible with .NET 10

#### Phase 4: Manual Verification (if applicable)

If this solution is referenced by consuming applications:

**Consumer Build Test**:
- [ ] Build consuming application that references these libraries
- [ ] Verify no breaking changes in public APIs
- [ ] Check for runtime compatibility

**Package Generation Test** (if NuGet package produced):
- [ ] `dotnet pack` succeeds for Core.Common
- [ ] NuGet package targets net10.0
- [ ] Package dependencies reflect updated versions

### Testing Checklist by Project

#### Core.Common.DataModels
- [ ] Builds without errors
- [ ] Builds without warnings
- [ ] Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.5 installed
- [ ] Identity data models compile
- [ ] No API breaking changes

#### Core.Common.Contracts
- [ ] Builds without errors
- [ ] Builds without warnings
- [ ] Microsoft.EntityFrameworkCore 10.0.5 installed
- [ ] REST-Parser 1.2.5 compatible
- [ ] All interface definitions compile
- [ ] Repository interfaces intact

#### Core.Common
- [ ] Builds without errors
- [ ] Builds without warnings
- [ ] **All security vulnerabilities resolved**
- [ ] Microsoft.EntityFrameworkCore 10.0.5 installed
- [ ] System.IdentityModel.Tokens.Jwt 8.17.0 installed
- [ ] Newtonsoft.Json 13.0.4 installed
- [ ] All base classes compile
- [ ] Repository implementations intact
- [ ] Data service implementations intact

### Success Criteria for Testing Phase

**Mandatory** (must pass):
- ✅ All projects build with 0 errors
- ✅ All projects build with 0 warnings
- ✅ **0 security vulnerabilities** reported by `dotnet list package --vulnerable`
- ✅ All project references resolve correctly
- ✅ All package dependencies compatible

**Recommended** (should verify):
- ✅ Consuming applications still build (if applicable)
- ✅ NuGet packages generate successfully (if applicable)
- ✅ All public APIs remain compatible

### Validation Commands

Run these commands to validate the upgrade:

```bash
# Build entire solution
dotnet restore SilverCodeAPI.sln
dotnet build SilverCodeAPI.sln --no-restore

# Check for vulnerabilities (MUST show 0)
dotnet list package --vulnerable

# Check package versions
dotnet list package

# Check for outdated packages (informational)
dotnet list package --outdated

# Build each project individually (if solution build fails)
dotnet build Core.Common.DataModels\Core.Common.DataModels.csproj
dotnet build Core.Common.Contracts\Core.Common.Contracts.csproj
dotnet build Core.Common\Core.Common.csproj

# Generate packages (if applicable)
dotnet pack Core.Common\Core.Common.csproj --configuration Release
```

### Rollback Criteria

If any of these conditions occur, **ROLLBACK** the upgrade:

- ❌ Build errors cannot be resolved after reasonable effort
- ❌ Security vulnerabilities still present after package updates
- ❌ Breaking changes in public APIs affect consuming applications
- ❌ Critical functionality broken with no clear resolution

**Rollback Command**:
```bash
git checkout master
# or
git reset --hard origin/master
```

---

## Complexity & Effort Assessment

### Overall Complexity: **Low**

The solution upgrade is straightforward due to small size, clean dependencies, and lack of detected breaking changes.

### Per-Project Complexity

| Project | Complexity | LOC | Packages | Dependencies | Risk Factors |
| :--- | :---: | :---: | :---: | :---: | :--- |
| Core.Common.DataModels | 🟢 Low | 253 | 1 | 0 | Leaf node, single package update |
| Core.Common.Contracts | 🟢 Low | 236 | 2 | 1 | Single project dependency, 1 package update |
| Core.Common | 🟡 Medium | 887 | 7 | 1 | 7 packages, 2 security vulnerabilities, largest codebase |

**Complexity Ratings**:
- 🟢 **Low**: Straightforward upgrade, minimal changes, low risk
- 🟡 **Medium**: Moderate package updates, security vulnerabilities to address
- 🔴 **High**: (none in this solution)

### Phase Complexity Assessment

**Phase 1: Atomic Upgrade**
- **Complexity**: 🟡 Medium
- **Factors**:
  - 3 project files to update (Low)
  - 4 package updates required (Low)
  - 2 security-vulnerable packages requiring attention (Medium)
  - Severely outdated AspNetCore packages (2.3.0 from 2018) (Medium)
  - 0 detected breaking changes (Low)
- **Dependencies**: Must follow dependency order for validation
- **Effort**: Low to Medium - security packages may require investigation

**Phase 2: Validation**
- **Complexity**: 🟢 Low
- **Factors**:
  - No test projects identified in assessment (Low)
  - Simple build validation only (Low)
  - Package vulnerability verification (Low)
- **Effort**: Low - primarily automated validation

### Relative Effort by Activity

| Activity | Complexity | Notes |
| :--- | :---: | :--- |
| Update TargetFramework Properties | 🟢 Low | 3 simple .csproj edits |
| Update Package References | 🟡 Medium | 4 packages, focus on security packages |
| Restore Dependencies | 🟢 Low | Automated `dotnet restore` |
| Build Solution | 🟢 Low | 0 breaking changes detected |
| Fix Compilation Errors | 🟢 Low | Expected 0 errors |
| Address Security Packages | 🟡 Medium | May require version research, compatibility testing |
| Validate AspNetCore Packages | 🟡 Medium | Assess necessity in class library context |
| Test Solution | 🟢 Low | No test projects, basic smoke testing |

### Resource Requirements

**Skills Required**:
- .NET framework upgrade experience (Basic)
- NuGet package management (Basic)
- Understanding of project dependencies (Basic)
- Security vulnerability assessment (Intermediate - for AspNetCore packages)
- Entity Framework Core knowledge (Basic - for EF Core 10 update)

**Parallel Work Capacity**:
- All-At-Once strategy requires sequential execution
- Single developer can complete upgrade
- No parallelization opportunities due to atomic operation

**Estimated Effort Distribution**:
- Project file updates: 15% (straightforward)
- Package reference updates: 30% (majority of effort, especially security packages)
- Build and error resolution: 20% (expected to be minimal)
- Security package assessment: 25% (determining necessity, compatibility)
- Validation and testing: 10% (no test projects)

### Key Effort Drivers

**Primary Effort Drivers**:
1. **Security Package Updates**: Assessing AspNetCore.Identity and AspNetCore.Mvc necessity and compatibility
2. **Package Version Research**: Determining correct versions for severely outdated packages
3. **Validation**: Ensuring security vulnerabilities resolved

**Secondary Effort Drivers**:
1. Entity Framework Core 10 compatibility verification
2. REST-Parser compatibility testing
3. General smoke testing of repository/data service patterns

---

## Source Control Strategy

### Branch Strategy

**Current Branch Structure**:
- **Source Branch**: `master` (stable, pre-upgrade state)
- **Upgrade Branch**: `upgrade-to-NET10` (current working branch, isolated changes)

**Rationale**:
- Feature branch isolation protects master from in-progress changes
- Easy rollback capability (`git checkout master`)
- Clear separation of .NET 9 (master) vs .NET 10 (upgrade branch)
- Enables PR-based review before merge

### Commit Strategy

**All-At-Once Strategy Approach**: Single comprehensive commit

Given the All-At-Once strategy where all projects update simultaneously, the recommended approach is:

**Single Atomic Commit** (Preferred):
- All project file updates
- All package reference updates
- All compilation fixes
- Commit only after entire solution builds successfully with 0 errors

**Commit Message Format**:
```
feat: Upgrade solution to .NET 10

- Update all projects from net9.0 to net10.0
- Update Microsoft.EntityFrameworkCore 9.0.1 → 10.0.5
- Update Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.1 → 10.0.5
- Resolve security vulnerabilities:
  - Microsoft.AspNetCore.Identity 2.3.0 → 2.3.9 (or removed)
  - Microsoft.AspNetCore.Mvc 2.3.0 (resolved/removed)
- Update Newtonsoft.Json 13.0.3 → 13.0.4
- Update System.IdentityModel.Tokens.Jwt 8.3.1 → 8.17.0

Projects updated:
- Core.Common.DataModels
- Core.Common.Contracts
- Core.Common

Validated: 0 build errors, 0 warnings, 0 security vulnerabilities
```

**Alternative: Checkpoint Commits** (if needed):

If atomic approach becomes unwieldy, consider these checkpoints:

1. **Checkpoint 1**: Project file updates only
   ```
   chore: Update all projects to target net10.0
   ```

2. **Checkpoint 2**: Package reference updates
   ```
   chore: Update package references for .NET 10 compatibility
   ```

3. **Checkpoint 3**: Compilation fixes
   ```
   fix: Address compilation errors from .NET 10 upgrade
   ```

4. **Checkpoint 4**: Security resolution
   ```
   security: Resolve package vulnerabilities
   ```

However, **single atomic commit is preferred** for All-At-Once strategy.

### Commit Best Practices

**What to Include**:
- ✅ All .csproj file changes
- ✅ All package reference updates
- ✅ Any code changes required for compilation
- ✅ Any configuration file changes
- ✅ Update to package release notes (if maintaining)

**What to Exclude**:
- ❌ Unrelated code changes
- ❌ Refactoring not required by upgrade
- ❌ New features
- ❌ bin/obj directories (should be in .gitignore)
- ❌ IDE-specific files

**Verification Before Commit**:
```bash
# Check status
git status

# Review changes
git diff

# Verify build succeeds
dotnet build SilverCodeAPI.sln

# Verify no vulnerabilities
dotnet list package --vulnerable

# Stage changes
git add Core.Common.DataModels/Core.Common.DataModels.csproj
git add Core.Common.Contracts/Core.Common.Contracts.csproj
git add Core.Common/Core.Common.csproj

# Include any additional changed files
git add [other-changed-files]

# Commit
git commit -m "feat: Upgrade solution to .NET 10"
```

### Review and Merge Process

**Pre-Merge Checklist**:

Before creating PR or merging to master:

**Build Verification**:
- [ ] `dotnet build SilverCodeAPI.sln` succeeds with 0 errors
- [ ] `dotnet build SilverCodeAPI.sln` succeeds with 0 warnings
- [ ] All 3 projects build successfully

**Security Verification**:
- [ ] `dotnet list package --vulnerable` shows **0 vulnerabilities**
- [ ] Microsoft.AspNetCore.Identity vulnerability resolved
- [ ] Microsoft.AspNetCore.Mvc vulnerability resolved

**Package Verification**:
- [ ] All package versions match plan specifications
- [ ] No package conflicts
- [ ] All packages compatible with net10.0

**Code Review Criteria** (if using PR):
- [ ] All .csproj files updated to net10.0
- [ ] Package versions match plan
- [ ] No unrelated changes included
- [ ] Commit message clear and descriptive
- [ ] Security vulnerabilities addressed

**Merge Command**:
```bash
# Switch to master
git checkout master

# Merge upgrade branch
git merge upgrade-to-NET10

# Push to remote
git push origin master

# Optionally delete upgrade branch
git branch -d upgrade-to-NET10
git push origin --delete upgrade-to-NET10
```

### Alternative: Pull Request Workflow

If using GitHub/Azure DevOps/GitLab:

1. **Create PR**: `upgrade-to-NET10` → `master`
2. **PR Title**: "Upgrade solution to .NET 10"
3. **PR Description**: Include commit message details + verification checklist
4. **Request Review**: Technical lead or team review
5. **Verify CI/CD**: Ensure automated builds pass (if configured)
6. **Approve and Merge**: Squash commits or merge based on team preference

**PR Template**:
```markdown
## Summary
Upgrades SilverCodeAPI solution from .NET 9 to .NET 10 (LTS).

## Changes
- All projects updated to target net10.0
- Package updates for .NET 10 compatibility
- Security vulnerabilities resolved

## Projects Affected
- Core.Common.DataModels
- Core.Common.Contracts
- Core.Common

## Security Fixes
- Microsoft.AspNetCore.Identity: 2.3.0 → 2.3.9 (or removed)
- Microsoft.AspNetCore.Mvc: 2.3.0 (resolved/removed)

## Verification
- [x] Solution builds with 0 errors
- [x] Solution builds with 0 warnings
- [x] 0 security vulnerabilities (`dotnet list package --vulnerable`)
- [x] All package versions updated per plan
- [x] No breaking API changes

## Testing
- [x] All projects build individually
- [x] Solution builds successfully
- [x] Package generation succeeds (if applicable)
```

### Rollback Strategy

**If Issues Found After Merge**:

```bash
# Option 1: Revert the merge commit
git revert -m 1 <merge-commit-hash>

# Option 2: Reset to pre-merge state (if not pushed to remote)
git reset --hard HEAD~1

# Option 3: Create new branch from master and cherry-pick fixes
git checkout -b hotfix-net10-issues master
git cherry-pick <fix-commit>
```

**Protection**:
- Master branch remains stable throughout upgrade
- Upgrade work isolated on feature branch
- Can always return to working state via `git checkout master`

### Post-Merge Activities

**After Successful Merge**:
1. Update documentation with .NET 10 target
2. Update CI/CD pipelines to use .NET 10 SDK (if applicable)
3. Notify team of framework change
4. Update any deployment documentation
5. Verify consuming applications still compatible (if applicable)
6. Tag release (optional): `git tag v1.2.10-net10`

---

## Success Criteria

### The migration is complete when:

#### Technical Criteria (Mandatory)

**Framework Migration**:
- ✅ All 3 projects target net10.0
- ✅ No projects remain on net9.0
- ✅ All .csproj files updated with `<TargetFramework>net10.0</TargetFramework>`

**Package Updates**:
- ✅ Microsoft.EntityFrameworkCore updated from 9.0.1 to 10.0.5 (2 projects)
- ✅ Microsoft.AspNetCore.Identity.EntityFrameworkCore updated from 9.0.1 to 10.0.5 (1 project)
- ✅ Newtonsoft.Json updated from 13.0.3 to 13.0.4 (1 project)
- ✅ System.IdentityModel.Tokens.Jwt updated from 8.3.1 to 8.17.0 (recommended, 1 project)

**Security Resolution** (CRITICAL):
- ✅ **Microsoft.AspNetCore.Identity security vulnerability RESOLVED**
  - Either updated to 2.3.9+ OR removed from project
- ✅ **Microsoft.AspNetCore.Mvc security vulnerability RESOLVED**
  - Either updated to secure version OR removed from project
- ✅ `dotnet list package --vulnerable` reports **0 vulnerabilities**
- ✅ No critical, high, or medium severity vulnerabilities remain

**Build Success**:
- ✅ `dotnet restore SilverCodeAPI.sln` succeeds
- ✅ `dotnet build SilverCodeAPI.sln` succeeds with **0 errors**
- ✅ `dotnet build SilverCodeAPI.sln` succeeds with **0 warnings**
- ✅ All 3 projects build individually without errors
- ✅ All 3 projects build individually without warnings

**Dependency Resolution**:
- ✅ No NuGet package dependency conflicts
- ✅ All project references resolve correctly:
  - Core.Common → Core.Common.Contracts ✓
  - Core.Common.Contracts → Core.Common.DataModels ✓
- ✅ Transitive dependencies align properly
- ✅ No version conflicts in dependency graph

**API Compatibility**:
- ✅ All 1,410 APIs analyzed remain compatible (per assessment)
- ✅ No breaking changes in public interfaces
- ✅ Repository pattern APIs unchanged
- ✅ Data service pattern APIs unchanged

#### Quality Criteria

**Code Quality**:
- ✅ No compiler warnings introduced by upgrade
- ✅ No obsolete API usage warnings
- ✅ All base classes compile successfully:
  - BaseRepository and variants
  - BaseDataService and variants
  - BaseTokenService (if applicable)
- ✅ All interfaces compile successfully:
  - IRepository, IReadRepository
  - IUnitOfWork
  - IDatabaseFactory
  - IAuditor

**Test Coverage Maintained**:
- ✅ No test projects identified, N/A for this solution
- ✅ If consuming applications exist, their tests still pass

**Documentation Updated**:
- ✅ README updated with .NET 10 target (if applicable)
- ✅ Package release notes updated (Core.Common.csproj has PackageReleaseNotes)
- ✅ Version numbers incremented appropriately

#### Process Criteria

**All-At-Once Strategy Execution**:
- ✅ All 3 projects updated simultaneously in single atomic operation
- ✅ No intermediate multi-targeting states
- ✅ Single comprehensive validation pass
- ✅ All changes coordinated and consistent

**Source Control**:
- ✅ All changes committed to `upgrade-to-NET10` branch
- ✅ Commit message clear and descriptive
- ✅ No unrelated changes included in upgrade commit
- ✅ Master branch remains stable and unaffected

**Security-First Approach**:
- ✅ Security-vulnerable packages addressed first
- ✅ AspNetCore.Identity vulnerability resolved
- ✅ AspNetCore.Mvc vulnerability resolved
- ✅ Vulnerability scan confirms 0 issues

**Validation Complete**:
- ✅ All items in Testing & Validation Strategy checklist completed
- ✅ All project-specific validation checklists completed
- ✅ Build verification passed
- ✅ Package verification passed
- ✅ Security verification passed

#### Acceptance Criteria

**Before Merging to Master**:
- ✅ All Technical Criteria met (100%)
- ✅ All Quality Criteria met (100%)
- ✅ All Process Criteria met (100%)
- ✅ Security vulnerabilities = 0
- ✅ Build errors = 0
- ✅ Build warnings = 0
- ✅ Code review completed (if using PR workflow)
- ✅ Team notified of pending framework change

#### Optional Enhancement Criteria

**Package Optimization** (Nice-to-have):
- ⭕ AspNetCore.Identity assessed for necessity in class library
- ⭕ AspNetCore.Mvc assessed for necessity in class library
- ⭕ Unused packages removed (if any identified)
- ⭕ All packages updated to latest compatible versions

**Tooling Updates** (If applicable):
- ⭕ CI/CD pipeline updated to use .NET 10 SDK
- ⭕ Dockerfile updated to use .NET 10 runtime (if applicable)
- ⭕ Development environment documentation updated
- ⭕ NuGet package generation verified (Core.Common produces package)

#### Verification Commands

Run these commands to verify all success criteria met:

```bash
# Framework verification
grep -r "net9.0" *.csproj  # Should return 0 results
grep -r "net10.0" *.csproj  # Should return 3 results

# Build verification
dotnet clean
dotnet restore SilverCodeAPI.sln
dotnet build SilverCodeAPI.sln --no-restore
# Expected: Build succeeded. 0 Warning(s). 0 Error(s).

# Security verification (CRITICAL)
dotnet list package --vulnerable
# Expected: No vulnerable packages found

# Package version verification
dotnet list package
# Verify versions match plan specifications

# Package generation verification (if applicable)
dotnet pack Core.Common\Core.Common.csproj --configuration Release
# Expected: Successfully created package
```

#### Definition of Done

**The .NET 10 upgrade is DONE when**:

1. ✅ **All projects target .NET 10** - Verified by build success
2. ✅ **All recommended packages updated** - Verified by `dotnet list package`
3. ✅ **Zero security vulnerabilities** - Verified by `dotnet list package --vulnerable`
4. ✅ **Zero build errors** - Verified by `dotnet build`
5. ✅ **Zero build warnings** - Verified by `dotnet build`
6. ✅ **All tests pass** - N/A (no test projects identified)
7. ✅ **Changes committed** - Verified by `git log`
8. ✅ **Team notified** - Manual confirmation

At this point, the upgrade branch is ready to merge to master and the solution is successfully upgraded to .NET 10 (LTS).

---

## Summary

This plan provides a comprehensive roadmap for upgrading the SilverCodeAPI solution from .NET 9 to .NET 10 using an All-At-Once strategy. The key focus areas are:

1. **Security-First**: Resolving critical vulnerabilities in AspNetCore packages
2. **Atomic Upgrade**: All projects updated simultaneously for efficiency
3. **Dependency-Aware**: Following dependency order for validation
4. **Zero-Tolerance**: Must achieve 0 errors, 0 warnings, 0 vulnerabilities

The upgrade is low-to-medium complexity with primary challenges around assessing and resolving severely outdated security-vulnerable packages. With proper execution following this plan, the upgrade should complete successfully in a single work session.
