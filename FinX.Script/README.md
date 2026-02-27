# DESAFIO 3 - Scripts de Unificação de Pacientes Duplicados

## Problema

**Situação:** O Paciente está duplicado, deve ser mantido o paciente que foi cadastrado por último.

**Estrutura das Tabelas:**
- **Paciente:** Id, Nome, DataNascimento, Cpf, DataCadastro  
- **Hospital:** Id, Nome, Cnpj
- **GrupoPacienteHospital:** PacienteId, HospitalId, Codigo
- **Agendamento:** Id, HospitalId, PacienteId, Data

## Solução

**Regra de Negócio:** Manter o paciente com **DataCadastro** mais recente (último cadastrado).

**Processo de Unificação:**
1. Identificar pacientes duplicados por CPF
2. Manter o paciente com DataCadastro mais recente  
3. Atualizar todas as referências (grupos, agendamentos, registros médicos)
4. Remover pacientes duplicados
5. Limpar grupos duplicados para mesmo paciente-hospital

## Scripts C# Disponíveis

### 1. CreateTestData.cs
**Função:** Cria dados de teste com pacientes duplicados para demonstrar o processo.

**Cenários criados:**
- Maria Silva: 3 duplicatas (CPF: 12345678901)
- João Santos: 2 duplicatas (CPF: 98765432100)  
- Ana Costa: 4 duplicatas (CPF: 11122233344)
- Carlos Oliveira: sem duplicatas (controle)

### 2. UnifyDuplicatePatients.cs
**Função:** Script principal que executa a lógica de unificação.

**Funcionalidades:**
- Identifica CPFs duplicados via agregação MongoDB
- Mantém paciente com DataCadastro mais recente
- Atualiza referências em todas as collections relacionadas
- Gera relatório detalhado do processo

### 3. Program.cs
**Função:** Programa console para executar os scripts.

**Comandos disponíveis:**
- `create-test-data`: Criar dados de teste
- `unify-duplicates`: Executar unificação  
- `full-demo`: Demonstração completa
- `help`: Ajuda

## Como Executar

### Pré-requisitos
- .NET 10 SDK instalado
- MongoDB rodando (localhost:27017)
- Base de dados FinX configurada

### Comandos

```bash
# Navegar para a pasta FinX.Script
cd FinX.Script

# Restaurar dependências
dotnet restore

# Executar comandos:

# 1. Criar dados de teste com duplicatas
dotnet run create-test-data

# 2. Unificar pacientes duplicados
dotnet run unify-duplicates

# 3. Demonstração completa (criar dados + unificar)
dotnet run full-demo

# 4. Ajuda
dotnet run help
```

### Configuração

Editar `appsettings.json` para configurar conexão MongoDB:

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "Database": "finxdb"
  }
}
```

## Exemplo de Execução

```bash
$ dotnet run full-demo

=== DESAFIO 3: Scripts de Unificação de Pacientes Duplicados ===
Comando executado: full-demo
Data/Hora: 2026-02-26 15:30:00

🚀 Executando: Demonstração completa do DESAFIO 3

Etapa 1/2: Criando dados de teste...
🎯 Executando: Criação de dados de teste
Limpando dados existentes...
Criados 3 hospitais
Criados 10 pacientes (incluindo duplicatas)
  - Maria Silva: 3 duplicatas (CPF: 12345678901)
  - João Santos: 2 duplicatas (CPF: 98765432100)
  - Ana Costa: 4 duplicatas (CPF: 11122233344)
  - Carlos Oliveira: sem duplicatas
✅ Dados de teste criados:
   - Hospitais: 3
   - Pacientes (c/ duplicatas): 10
   - Vínculos: 8
   - Agendamentos: 6

Etapa 2/2: Unificando pacientes duplicados...
🔄 Executando: Unificação de pacientes duplicados
[DESAFIO 3] Iniciando processo de unificação de pacientes duplicados
Encontrados 3 CPFs com duplicatas
Processando 3 pacientes duplicados para CPF 12345678901
Mantendo paciente: abc-123 (Maria Santos Silva) - Cadastro: 2026-01-27
Removendo duplicata: abc-124 (Maria da Silva) - Cadastro: 2026-02-01
Referências atualizadas para paciente abc-124 -> abc-123: Grupos: 1, Agendamentos: 1, Registros Médicos: 0
...

=== RELATÓRIO FINAL DE UNIFICAÇÃO ===
CPFs processados: 3
Pacientes removidos: 6
Grupos atualizados: 3
Agendamentos atualizados: 2
Registros médicos atualizados: 0
✅ Nenhuma duplicata restante encontrada - Unificação bem-sucedida!
========================================

🎉 Demonstração completa finalizada com sucesso!
💡 Regra aplicada: Mantido o paciente com DataCadastro mais recente
💡 Todas as referências foram atualizadas para o paciente mantido

=== Execução finalizada ===
```

## Arquitetura dos Scripts

### Princípios Aplicados
- **Single Responsibility:** Cada script tem uma função específica
- **Dependency Injection:** Uso do DI container do .NET
- **Logging estruturado:** Logs detalhados para auditoria
- **Tratamento de erros:** Captura e log de exceções
- **Transações implícitas:** MongoDB garante atomicidade por operação

### Estrutura do Código
```
scripts/
├── Program.cs                 # Ponto de entrada e configuração
├── CreateTestData.cs         # Script para criar dados de teste
├── UnifyDuplicatePatients.cs # Script principal de unificação
├── scripts.csproj           # Projeto .NET
├── appsettings.json         # Configurações
└── README.md               # Esta documentação
```

## Validação dos Resultados

Após executar a unificação, verificar:

1. **Pacientes únicos por CPF:** Não deve haver CPFs duplicados
2. **Referências atualizadas:** Todos os grupos, agendamentos e registros médicos devem referenciar o paciente mantido
3. **Integridade dos dados:** Nenhuma referência órfã deve existir
4. **Logs de auditoria:** Processo deve ser totalmente rastreável

## Logs e Monitoramento

Os scripts geram logs detalhados incluindo:
- Pacientes identificados para unificação
- Decisão de qual paciente manter (DataCadastro mais recente)
- Quantidade de referências atualizadas por tipo
- Erros encontrados durante o processo
- Relatório final com estatísticas

## Solução Técnica

**Tecnologias utilizadas:**
- **C# .NET 10:** Linguagem e framework
- **MongoDB.Driver:** Acesso ao banco de dados
- **Microsoft.Extensions.Hosting:** Host para aplicação console
- **Microsoft.Extensions.Logging:** Sistema de logs
- **Agregação MongoDB:** Para identificar duplicatas eficientemente

**Vantagens da solução:**
- Performance otimizada com agregações MongoDB nativas
- Logs estruturados para auditoria completa
- Tratamento robusto de erros
- Facilmente extensível para novos tipos de referências
- Interface de linha de comando intuitiva