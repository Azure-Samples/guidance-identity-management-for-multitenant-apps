param(
[parameter(Mandatory=$true, ParameterSetName="SetupKeyVault")][string]$ResourceGroupName,
[parameter(Mandatory=$true, ParameterSetName="SetupKeyVault")][string]$Location,
[parameter(Mandatory=$true, ParameterSetName="SetupKeyVault")][string]$ApplicationId,
[parameter(Mandatory=$true)][string]$KeyVaultName,
[parameter(Mandatory=$false, ParameterSetName="SetValues")][Hashtable]$Values
)

function Set-KeyVaultSecret {
param(
[parameter(Mandatory=$true, ValueFromPipeline=$true)]$ConfigValue
)
    process {
        $secureString = ConvertTo-SecureString -String $ConfigValue.Value -AsPlainText -Force
        Set-AzureKeyVaultSecret -VaultName $KeyVaultName -Name $ConfigValue.Name -SecretValue $secureString
    }
}

Login-AzureRmAccount

switch($PSCmdlet.ParameterSetName)
{
    "SetupKeyVault"
    {
        New-AzureRmResourceGroup –Name $ResourceGroupName –Location $Location -ErrorAction Stop
        New-AzureRmKeyVault -VaultName $KeyVaultName -ResourceGroupName $ResourceGroupName -Location $Location -ErrorAction Stop
        Set-AzureRmKeyVaultAccessPolicy -VaultName $KeyVaultName -ServicePrincipalName $ApplicationId -ResourceGroupName $ResourceGroupName -PermissionsToSecrets get,list -ErrorAction Stop
    }
    "SetValues"
    {
        if ($Values -eq $null)
        {
            #For testing or inline config settings
            $Values = @{
                "Setting1"="Value1";
            }
        }
        
        $Values.GetEnumerator() | Set-KeyVaultSecret
    }
}
