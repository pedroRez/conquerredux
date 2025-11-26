<?php
// Simples endpoint de registro para MySQL 5.6.23.
// Ajuste as credenciais conforme o seu ambiente antes de usar em produção.

header('Content-Type: application/json; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['error' => 'Método não permitido.']);
    exit;
}

$required = ['username', 'password', 'email'];
foreach ($required as $field) {
    if (empty($_POST[$field])) {
        http_response_code(400);
        echo json_encode(['error' => 'Preencha todos os campos obrigatórios.']);
        exit;
    }
}

$honeypot = isset($_POST['website']) ? trim($_POST['website']) : '';
if ($honeypot !== '') {
    http_response_code(400);
    echo json_encode(['error' => 'Requisição recusada.']);
    exit;
}

$formTimestamp = isset($_POST['form_ts']) ? (int) $_POST['form_ts'] : 0;
// Valor vem em milissegundos; exige ao menos 3s entre carregar e enviar o formulário.
if ($formTimestamp <= 0 || (time() - (int) floor($formTimestamp / 1000)) < 3) {
    http_response_code(400);
    echo json_encode(['error' => 'Formulário enviado rápido demais. Confirme que não é um robô e tente novamente.']);
    exit;
}

$captchaResponse = $_POST['h-captcha-response'] ?? '';
$captchaSecret = getenv('HCAPTCHA_SECRET');

if (!$captchaSecret) {
    http_response_code(500);
    echo json_encode(['error' => 'Configure a variável HCAPTCHA_SECRET para validar o CAPTCHA.']);
    exit;
}

if (!$captchaResponse) {
    http_response_code(400);
    echo json_encode(['error' => 'Confirme o hCaptcha antes de enviar.']);
    exit;
}

$verifyContext = stream_context_create([
    'http' => [
        'header' => "Content-type: application/x-www-form-urlencoded\r\n",
        'method' => 'POST',
        'content' => http_build_query([
            'response' => $captchaResponse,
            'secret' => $captchaSecret,
        ]),
        'timeout' => 5,
    ],
]);

$verifyResult = file_get_contents('https://hcaptcha.com/siteverify', false, $verifyContext);
$captchaPayload = $verifyResult ? json_decode($verifyResult, true) : null;

if (!$captchaPayload || empty($captchaPayload['success'])) {
    http_response_code(400);
    echo json_encode(['error' => 'Não foi possível validar o hCaptcha. Tente novamente.']);
    exit;
}

$username = trim($_POST['username']);
$email = trim($_POST['email']);
$password = $_POST['password'];

// Restrições alinhadas ao schema do Nov_16_Backup.sql (accounts):
// Username/Password: varchar(16), Email: varchar(64)
$schemaLimits = [
    'username' => 16,
    'password' => 16,
    'email'    => 64,
];

if (!preg_match('/^[A-Za-z0-9_]+$/', $username)) {
    http_response_code(422);
    echo json_encode(['error' => 'O usuário deve conter apenas letras, números ou _']);
    exit;
}

if (strlen($username) > $schemaLimits['username']) {
    http_response_code(422);
    echo json_encode(['error' => 'O usuário deve ter até 16 caracteres para caber no banco.']);
    exit;
}

if (strlen($password) > $schemaLimits['password']) {
    http_response_code(422);
    echo json_encode(['error' => 'A senha deve ter até 16 caracteres para caber no banco.']);
    exit;
}

if (!filter_var($email, FILTER_VALIDATE_EMAIL) || strlen($email) > $schemaLimits['email']) {
    http_response_code(422);
    echo json_encode(['error' => 'Informe um e-mail válido de até 64 caracteres.']);
    exit;
}

// Configuração de conexão.
$dbHost = 'localhost';
$dbName = 'redux';
$dbUser = 'conquer_user';
$dbPass = 'trocar_senha';

try {
    $pdo = new PDO(
        // Banco do backup usa charset latin1; mantenha para evitar truncamentos.
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

// Verifica duplicidade usando a tabela existente restaurada do Nov_16_Backup.sql.
$check = $pdo->prepare('SELECT UID FROM accounts WHERE Username = :username OR EMail = :email LIMIT 1');
$check->execute([':username' => $username, ':email' => $email]);
if ($check->fetch()) {
    http_response_code(409);
    echo json_encode(['error' => 'Usuário ou e-mail já cadastrado.']);
    exit;
}

// Registra o usuário na tabela já criada pelo servidor (Redux espera senha em texto puro; adeque se usar hashing).
// Permission = 1 mantém o jogador como "Player" (vide Enum/Permissions.cs); valores maiores são staff e devem ser ignorados no ranking.
try {
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

    if (!$insert->rowCount()) {
        throw new PDOException('Nenhuma linha afetada na tabela accounts. Verifique permissões do usuário do banco.');
    }

    echo json_encode(['message' => 'Cadastro realizado com sucesso. Agora você pode fazer login no servidor.']);
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode(['error' => 'Não foi possível salvar o registro no banco. Erro SQL: ' . $e->getMessage()]);
}
