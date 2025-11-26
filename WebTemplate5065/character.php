<?php
// Retorna personagem associado à sessão ativa.
session_start();
header('Content-Type: application/json; charset=utf-8');

if (!isset($_SESSION['account_uid'])) {
    http_response_code(401);
    echo json_encode(['error' => 'Nenhuma sessão de login ativa.']);
    exit;
}

$account = [
    'uid' => $_SESSION['account_uid'],
    'username' => $_SESSION['account_username'] ?? null,
];

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

$characterStmt = $pdo->prepare(
    'SELECT Name, Profession, Level, CP FROM characters WHERE UID = :uid ORDER BY Level DESC, Experience DESC LIMIT 1'
);
$characterStmt->execute([':uid' => $_SESSION['account_uid']]);
$character = $characterStmt->fetch();

if (!$character) {
    echo json_encode([
        'message' => 'Sessão ativa, mas nenhum personagem foi encontrado para essa conta.',
        'account' => $account,
        'character' => null,
    ]);
    exit;
}

echo json_encode([
    'message' => 'Sessão ativa. Personagem recuperado com sucesso.',
    'account' => $account,
    'character' => $character,
]);
