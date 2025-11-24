# WebTemplate5065

Este template agora inclui um endpoint PHP simples para registrar usuários em um banco MySQL 5.6.23.

## Como usar o registro
1. Configure o MySQL 5.6.23 rodando localmente ou em seu servidor.
2. Crie um banco de dados e um usuário com permissões de escrita, por exemplo:
   ```sql
   CREATE DATABASE conquerredux CHARACTER SET utf8;
   CREATE USER 'conquer_user'@'localhost' IDENTIFIED BY 'trocar_senha';
   GRANT ALL PRIVILEGES ON conquerredux.* TO 'conquer_user'@'localhost';
   FLUSH PRIVILEGES;
   ```
3. Ajuste as variáveis `$dbHost`, `$dbName`, `$dbUser` e `$dbPass` no arquivo [`register.php`](register.php) para refletir seu ambiente.
4. Hospede os arquivos `index.html`, `styles.css` e `register.php` em um servidor com PHP 5.5+ (compatível com MySQL 5.6.23).
5. Acesse `index.html`, preencha o formulário de registro e envie. O script criará a tabela `users` caso não exista e salvará o cadastro com senha hash.

> Observação: o script é um exemplo mínimo. Em produção, proteja o formulário com CAPTCHA, confirmação por e-mail e validações específicas do seu servidor Conquer.
