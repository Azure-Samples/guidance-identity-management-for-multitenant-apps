param(
[parameter(Mandatory=$true, ParameterSetName="GenerateCertificate")]
[string]$Subject,

[parameter(Mandatory=$false, ParameterSetName="GenerateCertificate")]
[string]$CertStoreLocation = "Cert:\CurrentUser\My",

[parameter(Mandatory=$false, ParameterSetName="GenerateCertificate")]
[DateTime]$NotBefore = [DateTime]::Now.Date,

[parameter(Mandatory=$false, ParameterSetName="GenerateCertificate")]
[DateTime]$NotAfter = $NotBefore.AddYears(1),

[parameter(Mandatory=$false, ParameterSetName="GenerateCertificate")]
[ValidateScript({if ($_){ Test-Path $_ -PathType Container }})][string]$OutputPath,

[parameter(Mandatory=$true, ParameterSetName="CreateKeyVault")]
[string]$ResourceGroupName,

[parameter(Mandatory=$true, ParameterSetName="CreateKeyVault")]
[string]$Location,

[parameter(Mandatory=$true, ParameterSetName="SetAccessPolicy")]
[Guid[]]$ApplicationIds,

[parameter(Mandatory=$true, ParameterSetName="CreateKeyVault")]
[parameter(Mandatory=$true, ParameterSetName="SetConfigValue")]
[parameter(Mandatory=$true, ParameterSetName="SetAccessPolicy")]
[string]$KeyVaultName,

[parameter(Mandatory=$false, ParameterSetName="SetConfigValue")]
[string]$KeyName,

[parameter(Mandatory=$false, ParameterSetName="SetConfigValue")]
[string]$KeyValue,

[parameter(Mandatory=$false, ParameterSetName="SetConfigValue")]
[string]$ConfigName
)

function Validate-ValueParameter {
param(
[parameter(Mandatory=$true, ValueFromPipeline=$true)]$ConfigValue
)
    $isValid = $true
    $member = Get-Member -InputObject $ConfigValue -Name "Name"
    if ($member -eq $null)
    {
        throw "Configuration parameter has a missing 'Name' member"
    }

    $member = Get-Member -InputObject $ConfigValue -Name "Value"
    if ($member -eq $null)
    {
        throw "Configuration parameter has a missing 'Value' member"
    }

    $member = Get-Member -InputObject $ConfigValue -Name "Tags"
    if ($member -eq $null)
    {
        throw "Configuration parameter has a missing 'Tags' member"
    }
}

function Set-KeyVaultSecret {
param(
[parameter(Mandatory=$true, ValueFromPipeline=$true)]$ConfigValue
)
    process {
        $secureString = ConvertTo-SecureString -String $ConfigValue.Value -AsPlainText -Force
        Set-AzureKeyVaultSecret -VaultName $KeyVaultName -Name $ConfigValue.Name -SecretValue $secureString -Tags $ConfigValue.Tags
    }
}

function Set-AccessPolicy {
param(
[parameter(Mandatory=$true, ValueFromPipeline=$true)]$ApplicationId
)
    process {
        Set-AzureRmKeyVaultAccessPolicy -VaultName $KeyVaultName -ServicePrincipalName $ApplicationId -PermissionsToSecrets get,list -ErrorAction Stop
    }
}

switch($PSCmdlet.ParameterSetName)
{
    "GenerateCertificate"
    {
        # There is an existing bug with PowerShell where $PSScriptRoot is not set if used as a parameter's default value.  We'll work around
        # this here by checking to see if a value was provided for $OutputPath, and if not, use the default
        if ($PSBoundParameters.ContainsKey('OutputPath'))
        {
            $outputFilePath = Join-Path $OutputPath "$Subject.cer"
        }
        else
        {
            $OutputFilePath = "$PSScriptRoot\$Subject.cer"
        }

        $x509Cert = New-SelfSignedCertificate -NotBefore $NotBefore -NotAfter $NotAfter -Subject $Subject -CertStoreLocation $CertStoreLocation -Provider "Microsoft Strong Cryptographic Provider"

        # Export the public key portion of the certificate
        $cerFileInfo = Export-Certificate -Cert $x509Cert -FilePath $outputFilePath
        $publicKeyCert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
        $publicKeyCert.Import($outputFilePath)

        $rawCertData = [System.Convert]::ToBase64String($publicKeyCert.GetRawCertData())
        $thumbprint = [System.Convert]::ToBase64String($publicKeyCert.GetCertHash())
        $keyId = [System.Guid]::NewGuid().ToString()

        $jsonForManifest = "{ `"type`": `"AsymmetricX509Cert`", `"usage`": `"Verify`", `"keyId`": `"$keyId`", `"customKeyIdentifier`": `"$thumbprint`", `"value`" : `"$rawCertData`" }"
        $($jsonForManifest)
    }
    "CreateKeyVault"
    {
        Login-AzureRmAccount
        New-AzureRmResourceGroup –Name $ResourceGroupName –Location $Location -ErrorAction Stop
        New-AzureRmKeyVault -VaultName $KeyVaultName -ResourceGroupName $ResourceGroupName -Location $Location -ErrorAction Stop
    }
    "SetAccessPolicy"
    {
        Login-AzureRmAccount
        # Set the access policy for the ApplicationIds
        $ApplicationIds.GetEnumerator() | Set-AccessPolicy
    }
    "SetConfigValue"
    {
        $KeyValueObject = [PSCustomObject]@{ Name=$KeyName; Value=$KeyValue; Tags=@{"ConfigKey"=$ConfigName}}

        #Make sure our $KeyValueObject is valid
        Validate-ValueParameter $KeyValueObject
        #Everything is good, so login to AzureRM
        Login-AzureRmAccount
        #Setup secrets
        Set-KeyVaultSecret $KeyValueObject
    }
}
