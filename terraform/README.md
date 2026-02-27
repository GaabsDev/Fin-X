# FinX Azure Terraform Deployment

Este diretório contém a configuração Terraform para deploy do sistema FinX no Azure.

## 🏗️ Arquitetura Azure

### Recursos Criados
- **Azure Container Registry (ACR)** - Armazenamento das imagens Docker
- **Azure Container Instances (ACI)** - Execução da aplicação
- **Azure Cosmos DB (MongoDB API)** - Banco de dados NoSQL
- **Log Analytics Workspace** - Monitoramento e logs
- **Resource Group** - Agrupamento lógico dos recursos

### Componentes da Aplicação
- **finx-api** - API principal em ASP.NET Core
- **finx-scripts** - Console app para DESAFIO 3 (unificação de pacientes)

## 📋 Pré-requisitos

### Ferramentas Necessárias
```bash
# Terraform
curl -fsSL https://apt.releases.hashicorp.com/gpg | sudo apt-key add -
sudo apt-add-repository "deb [arch=amd64] https://apt.releases.hashicorp.com $(lsb_release -cs) main"
sudo apt-get update && sudo apt-get install terraform

# Azure CLI
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Docker (para build das imagens)
sudo apt-get install docker.io
```

### Configuração Azure
```bash
# Login no Azure
az login

# Listar subscriptions disponíveis
az account list --output table

# Definir subscription ativa (se necessário)
az account set --subscription "your-subscription-id"
```

## 🚀 Deploy Passo a Passo

### 1. Configuração Inicial
```bash
# Clonar o repositório
git clone <repo-url>
cd Fin-x/terraform

# Criar arquivo de variáveis
cp terraform.tfvars.example terraform.tfvars

# Editar configurações (OBRIGATÓRIO)
vim terraform.tfvars
```

### 2. Build e Upload das Imagens Docker
```bash
# Navegar para o diretório raiz do projeto
cd ..

# Inicializar terraform para obter ACR info
cd terraform
terraform init
terraform plan
terraform apply -target=azurerm_container_registry.finx_acr

# Obter informações do ACR
ACR_LOGIN_SERVER=$(terraform output -raw acr_login_server)
ACR_USERNAME=$(terraform output -raw acr_admin_username)  
ACR_PASSWORD=$(terraform output -raw acr_admin_password)

# Login no ACR
echo $ACR_PASSWORD | docker login $ACR_LOGIN_SERVER --username $ACR_USERNAME --password-stdin

# Build e push da imagem da API
cd ..
docker build -t $ACR_LOGIN_SERVER/finx-api:latest -f FinX.Api/Dockerfile .
docker push $ACR_LOGIN_SERVER/finx-api:latest

# Build e push da imagem dos scripts
docker build -t $ACR_LOGIN_SERVER/finx-scripts:latest -f FinX.Script/Dockerfile .
docker push $ACR_LOGIN_SERVER/finx-scripts:latest
```

### 3. Deploy Completo
```bash
cd terraform

# Inicializar Terraform
terraform init

# Validar configuração
terraform validate

# Planejar deployment
terraform plan

# Aplicar configuração
terraform apply
```

### 4. Verificação do Deploy
```bash
# Obter URLs da aplicação
terraform output deployment_summary

# Testar API
API_URL=$(terraform output -raw api_url)
curl $API_URL/swagger

# Testar autenticação
curl -X POST $API_URL/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}'
```

## 🔧 Comandos Úteis

### Gerenciamento da Infraestrutura
```bash
# Ver estado atual
terraform show

# Listar recursos
terraform state list

# Ver outputs
terraform output

# Destruir infraestrutura
terraform destroy
```

### Monitoramento
```bash
# Ver logs dos containers
az container logs --resource-group $(terraform output -raw resource_group_name) --name finx-dev-api-cg

# Status dos containers
az container show --resource-group $(terraform output -raw resource_group_name) --name finx-dev-api-cg
```

### Execução dos Scripts DESAFIO 3
```bash
# Executar script de unificação
az container start --resource-group $(terraform output -raw resource_group_name) --name finx-dev-scripts-cg

# Ver logs dos scripts
az container logs --resource-group $(terraform output -raw resource_group_name) --name finx-dev-scripts-cg
```

## 📊 Custos Estimados (Brazil South)

| Recurso | Especificação | Custo/Mês (USD) |
|---------|--------------|------------------|
| Container Registry | Basic | ~$5 |
| Container Instances | 1 CPU, 2GB RAM | ~$30-50 |
| Cosmos DB | 400 RU/s | ~$25 |
| Log Analytics | 5GB/mês | ~$10 |
| **Total Estimado** | | **~$70-90** |

## 🔒 Segurança

### Produção - Configurações Recomendadas
```hcl
# No arquivo terraform.tfvars
environment = "prod"

# Restringir IPs permitidos
allowed_ips = [
  "203.0.113.0",  # IP do escritório
  "198.51.100.0"  # IP do servidor CI/CD
]
```

### Secrets Management
- Senhas são gerenciadas como `sensitive` no Terraform
- Connection strings são passadas via environment variables seguras
- Usar Azure Key Vault para segredos em produção

## 🐛 Troubleshooting

### Problemas Comuns

**Erro: "Container image pull failed"**
```bash
# Verificar se as imagens foram enviadas para o ACR
az acr repository list --name <acr-name>

# Re-build e push das imagens
docker build -t <acr-server>/finx-api:latest -f FinX.Api/Dockerfile .
docker push <acr-server>/finx-api:latest
```

**Erro: "Cosmos DB connection failed"**
```bash
# Verificar firewall do Cosmos DB
az cosmosdb show --resource-group <rg-name> --name <cosmos-name>

# Atualizar regras de IP se necessário
terraform apply -var="allowed_ips=[\"0.0.0.0\"]"
```

## 📞 Suporte

Para questões sobre o deployment:
1. Verificar logs via `terraform output` e `az container logs`
2. Validar configurações no `terraform.tfvars`
3. Consultar documentação oficial do [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)