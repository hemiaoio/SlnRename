# 设置输出格式
$OutputEncoding = [Text.UTF8Encoding]::UTF8

## 公共参数

# 替换前的公司名称
$oldCompanyName="MyCompanyName"
# 替换后的公司名称
$newCompanyName="Winston"

# 替换前的项目名称
$oldProjectName="AbpZeroTemplate"
# 替换后的项目名称
$newProjectName="WinERP"

# 文件类型名称
$fileType="FileInfo"

# 目录类型名称
$dirType="DirectoryInfo"

# sln所在目录
$slnFolder = (Get-Item -Path "./aspnet-core/" -Verbose).FullName
$angluarFolder = (Get-Item -Path "./angular/" -Verbose).FullName

# 需要修改文件内容的文件后缀名
$include=@("*.cs","*.cshtml","*.asax","*.ps1","*.ts","*.csproj","*.sln","*.xaml","*.json","*.js","*.xml","*.config","Dockerfile")

$elapsed = [System.Diagnostics.Stopwatch]::StartNew()

Write-Host '开始重命名文件夹'
# 重命名文件夹
Ls $slnFolder -Recurse | Where { $_.GetType().Name -eq $dirType -and ($_.Name.Contains($oldCompanyName) -or $_.Name.Contains($oldProjectName)) } | ForEach-Object{
	Write-Host 'directory ' $_.FullName
	$newDirectoryName=$_.Name.Replace($oldCompanyName,$newCompanyName).Replace($oldProjectName,$newProjectName)
	Rename-Item $_.FullName $newDirectoryName
}
Write-Host '结束重命名文件夹'
Write-Host '-------------------------------------------------------------'


# 替换文件中的内容和文件名
Write-Host '开始替换文件中的内容和文件名'
Ls $slnFolder -Include $include -Recurse | Where { $_.GetType().Name -eq $fileType} | ForEach-Object{
	$fileText = Get-Content $_ -Raw -Encoding UTF8
	if($fileText.Length -gt 0 -and ($fileText.contains($oldCompanyName) -or $fileText.contains($oldProjectName))){
		$fileText.Replace($oldCompanyName,$newCompanyName).Replace($oldProjectName,$newProjectName) | Set-Content $_ -Encoding UTF8
		Write-Host 'file(change text) ' $_.FullName
	}
	If($_.Name.contains($oldCompanyName) -or $_.Name.contains($oldProjectName)){
		$newFileName=$_.Name.Replace($oldCompanyName,$newCompanyName).Replace($oldProjectName,$newProjectName)
		Rename-Item $_.FullName $newFileName
		Write-Host 'file(change name) ' $_.FullName
	}
}
Write-Host '结束替换文件中的内容和文件名'
Write-Host '-------------------------------------------------------------'

$elapsed.stop()
write-host "共花费时间: $($elapsed.Elapsed.ToString())"
