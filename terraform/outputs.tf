output "public_ip" {
  description = "VM Public IP"
  value       = azurerm_public_ip.pip.ip_address
}