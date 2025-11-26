<?php
// Finaliza a sessão de login para o site.
session_start();
header('Content-Type: application/json; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['error' => 'Método não permitido.']);
    exit;
}

if (!isset($_SESSION['account_uid'])) {
    echo json_encode(['message' => 'Nenhuma sessão ativa no momento.']);
    exit;
}

$_SESSION = [];
if (ini_get('session.use_cookies')) {
    $params = session_get_cookie_params();
    setcookie(session_name(), '', time() - 42000, $params['path'], $params['domain'], $params['secure'], $params['httponly']);
}

session_destroy();

echo json_encode(['message' => 'Sessão encerrada com sucesso.']);
