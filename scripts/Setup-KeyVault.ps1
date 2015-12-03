param(
[parameter(Mandatory=$true, ParameterSetName="SetupKeyVault")][string]$ResourceGroupName,
[parameter(Mandatory=$true, ParameterSetName="SetupKeyVault")][string]$Location,
[parameter(Mandatory=$true, ParameterSetName="SetupKeyVault")][string]$ApplicationId,
[parameter(Mandatory=$true)][string]$KeyVaultName,
[parameter(Mandatory=$false, ParameterSetName="SetValues")][PSObject[]]$Values
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

switch($PSCmdlet.ParameterSetName)
{
    "SetupKeyVault"
    {
        Login-AzureRmAccount
        New-AzureRmResourceGroup –Name $ResourceGroupName –Location $Location -ErrorAction Stop
        New-AzureRmKeyVault -VaultName $KeyVaultName -ResourceGroupName $ResourceGroupName -Location $Location -ErrorAction Stop
        Set-AzureRmKeyVaultAccessPolicy -VaultName $KeyVaultName -ServicePrincipalName $ApplicationId -ResourceGroupName $ResourceGroupName -PermissionsToSecrets get,list -ErrorAction Stop
    }
    "SetValues"
    {
        if ($Values -eq $null)
        {
            #For testing or inline config settings
            $Values = @(
                [PSCustomObject]@{ Name="Setting1"; Value="Value1"; Tags=@{"ConfigKey"="ConfigurationKeyName"}};
            )
        }

        #Make sure our $Values parameter is valid
        $Values.GetEnumerator() | Validate-ValueParameter
        #Everything is good, so login to AzureRM
        Login-AzureRmAccount
        #Setup secrets
        $Values.GetEnumerator() | Set-KeyVaultSecret
    }
}
