# WebTemplate5065

Este template agora inclui um endpoint PHP simples para registrar usuários em um banco MySQL 5.6.23.

## Como usar o registro
1. Configure o MySQL 5.6.23 rodando localmente ou em seu servidor.
2. Crie o banco de dados `redux` e o usuário com permissões de escrita, por exemplo:
   ```sql
   CREATE DATABASE redux CHARACTER SET utf8;
   CREATE USER 'conquer_user'@'localhost' IDENTIFIED BY 'trocar_senha';
   GRANT ALL PRIVILEGES ON redux.* TO 'conquer_user'@'localhost';
   FLUSH PRIVILEGES;
   ```
3. Importe o `Nov_16_Backup.sql` original (presente na raiz do repositório) ou seu próprio backup para criar as tabelas reais do servidor (accounts, characters, etc.).
4. Ajuste as variáveis `$dbHost`, `$dbName`, `$dbUser` e `$dbPass` nos arquivos [`register.php`](register.php) e [`rank.php`](rank.php) para refletir seu ambiente.
5. Hospede os arquivos `index.html`, `styles.css`, `register.php` e `rank.php` em um servidor com PHP 5.5+ (compatível com MySQL 5.6.23).
6. Acesse `index.html`, preencha o formulário de registro e envie. O script grava na tabela `accounts` já criada pelo servidor, com a senha em texto simples (o LoginServer a compara diretamente). Caso queira hashing, adeque também o servidor.
7. O bloco de ranking consome `rank.php`, que lê a tabela `characters` restaurada do backup e preenche a tabela com os personagens de maior nível/experiência/CP.

> Observação: o script é um exemplo mínimo. Em produção, proteja o formulário com CAPTCHA, confirmação por e-mail e validações específicas do seu servidor Conquer.

## Endpoint Pix
O caminho `POST /api/payments/pix` (arquivo [`api/payments/pix/index.php`](api/payments/pix/index.php)) cria um pagamento Pix e devolve QR Code e payload "copia e cola".

- Parâmetros obrigatórios no corpo JSON: `amount` (número > 0), `description` e `order_id`.
- Campos opcionais: `payer.name`, `payer.tax_id`, `payer.email`.
- Variáveis de ambiente para integrar com um provedor real:
  - `PIX_PROVIDER_BASE_URL`: URL base do provedor (ex.: `https://api.sandbox.banco.com`).
  - `PIX_PROVIDER_PATH`: caminho da rota (padrão `/api/pix/charges`).
  - `PIX_PROVIDER_TOKEN`: token Bearer para autenticação.
- Se as variáveis não estiverem configuradas, o script gera um QR Code SVG embutido (data URI) e um payload local com status inicial `pending` e `payment_id` pseudoaleatório.

Resposta típica:

```json
{
  "payment_id": "pix_e3a1...",
  "status": "pending",
  "payload": "PIX|ORDER:ORD-123|AMOUNT:10.50|DESC:Pacote CP",
  "qr_code": "data:image/svg+xml;base64,...",
  "provider_raw": null
}
```
