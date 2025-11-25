<?php
// Endpoint simples para retornar o ranking de personagens a partir das tabelas existentes no banco redux.
// Ajuste as credenciais conforme o seu ambiente antes de usar em produção.

header('Content-Type: application/json; charset=utf-8');

$dbHost = 'localhost';
$dbName = 'redux';
$dbUser = 'conquer_user';
$dbPass = 'trocar_senha';

try {
    $pdo = new PDO(
        // Base restaurada pelo Nov_16_Backup.sql usa latin1.
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

$limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 10;
$limit = max(1, min($limit, 50));

try {
    $stmt = $pdo->prepare(
        'SELECT Name, Profession, Level, CP, Experience
         FROM characters
         ORDER BY Level DESC, Experience DESC, CP DESC
         LIMIT :limit'
    );
    $stmt->bindValue(':limit', $limit, PDO::PARAM_INT);
    $stmt->execute();
    $rows = $stmt->fetchAll();

    echo json_encode(['data' => $rows]);
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode(['error' => 'Erro ao consultar ranking. Confira se a tabela characters existe e contém dados.']);
}
