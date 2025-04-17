# 🕷️ WebCrawler de Proxies

Este projeto é um **WebCrawler multithread em C# e .NET** desenvolvido como desafio técnico. Ele acessa o site [proxyservers.pro](https://proxyservers.pro/proxy/list/order/updated/order_dir/desc), extrai informações de proxies de várias páginas e salva os dados em um arquivo JSON, além de registrar logs da execução em um banco de dados SQLite.

---

## ✅ Funcionalidades

- ✅ Extrai os campos:
  - IP Address
  - Port
  - Country
  - Protocol
- ✅ Salva os dados extraídos em um arquivo `proxies.json`.
- ✅ Gera arquivos `.html` de cada página acessada (para auditoria).
- ✅ Registra em banco de dados SQLite:
  - Data de início da execução
  - Data de término da execução
  - Quantidade de páginas processadas
  - Quantidade de proxies (linhas) extraídas em todas as páginas
  - Caminho do arquivo JSON gerado
- ✅ Execução multithread com até 3 tarefas simultâneas (controle de concorrência com `SemaphoreSlim`)

---

## 🧱 Estrutura do Projeto

```
WebCrawler/
│
├── Models/
│   ├── Proxy.cs
│   └── CrawlerLog.cs
│
├── Services/
│   ├── ProxyService.cs
│   ├── IProxyService.cs
│   ├── DatabaseService.cs
│   └── HtmlHelper.cs
│
├── Data/
│   └── ProxyCrawler.db (gerado automaticamente)
│
├── pagina_1.html ... pagina_n.html (gerados na execução)
│
├── proxies.json (resultado final)
│
└── Program.cs (ponto de entrada)
```

---

## 🛠️ Tecnologias Utilizadas

- **C# com .NET 6+** — linguagem principal e ambiente de execução.
- **HttpClient** — usado para realizar as requisições HTTP ao site de proxies.
- **HtmlAgilityPack** — biblioteca para parse e extração de dados HTML de forma robusta.
- **System.Text.Json** — utilizado para serializar os dados extraídos para o arquivo `proxies.json`.
- **SQLite** com `Microsoft.Data.Sqlite` — banco de dados leve para armazenar logs de execução localmente.
- **Multithreading** com `Task` e `SemaphoreSlim` — permite que até 3 tarefas simultâneas sejam executadas, otimizando a performance do crawler.

---

## 📦 Como Executar

1. Clone o repositório:

```bash
git clone https://github.com/seuusuario/WebCrawler.git
cd WebCrawler
```

2. Restaure os pacotes e compile o projeto:

```bash
dotnet restore
dotnet build
```

3. Execute o projeto:

```bash
dotnet run
```

> O processo pode levar alguns segundos dependendo da conexão. Verifique os arquivos gerados após a execução:
> - `proxies.json`
> - `pagina_1.html`, `pagina_2.html`, ...
> - `Data/ProxyCrawler.db`

---

## 📸 Exemplo de Resultado

```json
[
  {
    "IPAddress": "123.456.789.000",
    "Port": "8080",
    "Country": "Brazil",
    "Protocol": "HTTP"
  },
  ...
]
```

---

## 📚 Licença

Este projeto é apenas para fins de avaliação técnica.
