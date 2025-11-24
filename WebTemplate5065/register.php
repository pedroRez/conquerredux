<?php
// Simples endpoint de registro para MySQL 5.6.23.
// Ajuste as credenciais conforme o seu ambiente antes de usar em produção.

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo 'Método não permitido.';
    exit;
}

$required = ['username', 'password', 'email'];
foreach ($required as $field) {
    if (empty($_POST[$field])) {
        http_response_code(400);
        echo 'Preencha todos os campos obrigatórios.';
        exit;
    }
}

$username = trim($_POST['username']);
$email = trim($_POST['email']);
$password = $_POST['password'];

// Configuração de conexão.
$dbHost = 'localhost';
$dbName = 'conquerredux';
$dbUser = 'conquer_user';
$dbPass = 'trocar_senha';

try {
    $pdo = new PDO(
        "mysql:host={$dbHost};dbname={$dbName};charset=utf8",
        $dbUser,
        $dbPass,
        [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
        ]
    );
} catch (PDOException $e) {
    http_response_code(500);
    echo 'Falha ao conectar ao banco. Verifique as credenciais e se o MySQL 5.6.23 está rodando.';
    exit;
}

// Cria a tabela se não existir (campos mínimos para 5065). Ajuste conforme sua estrutura oficial.
$pdo->exec(
    'CREATE TABLE IF NOT EXISTS users (
        id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
        username VARCHAR(32) NOT NULL UNIQUE,
        email VARCHAR(80) NOT NULL UNIQUE,
        password_hash VARCHAR(255) NOT NULL,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8'
);

// Verifica duplicidade.
$check = $pdo->prepare('SELECT id FROM users WHERE username = :username OR email = :email LIMIT 1');
$check->execute([':username' => $username, ':email' => $email]);
if ($check->fetch()) {
    http_response_code(409);
    echo 'Usuário ou e-mail já cadastrado.';
    exit;
}

// Registra o usuário.
$insert = $pdo->prepare(
    'INSERT INTO users (username, email, password_hash) VALUES (:username, :email, :hash)'
);
$insert->execute([
    ':username' => $username,
    ':email' => $email,
    ':hash' => password_hash($password, PASSWORD_BCRYPT),
]);

echo 'Cadastro realizado com sucesso. Agora você pode fazer login no servidor.';
