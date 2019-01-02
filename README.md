# GoHttpsClientForCSharp
Wrapper for GoHttpsClient in CSharp .NET

# GoHttpsClientForCSharp
Encapsulamento do projeto GoHttpsClient para uso facilitado em C#

## Começando
Basta clonar o projeto, compilar e executar.

## Uso do Software
Exemplo:
var goHttpsClient = new GoHttpsClient(TimeSpan.FromSeconds(10));
var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://google.com/");
var httpResponseMessage = goHttpsClient.Send(request);

## Ferramentas
1. C#

## Regras de colaboração
Deve estar em sincronia com a classe GoHttpClientWrapper do Cappta.Gp
 
 ## Colaboradores
 - Sérgio Fonseca - _Autor inicial_ - SammyROCK
