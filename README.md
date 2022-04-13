# weather-info-stack

This project showcases fetching Tallinn weather metrics, scraping these metrics with prometheus and visualizing data with grafana graphs. Terraform scripts to provision required infrastructure in Azure is also provided. For Fetching metrics, .NET Core docker image is used, the deployment script copies source to Azure VM and build a local image there. Docker-compose is used to deploy Prometheus, Grafana, WeatherApp stack on Azure VM. Prometheus and Grafana also have configurations predefined. Grafana also has dashboards and Prometheus datasource defined.

## How to run

Define following Azure variables in variables.tf, or provide these values when asked:
* subscription_id
* client_id
* client_secret
* tenant_id

Run following commands in terraform folder:
1. 'terraform init'
2. 'terraform apply -auto-approve'
3. After you receive 'host for provisioner cannot be empty' error, run 'terraform apply -auto-approve' again
4. Use ip address from terraform output to access Grafana
5. For admin access, use user 'admin' and password 'admin'
6. Dashboards are available at http://{ip}:3000/d/VZ7-RdUnz/weather-information?orgId=1
