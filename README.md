# Fin-X API — Sistema de Gestão de Pacientes

## 📋 Sobre o Projeto

**Fin-X API** é uma solução robusta para gestão de pacientes e histórico médico desenvolvida com **.NET 10**. O sistema foi projetado seguindo rigorosamente os **princípios SOLID**, práticas de **Clean Code** e implementa as melhores práticas de **segurança empresarial**.

### 🎯 Objetivos da Solução
- **Gestão Completa**: CRUD de pacientes com histórico médico
- **Integração Externa**: Consulta de exames via APIs externas
- **Segurança Robusta**: Autenticação JWT RSA-256 com rotação de chaves
- **Escalabilidade**: Arquitetura preparada para alta demanda
- **Manutenibilidade**: Código limpo e bem documentado
- **Deploy Flexível**: Suporte local, Docker e Azure

## 🚀 Stack Tecnológica

- **.NET 10** - Framework principal
- **ASP.NET Core Web API** - API REST
- **MongoDB** - Banco de dados NoSQL
- **JWT (RSA-256)** - Autenticação e autorização
- **Docker** - Containerização (via docker-compose)
- **xUnit** - Framework de testes
- **Swagger/OpenAPI** - Documentação da API

## 🏗️ Arquitetura e Decisões Técnicas

### 🎯 Princípios e Padrões Implementados

#### **SOLID Principles (Detalhamento)**

**S - Single Responsibility Principle**
- `AuthService`: Apenas autenticação JWT
- `PatientService`: Apenas operações de pacientes
- `KeyService`: Apenas gerenciamento de chaves RSA

**O - Open/Closed Principle**
- `IPatientService`: Interface extensível para novos providers
- `IAuthService`: Abstração permite diferentes métodos de auth

**L - Liskov Substitution**
- `MongoPatientService` substituível por qualquer `IPatientService`
- `FakePatientService` usado em testes sem quebrar contratos

**I - Interface Segregation**
- `IKeyService`: Apenas operações de chaves
- `IPatientService`: Apenas operações de pacientes
- Interfaces pequenas e específicas

**D - Dependency Inversion**
- Controllers dependem de interfaces, não implementações
- Dependency Injection nativo do .NET
- Fácil substituição para testes e mocks

#### **Clean Code Practices**
- ✅ **Naming**: Métodos e variáveis autoexplicativos
- ✅ **Functions**: Máximo 20 linhas, responsabilidade única
- ✅ **Comments**: Apenas quando necessário, código se documenta
- ✅ **Error Handling**: Try-catch específicos, não genéricos
- ✅ **DRY**: Zero duplicação de lógica
- ✅ **YAGNI**: Implementado apenas o necessário

### 🏗️ Decisões Arquiteturais Detalhadas

#### **1. MongoDB vs PostgreSQL/SQL Server**
**Decisão**: MongoDB
**Justificativa**:
- ✅ **Flexibilidade de Schema**: Dados médicos variam significativamente entre especialidades
- ✅ **Performance**: Queries JSON nativas sem JOINs complexos
- ✅ **Escalabilidade Horizontal**: Sharding nativo para crescimento
- ✅ **Cloud-Ready**: Azure Cosmos DB com API MongoDB
- ❌ **Trade-off**: Menor consistência ACID comparado ao SQL

#### **2. JWT RSA-256 vs HMAC-SHA256**
**Decisão**: RSA-256 Assimétrico
**Justificativa**:
- ✅ **Segurança Superior**: Chaves públicas/privadas separadas
- ✅ **Escalabilidade**: Microserviços podem validar sem chave secreta
- ✅ **Rotação de Chaves**: Sistema automático de renovação
- ✅ **Auditoria**: Melhor rastreabilidade de emissão/validação
- ❌ **Trade-off**: Ligeiramente mais lento que HMAC

#### **3. .NET 10 vs Node.js/Python**
**Decisão**: .NET 10
**Justificativa**:
- ✅ **Performance**: Runtime otimizado e AOT compilation
- ✅ **Type Safety**: Sistema de tipos forte previne erros
- ✅ **Ecossistema**: NuGet packages maduros para healthcare
- ✅ **Azure Integration**: Suporte nativo e otimizado
- ✅ **Enterprise**: Ideal para aplicações críticas

#### **4. Clean Architecture vs N-Tier**
**Decisão**: Clean Architecture com Repository Pattern
**Justificativa**:
- ✅ **Testabilidade**: Dependency injection facilita mocking
- ✅ **Manutenibilidade**: Separação clara de responsabilidades
- ✅ **Flexibilidade**: Troca de componentes sem impacto
- ✅ **SOLID**: Implementação natural dos princípios

## 📂 Estrutura do Projeto

```
FinX.Api/          # API principal
├── Controllers/   # Controladores REST
├── Models/        # Modelos de domínio
├── Services/      # Lógica de negócio
└── Data/         # Configuração MongoDB

FinX.Tests/        # Testes unitários
├── Fakes/        # Implementações mock
└── *Tests.cs     # Testes por controller/service

FinX.Script/       # Scripts unificação (DESAFIO 3)
├── Program.cs                    # Console app com DI e logging
├── UnifyDuplicatePatients.cs     # Lógica principal unificação
├── CreateTestData.cs             # Geração dados de teste
└── appsettings.json             # Config MongoDB

terraform/         # Infrastructure as Code (Azure)
├── main.tf       # Recursos principais
├── cosmos.tf     # Azure Cosmos DB
├── container-instances.tf        # ACI deployment
└── variables.tf  # Configurações parametrizáveis

FinX.Docs/         # Documentação adicional
```

## 🔧 Como Executar

### Pré-requisitos
- .NET 10 SDK
- Docker & Docker Compose
- Azure CLI e Terraform (para deploy em nuvem)

### Opção 1: Docker Compose (Recomendado)
```bash
# Executar aplicação completa
docker-compose up -d

# Apenas API e MongoDB
docker-compose up -d mongo finx-api

# Incluir scripts DESAFIO 3
docker-compose --profile scripts up -d
```

### Opção 2: Local Development
```bash
# Restaurar dependências
dotnet restore

# Executar API
dotnet run --project FinX.Api

# API estará disponível em http://localhost:5006
# Swagger UI estará disponível em http://localhost:5006/swagger
```

### Opção 3: Deploy Azure
```bash
# Configurar e executar deploy
cd terraform
cp terraform.tfvars.example terraform.tfvars
# Editar terraform.tfvars com suas configurações
terraform init && terraform apply
```

### Executar Scripts DESAFIO 3
```bash
# Local
cd FinX.Script && dotnet run help

# Docker
docker-compose run finx-scripts dotnet scripts.dll full-demo
```

### Executar Testes
```bash
dotnet test
```

## 🎯 DESAFIO 3: Unificação de Pacientes Duplicados

### 📋 Problema Resolvido
O sistema identifica e unifica pacientes duplicados com base no **CPF**, mantendo a integridade referencial de todos os dados relacionados.

### 🔧 Solução Implementada

#### **Algoritmo de Unificação**
```csharp
1. Identificar duplicatas via aggregation pipeline MongoDB
2. Para cada CPF duplicado:
   - Manter paciente com DataCadastro mais recente
   - Atualizar referências em todas as coleções
   - Remover pacientes duplicados após validação
```

#### **Componentes da Solução**
- **UnifyDuplicatePatients.cs**: Script principal com lógica de unificação
- **CreateTestData.cs**: Gerador de cenários de teste com duplicatas
- **Program.cs**: Interface CLI com comandos estruturados

#### **Recursos Técnicos**
- ✅ **MongoDB Aggregation**: Detecção eficiente de duplicatas
- ✅ **Transações Atômicas**: Garante consistência dos dados
- ✅ **Logging Detalhado**: Rastreamento completo das operações
- ✅ **Rollback Automático**: Reversão em caso de falhas

#### **Comandos Disponíveis**
```bash
cd FinX.Script

# Criar dados de teste com duplicatas
dotnet run create-test-data

# Executar unificação
dotnet run unify-duplicates  

# Demonstração completa
dotnet run full-demo

# Ajuda
dotnet run help
```

#### **Regras de Negócio Implementadas**
1. **Critério de Unificação**: CPF idêntico
2. **Prioridade**: Paciente com DataCadastro mais recente prevalece
3. **Integridade**: Atualização de todas as referências (grupos, agendamentos, registros médicos)
4. **Auditoria**: Log detalhado de todas as operações
5. **Segurança**: Validação antes da remoção definitiva

### 📊 Resultados Esperados
```
CPFs processados: 3
Pacientes removidos: 6  
Grupos atualizados: 6
Agendamentos atualizados: 4
✅ Nenhuma duplicata restante - Unificação bem-sucedida!
```

### Swagger/OpenAPI
A documentação interativa da API está disponível através do Swagger UI:
- **URL**: http://localhost:5006/swagger
- **JSON Schema**: http://localhost:5006/swagger/v1/swagger.json
- **Funcionalidades**: 
  - Exploração interativa dos endpoints
  - Teste direto das APIs com autenticação JWT
  - Esquemas de dados detalhados

### Postman Collection
Arquivo `postman_collection.json` incluído com:
- Todos os endpoints configurados
- Autenticação JWT automatizada
- Variáveis de ambiente pré-configuradas

## 📋 API Endpoints

### Autenticação
- `POST /api/auth/login` - Gerar JWT token

### Pacientes  
- `GET /api/patients` - Listar pacientes
- `GET /api/patients/{id}` - Buscar paciente por ID
- `POST /api/patients` - Criar paciente
- `PUT /api/patients/{id}` - Atualizar paciente
- `DELETE /api/patients/{id}` - Remover paciente

### Histórico Médico
- `GET /api/patients/{id}/records` - Histórico do paciente
- `POST /api/patients/{id}/records` - Adicionar registro médico

### Exames Externos
- `GET /api/externalexams/{cpf}` - Consultar exames (mock determinístico)

## 🔐 Autenticação

### Credenciais Demo
- **Usuário**: `admin`
- **Senha**: `password`

### Exemplo de Uso
```bash
# 1. Obter token
curl -X POST https://localhost:7001/api/auth/login \\
  -H "Content-Type: application/json" \\
  -d '{"username":"admin","password":"password"}'

# 2. Usar token nas requisições
curl -H "Authorization: Bearer <token>" \\
  https://localhost:7001/api/patients
```

## 🏥 Modelos de Dados

### Patient
```json
{
  "id": "guid",
  "name": "string",
  "cpf": "string",
  "dateOfBirth": "date",
  "contact": "string"
}
```

### MedicalRecord  
```json
{
  "id": "guid",
  "patientId": "guid",
  "diagnosis": "string",
  "prescription": "string",
  "examNotes": "string",
  "date": "datetime"
}
```

## 🛡️ Segurança

### Implementações de Segurança
- **JWT RSA-256**: Assinatura assimétrica com RSA 3072-bit
- **Chaves Rotacionáveis**: Sistema de geração/renovação de chaves RSA
- **Autorização por Endpoint**: Proteção via `[Authorize]`
- **Validação de Token**: Configuração robusta de validação JWT

### Para Produção
- ✅ Configurar HTTPS obrigatório
- ✅ Implementar rate limiting
- ✅ Adicionar logs de auditoria
- ✅ Usar Azure Key Vault ou AWS Secrets
- ✅ Implementar refresh tokens

## ⚡ Performance e Escalabilidade

### Otimizações Implementadas
- **Async/Await**: Operações assíncronas em toda API
- **Dependency Injection**: Componentes singleton quando apropriado
- **Índices MongoDB**: Índices automáticos por ID

### Para Alta Carga
- **Caching**: Implementar Redis para dados frequentes
- **Database**: Réplicas de leitura MongoDB
- **API Gateway**: Rate limiting e load balancing
- **Monitoramento**: Application Insights ou Prometheus

## 🧪 Testes

### Cobertura de Testes
- ✅ **Controllers**: Testes de integração
- ✅ **Services**: Testes unitários com mocks
- ✅ **Authentication**: Validação de tokens
- ✅ **Determinismo**: Testes consistentes e reproduzíveis

### Executar Testes Específicos
```bash
# Todos os testes
dotnet test

# Testes específicos
dotnet test --filter "ClassName=AuthControllerTests"
```

## � Containerização e Deploy

### Docker
A aplicação está completamente containerizada e pronta para deploy:

```bash
# Build das imagens
docker build -t finx-api -f FinX.Api/Dockerfile .
docker build -t finx-scripts -f FinX.Script/Dockerfile .

# Executar via Docker Compose
docker-compose up -d
```

### Componentes Docker
- **finx-api**: API principal ASP.NET Core
- **finx-scripts**: Console app DESAFIO 3  
- **finx-mongo**: MongoDB 7.0 com persistência

### Deploy Azure com Terraform
Deploy automatizado na Microsoft Azure:

```bash
cd terraform
terraform init && terraform apply
```

**Recursos Azure:**
- Azure Container Registry (ACR)
- Azure Container Instances (ACI)
- Azure Cosmos DB (MongoDB API)
- Log Analytics Workspace

**Custos estimados**: ~$70-90/mês

## �📦 Deploy e DevOps

### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
COPY bin/Release/net10.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "FinX.Api.dll"]
```

### Variáveis de Ambiente
```bash
ASPNETCORE_ENVIRONMENT=Production
MONGODB_CONNECTION=mongodb://localhost:27017
JWT_ISSUER=FinXApiProd
```

## 🔄 Roadmap Futuro

### Próximas Funcionalidades
- [ ] Refresh tokens para segurança aprimorada  
- [ ] Paginação para listas grandes
- [ ] Filtros avançados de busca
- [ ] Upload de arquivos médicos
- [ ] Notificações por email/SMS
- [ ] Dashboard administrativo

### Melhorias Técnicas
- [ ] CQRS para separação read/write
- [ ] Event Sourcing para auditoria
- [ ] GraphQL para queries flexíveis
- [ ] Microserviços por domínio

## Implementações

- **Arquitetura**: Decisões baseadas em DDD e Clean Architecture
- **Segurança**: JWT RSA com rotação de chaves
- **Performance**: MongoDB com índices otimizados
- **Testes**: Cobertura completa com mocks determinísticos

---

**Autor**: Gabriel Pires
**Versão**: MVP 1.0  
**Licença**: MIT
