#########################################################
# IMPORTANT
#
# Deployment to Staging and to Prod are done using 2 scripts (deploy-staging.ps1 and deploy-prod.ps1)
# This relies on env vars defined in a separate script
# It need only be executed once and then after each change by executed from a Terminal (Powershell) using the command 
# & "<full path to script file>"
#
# Also requires having the AWS tooling installed. This can be done with
# dotnet tool install -g Amazon.Lambda.Tools
#
#########################################################

$ErrorActionPreference = "Stop"

function Require-Env([string[]]$names) {
  $missing = $names | Where-Object {
    $value = [Environment]::GetEnvironmentVariable($_)
    [string]::IsNullOrWhiteSpace($value)
  }

  if ($missing.Count -gt 0) {
    throw "Missing environment variables: $($missing -join ', ')"
  }
}

# Map template parameter -> machine env var name
$commonMap = @{
    AuthExpireMinutes     = 'BDAC_COMMON_AuthExpireMinutes'   
    AuthSecretKey         = 'BDAC_COMMON_AuthSecretKey'        
    AWSRegion             = 'BDAC_COMMON_AWSRegion'            
    DeveloperName         = 'BDAC_COMMON_DeveloperName'        
    EmailAPIPrivateKey    = 'BDAC_COMMON_EmailAPIPrivateKey'   
    EmailAPIPublicKey     = 'BDAC_COMMON_EmailAPIPublicKey'    
    SwaDefaultHost        = 'BDAC_COMMON_SwaDefaultHost'       
    SwaRegion             = 'BDAC_COMMON_SwaRegion'            
    SyncfusionLicenseKey  = 'BDAC_COMMON_SyncfusionLicenseKey' 
    UseEmailAPI           = 'BDAC_COMMON_UseEmailAPI'          
}

# Map template parameter -> machine env var name
$envMap  = @{
    AWSAccessId				    = 'BDAC_STG_AWSAccessId'				
    AWSSecret                   = 'BDAC_STG_AWSSecret'                 
    BackupBucket                = 'BDAC_STG_BackupBucket'              
    CORSOrigins                 = 'BDAC_STG_CORSOrigins'               
    DocumentBucket              = 'BDAC_STG_DocumentBucket'            
    FallbackEmailFromAddress    = 'BDAC_STG_FallbackEmailFromAddress'  
    FallbackEmailFromName       = 'BDAC_STG_FallbackEmailFromName'     
    FallbackEmailHost           = 'BDAC_STG_FallbackEmailHost'         
    FallbackEmailPassword       = 'BDAC_STG_FallbackEmailPassword'     
    FallbackEmailPort           = 'BDAC_STG_FallbackEmailPort'         
    FallbackEmailUsername       = 'BDAC_STG_FallbackEmailUsername'     
    LogLevel                    = 'BDAC_STG_LogLevel'     
    PrimaryEmailBCC             = 'BDAC_STG_PrimaryEmailBCC'           
    PrimaryEmailFromAddress     = 'BDAC_STG_PrimaryEmailFromAddress'   
    PrimaryEmailFromName        = 'BDAC_STG_PrimaryEmailFromName'      
    PrimaryEmailHost            = 'BDAC_STG_PrimaryEmailHost'          
    PrimaryEmailPassword        = 'BDAC_STG_PrimaryEmailPassword'      
    PrimaryEmailPort            = 'BDAC_STG_PrimaryEmailPort'
    PrimaryEmailRepairUrl       = 'BDAC_STG_PrimaryEmailRepairUrl'	
    PrimaryEmailUsername        = 'BDAC_STG_PrimaryEmailUsername'      
    SimpleDbDomain              = 'BDAC_STG_SimpleDbDomain'            
    SiteUrl                     = 'BDAC_STG_SiteUrl'
	StripeApiKey                = 'BDAC_STG_StripeApiKey'
    StripeWebHookEndpointSecret = 'BDAC_STG_StripeWebHookEndpointSecret'	
    TmpFilesBucket              = 'BDAC_STG_TmpFilesBucket'            
}

# Merge maps (env-specific wins if same key)
$map = $commonMap + $envMap

Require-Env ($map.Values)

"All required env vars are present."

$mappedParams = (
  $map.GetEnumerator() |
  Sort-Object Name |
  ForEach-Object {
    $paramName = $_.Name
    $envName   = $_.Value
    $value     = [Environment]::GetEnvironmentVariable($envName)

    "$paramName=$value"
  }
) -join ';'

# Add Stage parameter
$templateParams = "Stage=staging;$mappedParams"

$artifactBucket = "boroughbridgeanglingclubwebservicesbucket"
$artifactPrefix = "sam/staging/"

# Deploy the serverless application
dotnet lambda deploy-serverless `
  --profile "Boroughbridge Angling Club" `
  --region "eu-west-1" `
  --template serverless.template `
  --stack-name "BoroughbridgeAnglingClubWebServices-staging" `
  --template-parameters $templateParams `
  --s3-bucket $artifactBucket `
  --s3-prefix $artifactPrefix
