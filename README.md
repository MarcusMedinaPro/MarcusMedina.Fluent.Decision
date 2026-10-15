# MarcusMedina.Fluent.Decision

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download)
[![NuGet](https://img.shields.io/nuget/v/MarcusMedina.Fluent.Decision.svg)](https://www.nuget.org/packages/MarcusMedina.Fluent.Decision/)

**Fluent API for structured decision analysis in C#**

I detta fall ville jag förenkla användandet av beslutsmatriser och flerkriterieanalys. Weighted scoring, kriterier, options — för alla gånger du behöver fatta ett välinformerat beslut i kod.

Model criteria, rank options, compare trade-offs, and compute recommendations with a readable builder-based API.

## Installation

```bash
dotnet add package MarcusMedina.Fluent.Decision
```

## Quick Start

```csharp
using MarcusMedina.Fluent.Decision.Enums;
using MarcusMedina.Fluent.Decision.Extensions;

var decision = Decision.Create("Choose hosting platform")
    .WithCriterion("Performance", 0.4, NormalizationType.Linear)
    .WithCriterion("Cost", 0.3, NormalizationType.InverseLinear)
    .WithCriterion("Reliability", 0.3, NormalizationType.Linear)
    .WithOption("Option A")
        .WithScore("Performance", 8.5)
        .WithScore("Cost", 7.0)
        .WithScore("Reliability", 9.0)
    .WithOption("Option B")
        .WithScore("Performance", 8.0)
        .WithScore("Cost", 8.5)
        .WithScore("Reliability", 8.0)
    .BuildAndCompute();

var recommendation = decision.GetRecommendation();
Console.WriteLine($"{recommendation?.Name}: {recommendation?.TotalScore:F3}");
```

## Included Features

- Weighted multi-criteria scoring
- Normalization strategies for cost and benefit criteria
- Fluent builder API for options and criteria
- Option comparison and simple sensitivity analysis
- Demo application and test project under `csharp/`

## Development

```bash
cd csharp
dotnet build MarcusMedina.Fluent.Decision.slnx -c Release
```
