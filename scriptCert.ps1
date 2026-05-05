$certPath = "C:\Users\fbjda\.aspnet\https\condonet-dev-cert.pfx"   # Cambia esto por la ruta real
$certPassword = "C0nd0n37C3r7"              # Cambia esto por la contraseña real

Try {
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certPath, $certPassword)
    Write-Host "¡Contraseña correcta! El certificado se cargó exitosamente."
} Catch {
    Write-Host "Contraseña incorrecta o archivo inválido."
}