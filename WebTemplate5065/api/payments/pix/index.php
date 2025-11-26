<?php
// Endpoint POST /api/payments/pix
// Gera um pagamento Pix via REST externo (quando configurado) ou fallback local.

declare(strict_types=1);

session_start();
header('Content-Type: application/json; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['error' => 'Método não permitido.']);
    exit;
}

$rawBody = file_get_contents('php://input');
$data = json_decode($rawBody, true);
if (!is_array($data) || empty($data)) {
    $data = $_POST;
}

$amount = isset($data['amount']) ? (float) $data['amount'] : null;
$description = isset($data['description']) ? trim((string) $data['description']) : '';
$orderId = isset($data['order_id']) ? trim((string) $data['order_id']) : '';
$payerInput = isset($data['payer']) && is_array($data['payer']) ? $data['payer'] : [];

$payer = [
    'name' => isset($payerInput['name']) ? trim((string) $payerInput['name']) : null,
    'tax_id' => isset($payerInput['tax_id']) ? preg_replace('/\D+/', '', (string) $payerInput['tax_id']) : null,
    'email' => isset($payerInput['email']) ? trim((string) $payerInput['email']) : null,
];

if ($amount === null || $amount <= 0 || $description === '' || $orderId === '') {
    http_response_code(422);
    echo json_encode([
        'error' => 'Informe amount (> 0), description e order_id.',
        'hint' => 'Exemplo: {"amount": 10.5, "description": "Pacote CP", "order_id": "ORD-123"}',
    ]);
    exit;
}

$payloadRequest = [
    'amount' => round($amount, 2),
    'description' => mb_substr($description, 0, 140, 'UTF-8'),
    'order_id' => $orderId,
    'payer' => array_filter($payer),
];

$providerBaseUrl = rtrim((string) getenv('PIX_PROVIDER_BASE_URL'), '/');
$providerPath = getenv('PIX_PROVIDER_PATH');
$providerToken = getenv('PIX_PROVIDER_TOKEN');
$providerResponse = null;

if ($providerBaseUrl !== '') {
    $endpoint = $providerBaseUrl . ($providerPath ?: '/api/pix/charges');
    $ch = curl_init($endpoint);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_POST, true);
    curl_setopt($ch, CURLOPT_HTTPHEADER, array_filter([
        'Content-Type: application/json',
        $providerToken ? 'Authorization: Bearer ' . $providerToken : null,
    ]));
    curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($payloadRequest, JSON_UNESCAPED_UNICODE));

    $responseBody = curl_exec($ch);
    $curlError = curl_error($ch);
    $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);

    if ($curlError) {
        error_log('PIX provider erro de cURL: ' . $curlError);
    } elseif ($responseBody !== false && $httpCode < 400) {
        $decoded = json_decode($responseBody, true);
        if (json_last_error() === JSON_ERROR_NONE) {
            $providerResponse = $decoded;
        } else {
            error_log('PIX provider retornou JSON inválido: ' . json_last_error_msg());
        }
    } else {
        error_log('PIX provider HTTP status: ' . $httpCode . ' body: ' . (string) $responseBody);
    }
}

$paymentId = $providerResponse['id'] ?? ('pix_' . bin2hex(random_bytes(8)));
$copyPastePayload = $providerResponse['payload'] ?? sprintf(
    'PIX|ORDER:%s|AMOUNT:%.2f|DESC:%s',
    $payloadRequest['order_id'],
    $payloadRequest['amount'],
    $payloadRequest['description']
);
$qrCodeData = $providerResponse['qr_code'] ?? null;

if ($qrCodeData === null) {
    $svg = sprintf(
        '<svg xmlns="http://www.w3.org/2000/svg" width="360" height="360" role="img" aria-label="PIX">'
        . '<rect width="100%%" height="100%%" fill="#f3f4f6"/>'
        . '<text x="50%%" y="50%%" dominant-baseline="middle" text-anchor="middle"'
        . ' font-family="Arial, sans-serif" font-size="14" fill="#111827">%s</text>'
        . '</svg>',
        htmlspecialchars($copyPastePayload, ENT_QUOTES, 'UTF-8')
    );
    $qrCodeData = 'data:image/svg+xml;base64,' . base64_encode($svg);
}

$response = [
    'payment_id' => $paymentId,
    'status' => 'pending',
    'payload' => $copyPastePayload,
    'qr_code' => $qrCodeData,
    'provider_raw' => $providerResponse,
];

echo json_encode($response, JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE);
