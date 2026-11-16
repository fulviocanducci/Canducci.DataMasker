# Canducci.DataMasker

Biblioteca para mascarar dados sensíveis em objetos .NET usando atributos.

## Uso básico

\`\`\`csharp
using Canducci.DataMasker;
using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;

public class Customer
{
    [Mask(MaskType.Name)]
    public string Name { get; set; }

    [Mask(MaskType.Cpf)]
    public string Cpf { get; set; }

    [Mask(MaskType.Email)]
    public string Email { get; set; }
}

var masked = DataMasker.Mask(customer);
\`\`\`

O objeto original não é alterado. A biblioteca cria uma cópia e aplica as máscaras nessa cópia.

## Tipos de máscara

| Tipo | Comportamento |
|---|---|
| \`None\` | Mantém o valor original. |
| \`Custom\` | Mantém o valor quando usado com a estratégia parcial; use a estratégia \`Custom\` para chamar um método personalizado. |
| \`Name\` | Preserva o primeiro elemento de texto de cada palavra. |
| \`Cpf\` | Preserva os três primeiros e os dois últimos dígitos. |
| \`Cnpj\` | Preserva os dois primeiros e os dois últimos dígitos. |
| \`Rg\` | Preserva os três últimos elementos de texto. |
| \`Email\` | Preserva o primeiro elemento do usuário e o domínio. |
| \`Phone\` | Preserva DDD e os quatro últimos dígitos de telefone fixo brasileiro. |
| \`Mobile\` | Preserva DDD e os quatro últimos dígitos de celular brasileiro. |
| \`Cep\` | Preserva os cinco primeiros dígitos. |
| \`Address\` | Preserva a primeira letra de cada palavra e a pontuação. |
| \`BirthDate\` | Oculta dia e mês, preservando o ano. |
| \`IpAddress\` | IPv4: preserva o primeiro octeto. IPv6: preserva o primeiro grupo. |
| \`CreditCard\` | Preserva os quatro últimos dígitos para números com 15 a 19 dígitos. |
| \`BankAccount\` | Mascara os dígitos e mantém os separadores. |
| \`Pix\` | Para e-mail, preserva o primeiro elemento do usuário e do domínio; para outras chaves, preserva o primeiro elemento. |
| \`Salary\`, \`Health\`, \`Biometric\`, \`Password\`, \`Token\` | Mascara integralmente. |

## Estratégias

- \`Full\`: mascara todos os elementos de texto.
- \`Partial\`: aplica a regra do \`MaskType\`.
- \`FirstCharacters\`: preserva o primeiro elemento de texto.
- \`LastCharacters\`: preserva o último elemento de texto.
- \`Custom\`: executa um método estático que recebe e retorna \`string\`.

Exemplo:

\`\`\`csharp
[Mask(MaskType.Name, MaskStrategy.Full)]
public string Name { get; set; }
\`\`\`

## Valores inválidos

Valores nulos e vazios são preservados. Quando um valor não vazio não corresponde ao formato esperado por uma máscara estruturada (CPF, CNPJ, RG, e-mail, telefone, CEP, data, IP ou cartão), o valor é mascarado integralmente. Essa política evita que uma entrada malformada exponha o conteúdo original.

Tipos \`None\` e \`Custom\` com estratégia parcial mantêm seu comportamento explícito. Erros na configuração de uma máscara ou na execução de um método personalizado geram exceção; não são ignorados silenciosamente.

## Objetos complexos e coleções

O mascaramento percorre recursivamente propriedades públicas de leitura e escrita, objetos aninhados, arrays e coleções que implementam \`IList\` (incluindo \`List<T>\`). Portanto, uma propriedade \`List<Address>\` terá as propriedades de cada \`Address\` mascaradas conforme seus atributos.

Cada objeto e coleção percorridos é clonado. O objeto original, suas listas e seus elementos não são modificados. Referências repetidas e ciclos de referência são tratados durante a cópia.

Limitações:
- Somente propriedades com getter e setter públicos são copiadas e percorridas.
- Tipos complexos aninhados precisam de construtor público sem parâmetros.
- Coleções suportadas são arrays e implementações de \`IList\`; dicionários, \`ISet<T>\`, enumeráveis customizados e coleções somente leitura não são percorridos.
- Campos, propriedades somente leitura, propriedades somente escrita, propriedades estáticas e indexadores são ignorados.
- Tipos imutáveis e tipos por valor são copiados como valores.
- A fábrica opcional deve retornar uma nova instância e não deve modificar o objeto de origem.

## Fábrica personalizada

\`\`\`csharp
var masked = DataMasker.Mask(customer, source => new Customer());
\`\`\`

A fábrica deve retornar uma instância diferente da origem. Retornar a própria origem gera \`InvalidOperationException\`, evitando que a cópia sobrescreva os dados originais.
