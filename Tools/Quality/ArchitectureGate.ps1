param(
    [string]$ProjectRoot = (Resolve-Path ".").Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function New-Finding {
    param(
        [string]$Rule,
        [string]$File,
        [int]$Line,
        [string]$Text
    )
    [PSCustomObject]@{
        Rule = $Rule
        File = $File
        Line = $Line
        Text = $Text.Trim()
    }
}

function Collect-Matches {
    param(
        [string]$RuleName,
        [string[]]$Files,
        [string[]]$Patterns
    )

    $results = @()
    foreach ($file in $Files) {
        foreach ($pattern in $Patterns) {
            $hits = Select-String -Path $file -Pattern $pattern
            foreach ($hit in $hits) {
                $results += New-Finding -Rule $RuleName -File $file -Line $hit.LineNumber -Text $hit.Line
            }
        }
    }
    return $results
}

function Resolve-RelativePath {
    param(
        [string]$FullPath,
        [string]$Root
    )
    return $FullPath.Replace($Root + "\", "").Replace("\", "/")
}

function Is-WhitelistedFinding {
    param(
        [object]$Finding,
        [object[]]$Whitelist
    )

    foreach ($entry in $Whitelist) {
        if ($entry.Rule -ne $Finding.Rule) {
            continue
        }

        if ($Finding.File -notmatch $entry.FileRegex) {
            continue
        }

        if ($Finding.Text -notmatch $entry.TextRegex) {
            continue
        }

        return $true
    }

    return $false
}

function Collect-FlowFallbackGuardFindings {
    param(
        [string[]]$Files
    )

    $findings = @()
    foreach ($file in $Files) {
        $lines = Get-Content -Path $file
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]
            if ($line -notmatch "return\s+(false|null)\s*;") {
                continue
            }

            $windowStart = [Math]::Max(0, $i - 3)
            $windowEnd = [Math]::Min($lines.Count - 1, $i)
            $guardMatched = $false
            for ($j = $windowStart; $j -le $windowEnd; $j++) {
                $guardLine = $lines[$j]
                if ($guardLine -match "string\.IsNullOrWhiteSpace\s*\(" -or
                    $guardLine -match "string\.IsNullOrEmpty\s*\(" -or
                    $guardLine -match "==\s*null" -or
                    $guardLine -match "!\s*.*\.IsValid\s*\(") {
                    $guardMatched = $true
                    break
                }
            }

            if (-not $guardMatched) {
                continue
            }

            $findings += New-Finding -Rule "FLOW_DEFENSIVE_FALLBACK_FORBIDDEN" -File $file -Line ($i + 1) -Text $line
        }
    }

    return $findings
}

$scriptFiles = Get-ChildItem -Path (Join-Path $ProjectRoot "Assets\Scripts") -Recurse -Filter "*.cs" |
    ForEach-Object { $_.FullName }

$flowCoreFiles = $scriptFiles | Where-Object { $_ -like "*\Assets\Scripts\Flow\*" }
$sceneLoadAllowedFiles = @(
    "Assets/Scripts/Flow/SceneTransitionService.cs"
)
$sceneLoadScanFiles = $scriptFiles | Where-Object {
    $relative = Resolve-RelativePath -FullPath $_ -Root $ProjectRoot
    -not ($sceneLoadAllowedFiles -contains $relative)
}

$allFindings = @()

$allFindings += Collect-Matches -RuleName "FIND_OBJECT_LOOKUP_FORBIDDEN" -Files $scriptFiles -Patterns @(
    "FindObjectOfType\s*\(",
    "FindObjectsOfType\s*\(",
    "FindAnyObjectByType\s*\(",
    "FindFirstObjectByType\s*\("
)

$allFindings += Collect-Matches -RuleName "SCENEMANAGER_LOAD_FORBIDDEN_OUTSIDE_ORCHESTRATOR" -Files $sceneLoadScanFiles -Patterns @(
    "SceneManager\.LoadScene\s*\(",
    "SceneManager\.LoadSceneAsync\s*\("
)

$allFindings += Collect-FlowFallbackGuardFindings -Files $flowCoreFiles

$rules = @(
    "SCENEMANAGER_LOAD_FORBIDDEN_OUTSIDE_ORCHESTRATOR",
    "FIND_OBJECT_LOOKUP_FORBIDDEN",
    "FLOW_DEFENSIVE_FALLBACK_FORBIDDEN"
)

$whitelist = @(
    [PSCustomObject]@{
        Rule = "FLOW_DEFENSIVE_FALLBACK_FORBIDDEN"
        FileRegex = "Assets\\Scripts\\Flow\\RuntimeBindingResolver\.cs$"
        TextRegex = "return\s+false\s*;"
    },
    [PSCustomObject]@{
        Rule = "FLOW_DEFENSIVE_FALLBACK_FORBIDDEN"
        FileRegex = "Assets\\Scripts\\Flow\\GlobalRuntimeBootstrap\.cs$"
        TextRegex = "return\s*;"
    }
)

Write-Output "ARCH_GUARD_REPORT"
Write-Output "ProjectRoot: $ProjectRoot"
Write-Output "Rules: $($rules.Count)"

$hasFailure = $false
foreach ($rule in $rules) {
    $ruleFindings = @(
        $allFindings |
            Where-Object { $_.Rule -eq $rule } |
            Where-Object { -not (Is-WhitelistedFinding -Finding $_ -Whitelist $whitelist) }
    )
    if ($ruleFindings.Count -eq 0) {
        Write-Output ""
        Write-Output "Rule: $rule"
        Write-Output "Status: PASS"
        Write-Output "Count: 0"
        continue
    }

    $hasFailure = $true
    Write-Output ""
    Write-Output "Rule: $rule"
    Write-Output "Status: FAIL"
    Write-Output "Count: $($ruleFindings.Count)"
    Write-Output "Findings:"
    foreach ($finding in $ruleFindings) {
        $relative = Resolve-RelativePath -FullPath $finding.File -Root $ProjectRoot
        Write-Output ("- {0}:{1} :: {2}" -f $relative, $finding.Line, $finding.Text)
    }
}

Write-Output ""
if ($hasFailure) {
    Write-Output "Summary: FAIL"
    exit 1
}

Write-Output "Summary: PASS"
exit 0
