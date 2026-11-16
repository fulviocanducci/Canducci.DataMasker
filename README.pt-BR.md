# Canducci.DataMasker

[![NuGet](https://img.shields.io/nuget/v/Canducci.DataMasker.svg)](https://www.nuget.org/packages/Canducci.DataMasker/)
[![Downloads NuGet](https://img.shields.io/nuget/dt/Canducci.DataMasker.svg)](https://www.nuget.org/packages/Canducci.DataMasker/)
[![Build and Tests](https://github.com/fulviocanducci/Canducci.DataMasker/actions/workflows/dotnet-tests.yml/badge.svg)](https://github.com/fulviocanducci/Canducci.DataMasker/actions/workflows/dotnet-tests.yml)

**Canducci.DataMasker** é uma biblioteca .NET para mascarar informações sensíveis em objetos por meio de atributos. Ela ajuda a proteger dados pessoais, financeiros e outras informações confidenciais ao preparar objetos para logs, diagnósticos, relatórios, saídas de testes e outros cenários em que o valor original completo não deve ser exposto.

A biblioteca vai além de substituir caracteres em uma string: ela pode criar uma cópia mascarada de um grafo de objetos, percorrer objetos aninhados e coleções suportadas e preservar os relacionamentos entre referências sem modificar o objeto de origem.

**Documentação:** [English](https://github.com/fulviocanducci/Canducci.DataMasker/blob/master/README.md)

## Índice

- [Destaques](#destaques)
- [Requisitos](#requisitos)
- [Instalação](#instalação)
- [Primeiros passos](#primeiros-passos)
- [Tipos de máscara](#tipos-de-máscara)
- [Estratégias de máscara](#estratégias-de-máscara)
- [Objetos aninhados e coleções](#objetos-aninhados-e-coleções)
- [Máscara personalizada](#máscara-personalizada)
- [Valores inválidos e vazios](#valores-inválidos-e-vazios)
- [Criação de objetos e fábrica personalizada](#criação-de-objetos-e-fábrica-personalizada)
- [Propriedades suportadas e limitações](#propriedades-suportadas-e-limitações)
- [Considerações de segurança](#considerações-de-segurança)
- [Executando os testes](#executando-os-testes)
- [Contribuindo](#contribuindo)

## Destaques

- Configuração por atributos usando `[Mask]`.
- Regras prontas para dados pessoais, contatos, informações financeiras e dados sensíveis.
- Várias estratégias: completa, parcial por tipo, primeiros caracteres, últimos caracteres e personalizada.
- Mascaramento recursivo de objetos aninhados.
- Suporte a arrays unidimensionais e coleções `IList`, incluindo `List<T>`.
- Clonagem que mantém o objeto original e as coleções suportadas intactos.
- Preservação de referências repetidas e referências circulares durante a clonagem.
- Tratamento de texto compatível com Unicode, incluindo caracteres combinados e emojis.
- Comportamento fail-closed para valores não vazios e malformados tratados por máscaras estruturadas.
- Opção de fábrica personalizada para controlar a criação do objeto raiz.

## Requisitos

- Biblioteca compatível com .NET Standard 2.0.
- Pode ser consumida por aplicações que utilizam implementações compatíveis do .NET, incluindo versões modernas do .NET.

## Instalação

Instale o pacote pelo NuGet:

```bash
dotnet add package Canducci.DataMasker
```

Ou adicione a referência ao projeto:

```xml
<PackageReference Include="Canducci.DataMasker" Version="SUA_VERSAO" />
```

Substitua `SUA_VERSAO` pela versão desejada. Consulte o pacote [Canducci.DataMasker no NuGet](https://www.nuget.org/packages/Canducci.DataMasker/).

## Primeiros passos

Adicione os namespaces:

```csharp
using Canducci.DataMasker;
using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;
```

Decore as propriedades que contêm informações sensíveis:

```csharp
public class Cliente
{
    [Mask(MaskType.Name)]
    public string Nome { get; set; }

    [Mask(MaskType.Cpf)]
    public string Cpf { get; set; }

    [Mask(MaskType.Email)]
    public string Email { get; set; }

    [Mask(MaskType.Phone)]
    public string Telefone { get; set; }
}
```

Crie uma cópia mascarada:

```csharp
var cliente = new Cliente
{
    Nome = "Fulvio Canducci",
    Cpf = "12345678901",
    Email = "fulvio@example.com",
    Telefone = "18999991234"
};

var clienteMascarado = DataMasker.Mask(cliente);
```

A chamada retorna um novo objeto com as propriedades configuradas mascaradas. A instância original `cliente` não é modificada.

> Somente propriedades decoradas com `[Mask]` recebem máscara. As demais propriedades suportadas são copiadas como parte do objeto clonado.

## Tipos de máscara

O enum `MaskType` define as regras de mascaramento disponíveis.

| Tipo de máscara | Comportamento |
|---|---|
| `None` | Mantém o valor original quando utilizado com a estratégia parcial. |
| `Custom` | Reservado para cenários personalizados; use o construtor com estratégia personalizada para executar um método próprio. |
| `Name` | Preserva o primeiro elemento de texto de cada palavra. |
| `Cpf` | Preserva os três primeiros e os dois últimos dígitos. |
| `Cnpj` | Preserva os dois primeiros e os dois últimos dígitos. |
| `Rg` | Preserva os três últimos elementos de texto. |
| `Email` | Preserva o primeiro elemento de texto do usuário e o domínio. |
| `Phone` | Preserva o DDD e os quatro últimos dígitos de telefone fixo brasileiro. |
| `Mobile` | Preserva o DDD e os quatro últimos dígitos de celular brasileiro. |
| `Cep` | Preserva os cinco primeiros dígitos. |
| `Address` | Preserva a primeira letra de cada palavra e a pontuação. |
| `BirthDate` | Oculta dia e mês, preservando o ano. |
| `IpAddress` | IPv4: preserva o primeiro octeto. IPv6: preserva o primeiro grupo. |
| `CreditCard` | Preserva os quatro últimos dígitos para valores com 15 a 19 dígitos. |
| `BankAccount` | Mascara os dígitos e mantém os separadores. |
| `Pix` | Para chaves de e-mail, preserva o primeiro elemento de texto do usuário e do domínio; para outras chaves, preserva o primeiro elemento de texto. |
| `Salary` | Mascara o valor integralmente. |
| `Health` | Mascara o valor integralmente. |
| `Biometric` | Mascara o valor integralmente. |
| `Password` | Mascara o valor integralmente. |
| `Token` | Mascara o valor integralmente. |

As regras consideram os formatos descritos acima. Para documentos e contatos brasileiros, informe os valores no formato esperado para que a regra correspondente consiga reconhecê-los.

## Estratégias de máscara

O enum `MaskStrategy` controla como uma propriedade será mascarada.

| Estratégia | Comportamento |
|---|---|
| `Partial` | Aplica a regra definida pelo `MaskType` da propriedade. É a estratégia padrão. |
| `Full` | Mascara todos os elementos de texto do valor. |
| `FirstCharacters` | Preserva o primeiro elemento de texto e mascara os demais. |
| `LastCharacters` | Preserva o último elemento de texto e mascara os elementos anteriores. |
| `Custom` | Executa um método estático fornecido pelo desenvolvedor. |

### Definindo uma estratégia

```csharp
public class Funcionario
{
    [Mask(MaskType.Name, MaskStrategy.Full)]
    public string Nome { get; set; }

    [Mask(MaskType.Email, MaskStrategy.Partial)]
    public string Email { get; set; }

    [Mask(MaskType.Cpf, MaskStrategy.LastCharacters)]
    public string Documento { get; set; }
}
```

Quando somente o tipo da máscara é informado, a estratégia `Partial` é utilizada:

```csharp
[Mask(MaskType.Cpf)]
public string Cpf { get; set; }
```

## Objetos aninhados e coleções

O mascaramento é recursivo. Quando o objeto raiz contém outro objeto, um array ou uma lista suportada, a biblioteca também clona e percorre esses valores. Os atributos das propriedades dos objetos aninhados são aplicados da mesma forma que os atributos das propriedades do objeto raiz.

```csharp
public class Cliente
{
    [Mask(MaskType.Name)]
    public string Nome { get; set; }

    public Endereco Endereco { get; set; }

    public List<Endereco> Enderecos { get; set; }
}

public class Endereco
{
    [Mask(MaskType.Address)]
    public string Rua { get; set; }

    [Mask(MaskType.Cep)]
    public string Cep { get; set; }
}
```

Execute o mascaramento uma única vez no objeto raiz:

```csharp
var clienteMascarado = DataMasker.Mask(cliente);
```

O objeto `Endereco` aninhado e cada item suportado de `Enderecos` são clonados e mascarados. Itens nulos dentro das coleções continuam nulos.

### Comportamento das coleções e referências

- Arrays unidimensionais são suportados.
- Coleções que implementam `IList`, incluindo `List<T>`, são suportadas.
- A coleção e seus elementos suportados são clonados.
- Itens nulos são preservados.
- Se várias posições da coleção apontarem para o mesmo objeto, as posições clonadas continuarão apontando para o mesmo objeto clonado.
- Referências circulares são rastreadas durante a clonagem para evitar recursão infinita.

## Máscara personalizada

Utilize o construtor personalizado de `MaskAttribute` para executar um método estático que recebe e retorna uma `string`.

```csharp
public static class MascaraAplicacao
{
    public static string MascararCodigoFuncionario(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return valor;
        }

        return valor.Length <= 2
            ? new string('*', valor.Length)
            : valor.Substring(0, 2) + new string('*', valor.Length - 2);
    }
}

public class Funcionario
{
    [Mask(MaskType.Custom, typeof(MascaraAplicacao), nameof(MascaraAplicacao.MascararCodigoFuncionario))]
    public string Codigo { get; set; }
}
```

O método configurado deve ser estático e compatível com uma entrada `string` e um retorno `string`. Erros de configuração ou execução são apresentados como exceções, em vez de serem ignorados silenciosamente.

## Valores inválidos e vazios

A biblioteca diferencia entradas vazias de valores estruturados malformados:

- Valores `null`, strings vazias e strings contendo somente espaços em branco são preservados.
- Para tipos estruturados (como CPF, CNPJ, RG, e-mail, telefone, CEP, data, endereço IP e cartão de crédito), um valor não vazio que não corresponde ao formato esperado é mascarado integralmente.
- Esse comportamento fail-closed ajuda a impedir que uma entrada malformada exponha acidentalmente seu conteúdo original.
- `None` e `Custom` com a estratégia parcial mantêm o comportamento explicitamente definido para esses tipos.

Mascaramento não é validação de entrada. Valide os dados separadamente antes de armazená-los ou processá-los.

## Criação de objetos e fábrica personalizada

A sobrecarga padrão exige um construtor público sem parâmetros no tipo raiz:

```csharp
var mascarado = DataMasker.Mask(cliente);
```

Quando o objeto raiz precisa de outra estratégia de criação, informe uma fábrica:

```csharp
var mascarado = DataMasker.Mask(cliente, origem => new Cliente
{
    // Inicialize a nova instância raiz, se necessário.
});
```

A fábrica deve retornar uma nova instância, nunca o próprio objeto de origem. Retornar a instância original gera uma `InvalidOperationException`. A fábrica também não deve modificar o objeto original.

Objetos complexos aninhados também precisam de construtores públicos sem parâmetros quando a biblioteca precisar criar suas cópias.

## Propriedades suportadas e limitações

A biblioteca utiliza propriedades públicas para percorrer e copiar os objetos.

- Somente propriedades públicas de instância com getter e setter públicos são copiadas e percorridas.
- Indexadores, propriedades estáticas, campos, propriedades somente leitura e propriedades somente escrita são ignorados.
- Tipos complexos aninhados precisam de construtor público sem parâmetros quando uma cópia precisa ser criada.
- As coleções suportadas são arrays unidimensionais e implementações de `IList`.
- Dicionários, `ISet<T>` (incluindo `HashSet<T>`), coleções que implementam somente enumeráveis personalizados e coleções somente leitura não são percorridos como listas suportadas.
- Tipos por valor e tipos imutáveis/de framework suportados são tratados como valores, em vez de serem percorridos recursivamente.
- A fábrica raiz opcional deve retornar uma instância diferente do objeto de origem.

Se seu modelo depender de um membro ou tipo de coleção fora desses casos suportados, revise cuidadosamente o objeto resultante e considere convertê-lo para um modelo compatível antes do mascaramento.

## Considerações de segurança

Canducci.DataMasker é uma ferramenta de **mascaramento de dados**, não um mecanismo de criptografia ou eliminação segura de dados.

- O objeto de origem permanece em memória com seus valores originais.
- O objeto retornado contém valores mascarados somente nas propriedades configuradas com `[Mask]`; propriedades sem o atributo são copiadas.
- Não trate a saída mascarada como anonimização irreversível nem como substituta de controle de acesso, criptografia, armazenamento seguro ou políticas de retenção de dados.
- Evite registrar em logs ou expor o objeto original quando o objetivo for proteger informações sensíveis.
- Revise quais propriedades devem ser mascaradas na aplicação, incluindo modelos aninhados e novos campos adicionados ao longo do tempo.

## Executando os testes

Clone o repositório e execute a suíte de testes:

```bash
git clone https://github.com/fulviocanducci/Canducci.DataMasker.git
cd Canducci.DataMasker
dotnet test Canducci.DataMasker.slnx --configuration Release
```

O repositório possui testes automatizados executados pelo GitHub Actions.

## Contribuindo

Issues, sugestões e pull requests são bem-vindos.

- Repositório: https://github.com/fulviocanducci/Canducci.DataMasker
- NuGet: https://www.nuget.org/packages/Canducci.DataMasker/

Ao relatar um problema, inclua um exemplo pequeno e reproduzível, o comportamento esperado e o comportamento observado.

## Licença

Consulte as informações de licença incluídas no repositório.
