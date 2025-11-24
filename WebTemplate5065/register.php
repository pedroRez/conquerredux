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
$dbName = 'redux';
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

// Verifica duplicidade usando a tabela existente restaurada do Nov_16_Backup.sql.
$check = $pdo->prepare('SELECT UID FROM accounts WHERE Username = :username OR EMail = :email LIMIT 1');
$check->execute([':username' => $username, ':email' => $email]);
if ($check->fetch()) {
    http_response_code(409);
    echo 'Usuário ou e-mail já cadastrado.';
    exit;
}

// Registra o usuário na tabela já criada pelo servidor (Redux espera senha em texto puro; adeque se usar hashing).
$insert = $pdo->prepare(
    'INSERT INTO accounts (
        Username, Password, EMail, EmailStatus, Question, Answer, Permission, Token, Timestamp
    ) VALUES (
        :username, :password, :email, 0, "", "", 1, 0, :timestamp
    )'
);
$insert->execute([
    ':username' => $username,
    ':email' => $email,
    ':password' => $password,
    ':timestamp' => time(),
]);

echo 'Cadastro realizado com sucesso. Agora você pode fazer login no servidor.';
