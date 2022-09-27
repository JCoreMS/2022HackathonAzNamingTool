
<#
This is NOT used in teh deployment but a MANUAL option for enabling Authentication on your created Web App.
- Update the 2 required variables below.
#>

$appName = "fta-aznamingtool"
$mgUsrId = "372ffadf-aa0c-4f67-8e79-d635a044f0b7"  #ID of the Managed User Identity created during the deployment
$Environment = "AzureCloud"

$appHomePageUrl = "https://$appname.azurewebsites.net/"

##################  CONFIGURE APP REGISTRATION  ############################

#Connect-AzureAD

## Assign a role to a user or service principal with resource scope
# Get the user and role definition you want to link
$user = Get-AzureADServicePrincipal -ObjectId $mgUsrID
$roleDefinition = Get-AzureADMSRoleDefinition -Filter "displayName eq 'Application Administrator'"

# Get app registration and construct resource scope for assignment.
$directoryScope = '/'

# Create a scoped role assignment
$roleAssignment = New-AzureADMSRoleAssignment -DirectoryScopeId $directoryScope -RoleDefinitionId $roleDefinition.Id -PrincipalId $mgUsrId

$Guid = New-Guid
$startDate = Get-Date

$PasswordCredential = New-Object -TypeName Microsoft.Open.AzureAD.Model.PasswordCredential
$PasswordCredential.StartDate = $startDate
$PasswordCredential.EndDate = $startDate.AddYears(1)
$PasswordCredential.KeyId = $Guid
$PasswordCredential.Value = ([System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes(($Guid))))


[string[]]$replyUrl = $appHomePageUrl + ".auth/login/aad/callback"

$reqAAD = New-Object -TypeName "Microsoft.Open.AzureAD.Model.RequiredResourceAccess"
$reqAAD.ResourceAppId = "00000002-0000-0000-c000-000000000000" #See above on how to find GUIDs 
$delPermission1 = New-Object -TypeName "Microsoft.Open.AzureAD.Model.ResourceAccess" -ArgumentList "311a71cc-e848-46a1-bdf8-97ff7156d8e6","Scope" #Sign you in and read your profile
$reqAAD.ResourceAccess = $delPermission1


$appReg = New-AzureADApplication -DisplayName $appName -Homepage $appHomePageUrl -ReplyUrls $replyUrl -PasswordCredential $PasswordCredential -RequiredResourceAccess $reqAAD
$IdentifierUri = "api://" + $appReg.AppId
Set-AzureADApplication -ObjectId $appReg.ObjectId -IdentifierUris $IdentifierUri

#########  CONFIGURE WEB APP TO USE AUTH  #######################

#Small inconsistency for US gov in current AzureRm module
$loginBaseUrl = $(Get-AzEnvironment -Name $Environment).ActiveDirectoryAuthority
if ($loginBaseUrl -eq "https://login-us.microsoftonline.com/") {
    $loginBaseUrl = "https://login.microsoftonline.us/"
}

$issuerUrl = $loginBaseUrl +  $aadConnection.Tenant.Id.Guid + "/"

$WebApp = Get-AzWebApp -Name $appName
$ResourceGroupName = ($WebApp.id -split '/')[4]
$authResourceName = $WebAppName + "/authsettings"
$auth = Invoke-AzResourceAction -ResourceGroupName $ResourceGroupName -ResourceType Microsoft.Web/sites/config -ResourceName $authResourceName -Action list -ApiVersion 2016-08-01 -Force

$auth.properties.enabled = "True"
$auth.properties.unauthenticatedClientAction = "RedirectToLoginPage"
$auth.properties.tokenStoreEnabled = "True"
$auth.properties.defaultProvider = "AzureActiveDirectory"
$auth.properties.isAadAutoProvisioned = "False"
$auth.properties.clientId = $appReg.AppId
$auth.properties.clientSecret = $PasswordCredential.Value
$auth.properties.issuer = $IssuerUrl

New-AzResource -PropertyObject $auth.properties -ResourceGroupName $ResourceGroupName -ResourceType Microsoft.Web/sites/config -ResourceName $authResourceName -ApiVersion 2016-08-01 -Force

$UpdateWebAppConfig = @{

}

