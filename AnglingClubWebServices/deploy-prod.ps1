#########################################################
# IMPORTANT
#
# Deployment to Staging and to Prod are done using 2 scripts (deploy-staging.ps1 and deploy-prod.ps1)
# This relies on env vars defined in a separate script
# It need only be executed once and then after each change by executed from a Terminal (Powerscript) using the command 
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
    AWSAccessId				    = 'BDAC_PROD_AWSAccessId'				
    AWSSecret                   = 'BDAC_PROD_AWSSecret'                 
    BackupBucket                = 'BDAC_PROD_BackupBucket'              
    CORSOrigins                 = 'BDAC_PROD_CORSOrigins'               
    DocumentBucket              = 'BDAC_PROD_DocumentBucket'            
    FallbackEmailFromAddress    = 'BDAC_PROD_FallbackEmailFromAddress'  
    FallbackEmailFromName       = 'BDAC_PROD_FallbackEmailFromName'     
    FallbackEmailHost           = 'BDAC_PROD_FallbackEmailHost'         
    FallbackEmailPassword       = 'BDAC_PROD_FallbackEmailPassword'     
    FallbackEmailPort           = 'BDAC_PROD_FallbackEmailPort'         
    FallbackEmailUsername       = 'BDAC_PROD_FallbackEmailUsername'     
    LogLevel                    = 'BDAC_PROD_LogLevel'     
    PrimaryEmailBCC             = 'BDAC_PROD_PrimaryEmailBCC'           
    PrimaryEmailFromAddress     = 'BDAC_PROD_PrimaryEmailFromAddress'   
    PrimaryEmailFromName        = 'BDAC_PROD_PrimaryEmailFromName'      
    PrimaryEmailHost            = 'BDAC_PROD_PrimaryEmailHost'          
    PrimaryEmailPassword        = 'BDAC_PROD_PrimaryEmailPassword'      
    PrimaryEmailPort            = 'BDAC_PROD_PrimaryEmailPort'
    PrimaryEmailRepairUrl       = 'BDAC_PROD_PrimaryEmailRepairUrl'	
    PrimaryEmailUsername        = 'BDAC_PROD_PrimaryEmailUsername'      
    SimpleDbDomain              = 'BDAC_PROD_SimpleDbDomain'            
    SiteUrl                     = 'BDAC_PROD_SiteUrl'
	StripeApiKey                = 'BDAC_PROD_StripeApiKey'
    StripeWebHookEndpointSecret = 'BDAC_PROD_StripeWebHookEndpointSecret'	
    TmpFilesBucket              = 'BDAC_PROD_TmpFilesBucket'            
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
$templateParams = "Stage=prod;$mappedParams"

# Deploy the serverless application
dotnet lambda deploy-serverless `
  --profile "Boroughbridge Angling Club" `
  --region "eu-west-1" `
  --template serverless.template `
  --stack-name "BoroughbridgeAnglingClubWebServices" `
  --template-parameters $templateParams