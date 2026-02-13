$ErrorActionPreference = "Continue"
cd "C:\Users\natha\RiderProjects\EasySave"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Exécution des tests unitaires..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

dotnet test EasySaveTest/EasySaveTest.csproj --logger "console;verbosity=normal" --nologo 2>&1 | Tee-Object -FilePath "test-output.txt"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Résumé des résultats:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$content = Get-Content "test-output.txt" -Raw

if ($content -match "Failed:\s+(\d+)") {
    $failed = $Matches[1]
    Write-Host "Tests échoués: $failed" -ForegroundColor Red
}

if ($content -match "Passed:\s+(\d+)") {
    $passed = $Matches[1]
    Write-Host "Tests réussis: $passed" -ForegroundColor Green
}

if ($content -match "Total:\s+(\d+)") {
    $total = $Matches[1]
    Write-Host "Total de tests: $total" -ForegroundColor Yellow
}

# Extract failed test names
$failedTests = $content | Select-String -Pattern "Failed\s+(.+?)\s+\[" -AllMatches
if ($failedTests.Matches.Count -gt 0) {
    Write-Host "`nTests qui ont échoué:" -ForegroundColor Red
    foreach ($match in $failedTests.Matches) {
        Write-Host "  - $($match.Groups[1].Value)" -ForegroundColor Red
    }
}

