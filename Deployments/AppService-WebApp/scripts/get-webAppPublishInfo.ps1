$webappName = "fta-aznamingtool"
$PublishFile = "c:\temp\aznamingToolPubFile.txt"

Get-AzWebApp -Name $webappName | Get-AzWebAppPublishingProfile -OutputFile $PublishFile | Out-Null