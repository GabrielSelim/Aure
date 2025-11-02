$password = "GSYSTEM.955078a"
$commands = @"
cd /root/Aure
git pull
docker-compose down
docker-compose up -d --build
docker ps
"@

$processInfo = New-Object System.Diagnostics.ProcessStartInfo
$processInfo.FileName = "ssh"
$processInfo.Arguments = "root@5.189.174.61"
$processInfo.RedirectStandardInput = $true
$processInfo.RedirectStandardOutput = $true
$processInfo.RedirectStandardError = $true
$processInfo.UseShellExecute = $false

$process = New-Object System.Diagnostics.Process
$process.StartInfo = $processInfo
$process.Start() | Out-Null

$process.StandardInput.WriteLine($password)
$process.StandardInput.WriteLine($commands)
$process.StandardInput.WriteLine("exit")
$process.StandardInput.Close()

$output = $process.StandardOutput.ReadToEnd()
$error = $process.StandardError.ReadToEnd()
$process.WaitForExit()

Write-Host "Output:"
Write-Host $output
Write-Host "Error:"
Write-Host $error
