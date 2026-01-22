param(
    [Parameter(Mandatory = $true)] [string] $Bucket,
    [Parameter(Mandatory = $true)] [int] $OldPr,
    [Parameter(Mandatory = $true)] [int] $NewPr
)

# Safety guard (adjust as you already did)
if ($Bucket -notmatch "(?i)(-stg|stg)$") {
    throw "Refusing to update CORS: bucket does not appear to be a staging bucket."
}

$baseOrigin = "https://purple-stone-0ae0b6b03-{0}.westeurope.3.azurestaticapps.net"
$oldOrigin = [string]::Format($baseOrigin, $OldPr)
$newOrigin = [string]::Format($baseOrigin, $NewPr)

Write-Host "Updating S3 CORS for bucket '$Bucket'"
Write-Host "  Old origin: $oldOrigin"
Write-Host "  New origin: $newOrigin"
Write-Host ""

$tmpIn  = Join-Path $env:TEMP "cors-in.json"
$tmpOut = Join-Path $env:TEMP "cors-out.json"

# Get current CORS
aws s3api get-bucket-cors `
    --profile "Boroughbridge Angling Club" `
    --bucket $Bucket | Out-File -FilePath $tmpIn -Encoding utf8
if ($LASTEXITCODE -ne 0) { throw "aws get-bucket-cors failed with exit code $LASTEXITCODE" }

$json = Get-Content $tmpIn -Raw | ConvertFrom-Json

$updated = $false
foreach ($rule in $json.CORSRules) {
    for ($i = 0; $i -lt $rule.AllowedOrigins.Count; $i++) {
        if ($rule.AllowedOrigins[$i] -eq $oldOrigin) {
            $rule.AllowedOrigins[$i] = $newOrigin
            $updated = $true
        }
    }
}

if (-not $updated) {
    throw "Old preview origin not found in bucket CORS: $oldOrigin"
}

# Convert to JSON and write WITHOUT BOM (critical)
$outJson = $json | ConvertTo-Json -Depth 10
[System.IO.File]::WriteAllText($tmpOut, $outJson, (New-Object System.Text.UTF8Encoding($false)))

# Put updated CORS
aws s3api put-bucket-cors `
    --profile "Boroughbridge Angling Club" `
    --region "eu-west-1" `
    --bucket $Bucket `
    --cors-configuration file://$tmpOut
if ($LASTEXITCODE -ne 0) { throw "aws put-bucket-cors failed with exit code $LASTEXITCODE" }

# Verify
aws s3api get-bucket-cors `
    --profile "Boroughbridge Angling Club" `
    --bucket $Bucket
if ($LASTEXITCODE -ne 0) { throw "aws verify get-bucket-cors failed with exit code $LASTEXITCODE" }

Write-Host ""
Write-Host "CORS update complete."
