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

$type = isset($_GET['type']) ? strtolower(trim($_GET['type'])) : 'characters';
$type = in_array($type, ['characters', 'guilds'], true) ? $type : 'characters';

// Para classe, usamos a base (1=Trojans, 2=Warriors, 4=Archers, 5=Ninjas, 6=Monks, 9=Taoists).
$allowedClasses = [1, 2, 4, 5, 6, 9];
$classFilter = null;
if ($type === 'characters' && isset($_GET['class']) && $_GET['class'] !== '') {
    $candidate = (int) $_GET['class'];
    if (in_array($candidate, $allowedClasses, true)) {
        $classFilter = $candidate;
    }
}

try {
    if ($type === 'guilds') {
        $stmt = $pdo->prepare(
            'SELECT name AS Name, leader_name AS LeaderName, money AS Money, amount AS Members
             FROM guild
             WHERE del_flag = 0
             ORDER BY money DESC, amount DESC, name ASC
             LIMIT :limit'
        );
        $stmt->bindValue(':limit', $limit, PDO::PARAM_INT);
        $stmt->execute();
        $rows = $stmt->fetchAll();
    } else {
        $sql = 'SELECT c.Name, c.Profession, c.Level, c.CP, c.Experience
                FROM characters c
                INNER JOIN accounts a ON a.UID = c.UID
                WHERE a.Permission = 1';

        if ($classFilter !== null) {
            $sql .= ' AND FLOOR(c.Profession / 10) = :classBase';
        }

        $sql .= ' ORDER BY c.Level DESC, c.Experience DESC, c.CP DESC
                  LIMIT :limit';

        $stmt = $pdo->prepare($sql);

        if ($classFilter !== null) {
            $stmt->bindValue(':classBase', $classFilter, PDO::PARAM_INT);
        }

        $stmt->bindValue(':limit', $limit, PDO::PARAM_INT);
        $stmt->execute();
        $rows = $stmt->fetchAll();
    }

    echo json_encode([
        'type' => $type,
        'data' => $rows,
    ]);
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode(['error' => 'Erro ao consultar ranking. Confira se as tabelas necessárias existem e contém dados.']);
}
