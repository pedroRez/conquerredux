<?php
// Endpoint de login simples alinhado ao schema accounts/characters do Nov_16_Backup.sql.
session_start();
header('Content-Type: application/json; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['error' => 'Método não permitido.']);
    exit;
}

$required = ['username', 'password'];
foreach ($required as $field) {
    if (empty($_POST[$field])) {
        http_response_code(400);
        echo json_encode(['error' => 'Informe usuário e senha.']);
        exit;
    }
}

$username = trim($_POST['username']);
$password = $_POST['password'];

$schemaLimits = [
    'username' => 16,
    'password' => 16,
];

if (!preg_match('/^[A-Za-z0-9_]+$/', $username) || strlen($username) > $schemaLimits['username']) {
    http_response_code(422);
    echo json_encode(['error' => 'Usuário inválido ou maior que 16 caracteres.']);
    exit;
}

if (strlen($password) > $schemaLimits['password']) {
    http_response_code(422);
    echo json_encode(['error' => 'A senha deve ter até 16 caracteres.']);
    exit;
}

$dbHost = 'localhost';
$dbName = 'redux';
$dbUser = 'conquer_user';
$dbPass = 'trocar_senha';

try {
    $pdo = new PDO(
        "mysql:host={$dbHost};dbname={$dbName};charset=latin1",
        $dbUser,
        $dbPass,
        [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
        ]
    );
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode(['error' => 'Falha ao conectar ao banco. Verifique as credenciais e se o MySQL 5.6.23 está rodando.']);
    exit;
}

// Validação direta: o backup armazena senha em texto puro; ajuste se usar hashing no servidor.
$stmt = $pdo->prepare('SELECT UID, Username, Password FROM accounts WHERE Username = :username LIMIT 1');
$stmt->execute([':username' => $username]);
$account = $stmt->fetch();

if (!$account || $account['Password'] !== $password) {
    http_response_code(401);
    echo json_encode(['error' => 'Usuário ou senha incorretos.']);
    exit;
}

$_SESSION['account_uid'] = $account['UID'];
$_SESSION['account_username'] = $account['Username'];

$characterStmt = $pdo->prepare(
    'SELECT Name, Profession, Level, CP FROM characters WHERE UID = :uid ORDER BY Level DESC, Experience DESC LIMIT 1'
);
$characterStmt->execute([':uid' => $account['UID']]);
$character = $characterStmt->fetch();

$response = [
    'message' => 'Login bem-sucedido. Sessão iniciada.',
    'account' => [
        'uid' => $account['UID'],
        'username' => $account['Username'],
    ],
    'character' => $character,
];

echo json_encode($response);
