# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [SistemaVenta.API\SistemaVenta.API.csproj](#sistemaventaapisistemaventaapicsproj)
  - [SistemaVenta.BLL\SistemaVenta.BLL.csproj](#sistemaventabllsistemaventabllcsproj)
  - [SistemaVenta.DAL\SistemaVenta.DAL.csproj](#sistemaventadalsistemaventadalcsproj)
  - [SistemaVenta.DTO\SistemaVenta.DTO.csproj](#sistemaventadtosistemaventadtocsproj)
  - [SistemaVenta.IOC\SistemaVenta.IOC.csproj](#sistemaventaiocsistemaventaioccsproj)
  - [SistemaVenta.Model\SistemaVenta.Model.csproj](#sistemaventamodelsistemaventamodelcsproj)
  - [SistemaVenta.Utility\SistemaVenta.Utility.csproj](#sistemaventautilitysistemaventautilitycsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 7 | All require upgrade |
| Total NuGet Packages | 6 | 3 need upgrade |
| Total Code Files | 51 |  |
| Total Code Files with Incidents | 8 |  |
| Total Lines of Code | 2325 |  |
| Total Number of Issues | 11 |  |
| Estimated LOC to modify | 1+ | at least 0,0% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| [SistemaVenta.API\SistemaVenta.API.csproj](#sistemaventaapisistemaventaapicsproj) | net7.0 | 🟢 Low | 1 | 0 |  | AspNetCore, Sdk Style = True |
| [SistemaVenta.BLL\SistemaVenta.BLL.csproj](#sistemaventabllsistemaventabllcsproj) | net7.0 | 🟢 Low | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [SistemaVenta.DAL\SistemaVenta.DAL.csproj](#sistemaventadalsistemaventadalcsproj) | net7.0 | 🟢 Low | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [SistemaVenta.DTO\SistemaVenta.DTO.csproj](#sistemaventadtosistemaventadtocsproj) | net7.0 | 🟢 Low | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [SistemaVenta.IOC\SistemaVenta.IOC.csproj](#sistemaventaiocsistemaventaioccsproj) | net7.0 | 🟢 Low | 0 | 1 | 1+ | ClassLibrary, Sdk Style = True |
| [SistemaVenta.Model\SistemaVenta.Model.csproj](#sistemaventamodelsistemaventamodelcsproj) | net7.0 | 🟢 Low | 2 | 0 |  | ClassLibrary, Sdk Style = True |
| [SistemaVenta.Utility\SistemaVenta.Utility.csproj](#sistemaventautilitysistemaventautilitycsproj) | net7.0 | 🟢 Low | 0 | 0 |  | ClassLibrary, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 3 | 50,0% |
| ⚠️ Incompatible | 0 | 0,0% |
| 🔄 Upgrade Recommended | 3 | 50,0% |
| ***Total NuGet Packages*** | ***6*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 1 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 2543 |  |
| ***Total APIs Analyzed*** | ***2544*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| AutoMapper | 12.0.0 |  | [SistemaVenta.Utility.csproj](#sistemaventautilitysistemaventautilitycsproj) | ✅Compatible |
| AutoMapper.Extensions.Microsoft.DependencyInjection | 12.0.0 |  | [SistemaVenta.Utility.csproj](#sistemaventautilitysistemaventautilitycsproj) | ✅Compatible |
| Microsoft.AspNetCore.OpenApi | 7.0.5 | 10.0.3 | [SistemaVenta.API.csproj](#sistemaventaapisistemaventaapicsproj) | Se recomienda actualizar el paquete NuGet |
| Microsoft.EntityFrameworkCore.SqlServer | 7.0.1 | 10.0.3 | [SistemaVenta.Model.csproj](#sistemaventamodelsistemaventamodelcsproj) | Se recomienda actualizar el paquete NuGet |
| Microsoft.EntityFrameworkCore.Tools | 7.0.1 | 10.0.3 | [SistemaVenta.Model.csproj](#sistemaventamodelsistemaventamodelcsproj) | Se recomienda actualizar el paquete NuGet |
| Swashbuckle.AspNetCore | 6.4.0 |  | [SistemaVenta.API.csproj](#sistemaventaapisistemaventaapicsproj) | ✅Compatible |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |
| T:Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions | 1 | 100,0% | Binary Incompatible |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;SistemaVenta.API.csproj</b><br/><small>net7.0</small>"]
    P2["<b>📦&nbsp;SistemaVenta.DAL.csproj</b><br/><small>net7.0</small>"]
    P3["<b>📦&nbsp;SistemaVenta.BLL.csproj</b><br/><small>net7.0</small>"]
    P4["<b>📦&nbsp;SistemaVenta.Model.csproj</b><br/><small>net7.0</small>"]
    P5["<b>📦&nbsp;SistemaVenta.IOC.csproj</b><br/><small>net7.0</small>"]
    P6["<b>📦&nbsp;SistemaVenta.DTO.csproj</b><br/><small>net7.0</small>"]
    P7["<b>📦&nbsp;SistemaVenta.Utility.csproj</b><br/><small>net7.0</small>"]
    P1 --> P5
    P1 --> P3
    P1 --> P6
    P2 --> P4
    P3 --> P2
    P3 --> P4
    P3 --> P7
    P3 --> P6
    P5 --> P2
    P5 --> P3
    P5 --> P7
    P7 --> P4
    P7 --> P6
    click P1 "#sistemaventaapisistemaventaapicsproj"
    click P2 "#sistemaventadalsistemaventadalcsproj"
    click P3 "#sistemaventabllsistemaventabllcsproj"
    click P4 "#sistemaventamodelsistemaventamodelcsproj"
    click P5 "#sistemaventaiocsistemaventaioccsproj"
    click P6 "#sistemaventadtosistemaventadtocsproj"
    click P7 "#sistemaventautilitysistemaventautilitycsproj"

```

## Project Details

<a id="sistemaventaapisistemaventaapicsproj"></a>
### SistemaVenta.API\SistemaVenta.API.csproj

#### Project Info

- **Current Target Framework:** net7.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** AspNetCore
- **Dependencies**: 3
- **Dependants**: 0
- **Number of Files**: 11
- **Number of Files with Incidents**: 1
- **Lines of Code**: 539
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["SistemaVenta.API.csproj"]
        MAIN["<b>📦&nbsp;SistemaVenta.API.csproj</b><br/><small>net7.0</small>"]
        click MAIN "#sistemaventaapisistemaventaapicsproj"
    end
    subgraph downstream["Dependencies (3"]
        P5["<b>📦&nbsp;SistemaVenta.IOC.csproj</b><br/><small>net7.0</small>"]
        P3["<b>📦&nbsp;SistemaVenta.BLL.csproj</b><br/><small>net7.0</small>"]
        P6["<b>📦&nbsp;SistemaVenta.DTO.csproj</b><br/><small>net7.0</small>"]
        click P5 "#sistemaventaiocsistemaventaioccsproj"
        click P3 "#sistemaventabllsistemaventabllcsproj"
        click P6 "#sistemaventadtosistemaventadtocsproj"
    end
    MAIN --> P5
    MAIN --> P3
    MAIN --> P6

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 389 |  |
| ***Total APIs Analyzed*** | ***389*** |  |

<a id="sistemaventabllsistemaventabllcsproj"></a>
### SistemaVenta.BLL\SistemaVenta.BLL.csproj

#### Project Info

- **Current Target Framework:** net7.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 4
- **Dependants**: 2
- **Number of Files**: 14
- **Number of Files with Incidents**: 1
- **Lines of Code**: 723
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P1["<b>📦&nbsp;SistemaVenta.API.csproj</b><br/><small>net7.0</small>"]
        P5["<b>📦&nbsp;SistemaVenta.IOC.csproj</b><br/><small>net7.0</small>"]
        click P1 "#sistemaventaapisistemaventaapicsproj"
        click P5 "#sistemaventaiocsistemaventaioccsproj"
    end
    subgraph current["SistemaVenta.BLL.csproj"]
        MAIN["<b>📦&nbsp;SistemaVenta.BLL.csproj</b><br/><small>net7.0</small>"]
        click MAIN "#sistemaventabllsistemaventabllcsproj"
    end
    subgraph downstream["Dependencies (4"]
        P2["<b>📦&nbsp;SistemaVenta.DAL.csproj</b><br/><small>net7.0</small>"]
        P4["<b>📦&nbsp;SistemaVenta.Model.csproj</b><br/><small>net7.0</small>"]
        P7["<b>📦&nbsp;SistemaVenta.Utility.csproj</b><br/><small>net7.0</small>"]
        P6["<b>📦&nbsp;SistemaVenta.DTO.csproj</b><br/><small>net7.0</small>"]
        click P2 "#sistemaventadalsistemaventadalcsproj"
        click P4 "#sistemaventamodelsistemaventamodelcsproj"
        click P7 "#sistemaventautilitysistemaventautilitycsproj"
        click P6 "#sistemaventadtosistemaventadtocsproj"
    end
    P1 --> MAIN
    P5 --> MAIN
    MAIN --> P2
    MAIN --> P4
    MAIN --> P7
    MAIN --> P6

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 502 |  |
| ***Total APIs Analyzed*** | ***502*** |  |

<a id="sistemaventadalsistemaventadalcsproj"></a>
### SistemaVenta.DAL\SistemaVenta.DAL.csproj

#### Project Info

- **Current Target Framework:** net7.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 2
- **Number of Files**: 5
- **Number of Files with Incidents**: 1
- **Lines of Code**: 448
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P3["<b>📦&nbsp;SistemaVenta.BLL.csproj</b><br/><small>net7.0</small>"]
        P5["<b>📦&nbsp;SistemaVenta.IOC.csproj</b><br/><small>net7.0</small>"]
        click P3 "#sistemaventabllsistemaventabllcsproj"
        click P5 "#sistemaventaiocsistemaventaioccsproj"
    end
    subgraph current["SistemaVenta.DAL.csproj"]
        MAIN["<b>📦&nbsp;SistemaVenta.DAL.csproj</b><br/><small>net7.0</small>"]
        click MAIN "#sistemaventadalsistemaventadalcsproj"
    end
    subgraph downstream["Dependencies (1"]
        P4["<b>📦&nbsp;SistemaVenta.Model.csproj</b><br/><small>net7.0</small>"]
        click P4 "#sistemaventamodelsistemaventamodelcsproj"
    end
    P3 --> MAIN
    P5 --> MAIN
    MAIN --> P4

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 684 |  |
| ***Total APIs Analyzed*** | ***684*** |  |

<a id="sistemaventadtosistemaventadtocsproj"></a>
### SistemaVenta.DTO\SistemaVenta.DTO.csproj

#### Project Info

- **Current Target Framework:** net7.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 0
- **Dependants**: 3
- **Number of Files**: 12
- **Number of Files with Incidents**: 1
- **Lines of Code**: 232
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (3)"]
        P1["<b>📦&nbsp;SistemaVenta.API.csproj</b><br/><small>net7.0</small>"]
        P3["<b>📦&nbsp;SistemaVenta.BLL.csproj</b><br/><small>net7.0</small>"]
        P7["<b>📦&nbsp;SistemaVenta.Utility.csproj</b><br/><small>net7.0</small>"]
        click P1 "#sistemaventaapisistemaventaapicsproj"
        click P3 "#sistemaventabllsistemaventabllcsproj"
        click P7 "#sistemaventautilitysistemaventautilitycsproj"
    end
    subgraph current["SistemaVenta.DTO.csproj"]
        MAIN["<b>📦&nbsp;SistemaVenta.DTO.csproj</b><br/><small>net7.0</small>"]
        click MAIN "#sistemaventadtosistemaventadtocsproj"
    end
    P1 --> MAIN
    P3 --> MAIN
    P7 --> MAIN

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 212 |  |
| ***Total APIs Analyzed*** | ***212*** |  |

<a id="sistemaventaiocsistemaventaioccsproj"></a>
### SistemaVenta.IOC\SistemaVenta.IOC.csproj

#### Project Info

- **Current Target Framework:** net7.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 3
- **Dependants**: 1
- **Number of Files**: 1
- **Number of Files with Incidents**: 2
- **Lines of Code**: 45
- **Estimated LOC to modify**: 1+ (at least 2,2% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (1)"]
        P1["<b>📦&nbsp;SistemaVenta.API.csproj</b><br/><small>net7.0</small>"]
        click P1 "#sistemaventaapisistemaventaapicsproj"
    end
    subgraph current["SistemaVenta.IOC.csproj"]
        MAIN["<b>📦&nbsp;SistemaVenta.IOC.csproj</b><br/><small>net7.0</small>"]
        click MAIN "#sistemaventaiocsistemaventaioccsproj"
    end
    subgraph downstream["Dependencies (3"]
        P2["<b>📦&nbsp;SistemaVenta.DAL.csproj</b><br/><small>net7.0</small>"]
        P3["<b>📦&nbsp;SistemaVenta.BLL.csproj</b><br/><small>net7.0</small>"]
        P7["<b>📦&nbsp;SistemaVenta.Utility.csproj</b><br/><small>net7.0</small>"]
        click P2 "#sistemaventadalsistemaventadalcsproj"
        click P3 "#sistemaventabllsistemaventabllcsproj"
        click P7 "#sistemaventautilitysistemaventautilitycsproj"
    end
    P1 --> MAIN
    MAIN --> P2
    MAIN --> P3
    MAIN --> P7

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 1 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 41 |  |
| ***Total APIs Analyzed*** | ***42*** |  |

<a id="sistemaventamodelsistemaventamodelcsproj"></a>
### SistemaVenta.Model\SistemaVenta.Model.csproj

#### Project Info

- **Current Target Framework:** net7.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 0
- **Dependants**: 3
- **Number of Files**: 9
- **Number of Files with Incidents**: 1
- **Lines of Code**: 171
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (3)"]
        P2["<b>📦&nbsp;SistemaVenta.DAL.csproj</b><br/><small>net7.0</small>"]
        P3["<b>📦&nbsp;SistemaVenta.BLL.csproj</b><br/><small>net7.0</small>"]
        P7["<b>📦&nbsp;SistemaVenta.Utility.csproj</b><br/><small>net7.0</small>"]
        click P2 "#sistemaventadalsistemaventadalcsproj"
        click P3 "#sistemaventabllsistemaventabllcsproj"
        click P7 "#sistemaventautilitysistemaventautilitycsproj"
    end
    subgraph current["SistemaVenta.Model.csproj"]
        MAIN["<b>📦&nbsp;SistemaVenta.Model.csproj</b><br/><small>net7.0</small>"]
        click MAIN "#sistemaventamodelsistemaventamodelcsproj"
    end
    P2 --> MAIN
    P3 --> MAIN
    P7 --> MAIN

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 186 |  |
| ***Total APIs Analyzed*** | ***186*** |  |

<a id="sistemaventautilitysistemaventautilitycsproj"></a>
### SistemaVenta.Utility\SistemaVenta.Utility.csproj

#### Project Info

- **Current Target Framework:** net7.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 2
- **Dependants**: 2
- **Number of Files**: 1
- **Number of Files with Incidents**: 1
- **Lines of Code**: 167
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P3["<b>📦&nbsp;SistemaVenta.BLL.csproj</b><br/><small>net7.0</small>"]
        P5["<b>📦&nbsp;SistemaVenta.IOC.csproj</b><br/><small>net7.0</small>"]
        click P3 "#sistemaventabllsistemaventabllcsproj"
        click P5 "#sistemaventaiocsistemaventaioccsproj"
    end
    subgraph current["SistemaVenta.Utility.csproj"]
        MAIN["<b>📦&nbsp;SistemaVenta.Utility.csproj</b><br/><small>net7.0</small>"]
        click MAIN "#sistemaventautilitysistemaventautilitycsproj"
    end
    subgraph downstream["Dependencies (2"]
        P4["<b>📦&nbsp;SistemaVenta.Model.csproj</b><br/><small>net7.0</small>"]
        P6["<b>📦&nbsp;SistemaVenta.DTO.csproj</b><br/><small>net7.0</small>"]
        click P4 "#sistemaventamodelsistemaventamodelcsproj"
        click P6 "#sistemaventadtosistemaventadtocsproj"
    end
    P3 --> MAIN
    P5 --> MAIN
    MAIN --> P4
    MAIN --> P6

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 529 |  |
| ***Total APIs Analyzed*** | ***529*** |  |

