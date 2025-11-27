<?php
header('Content-Type: text/html; charset=utf-8');

$dbHost = 'localhost';
$dbName = 'redux';
$dbUser = 'conquer_user';
$dbPass = 'trocar_senha';

$characterId = isset($_GET['character_id']) ? (int) $_GET['character_id'] : 0;

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
    echo '<p>Erro ao conectar ao banco. Verifique as credenciais em events.php.</p>';
    exit;
}

try {
    $eventsSql = "
        SELECT c.id, c.title, c.starts_at, c.ends_at, c.reward_type, c.reward_value, c.winners_count, c.max_tickets_per_player,
               IFNULL(e.mini_objective_tickets, 0) AS tickets
        FROM event_config c
        LEFT JOIN event_entry e ON e.event_config_id = c.id AND e.character_id = :characterId
        WHERE c.status = 'ACTIVE' AND c.starts_at <= NOW() AND c.ends_at >= NOW()
        ORDER BY c.starts_at ASC";

    $eventsStmt = $pdo->prepare($eventsSql);
    $eventsStmt->bindValue(':characterId', $characterId, PDO::PARAM_INT);
    $eventsStmt->execute();
    $activeEvents = $eventsStmt->fetchAll();

    $history = [];
    if ($characterId > 0) {
        $historySql = "
            SELECT c.title, r.reward_type, r.reward_value, r.granted_at, r.delivered
            FROM event_reward r
            INNER JOIN event_entry e ON e.id = r.event_entry_id
            INNER JOIN event_config c ON c.id = e.event_config_id
            WHERE e.character_id = :characterId
            ORDER BY r.granted_at DESC
            LIMIT 20";

        $historyStmt = $pdo->prepare($historySql);
        $historyStmt->bindValue(':characterId', $characterId, PDO::PARAM_INT);
        $historyStmt->execute();
        $history = $historyStmt->fetchAll();
    }
} catch (PDOException $e) {
    http_response_code(500);
    echo '<p>Erro ao consultar eventos. Confirme se as tabelas event_config, event_entry e event_reward existem.</p>';
    exit;
}

function formatReward(array $row)
{
    $value = isset($row['reward_value']) ? (int) $row['reward_value'] : 0;
    $type = isset($row['reward_type']) ? strtoupper($row['reward_type']) : 'ITEM';

    switch ($type) {
        case 'CURRENCY':
            return $value . ' CP';
        case 'EXPERIENCE':
            return $value . ' EXP';
        default:
            return 'Item #' . $value;
    }
}

?>
<!doctype html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Eventos - Conquer Redux 5065</title>
  <link rel="stylesheet" href="styles.css">
  <style>
    .table { width: 100%; border-collapse: collapse; }
    .table th, .table td { padding: 10px; border-bottom: 1px solid #e0e0e0; text-align: left; }
    .muted { color: #6b7280; }
    .badge { display: inline-block; padding: 4px 8px; border-radius: 999px; background: #eef2ff; color: #4338ca; font-size: 12px; }
  </style>
</head>
<body>
  <header class="hero">
    <div class="container">
      <div>
        <p class="eyebrow">Eventos</p>
        <h1>Painel de Eventos</h1>
        <p class="lede">Consulte eventos ativos, seus tickets e o histórico de recompensas diretamente do banco.</p>
        <div class="cta-group">
          <a class="btn ghost" href="index.html">← Voltar ao site</a>
        </div>
      </div>
    </div>
  </header>

  <main class="section">
    <div class="container">
      <div class="card" style="margin-bottom:20px;">
        <p class="muted">Personalize a consulta passando <code>?character_id=SEU_UID</code> na URL.</p>
        <?php if ($characterId > 0): ?>
          <p>Consultando dados para o personagem UID <strong><?php echo htmlspecialchars((string)$characterId, ENT_QUOTES, 'UTF-8'); ?></strong>.</p>
        <?php else: ?>
          <p class="muted">Exibindo apenas eventos ativos. Adicione seu UID para ver tickets e recompensas.</p>
        <?php endif; ?>
      </div>

      <div class="card" style="margin-bottom:20px;">
        <div class="card-header">
          <h2>Eventos ativos</h2>
        </div>
        <?php if (empty($activeEvents)): ?>
          <p>Nenhum evento ativo neste momento.</p>
        <?php else: ?>
          <table class="table">
            <thead>
              <tr>
                <th>Título</th>
                <th>Início</th>
                <th>Fim</th>
                <th>Recompensa</th>
                <th>Tickets</th>
              </tr>
            </thead>
            <tbody>
              <?php foreach ($activeEvents as $event): ?>
                <tr>
                  <td><?php echo htmlspecialchars($event['title'], ENT_QUOTES, 'UTF-8'); ?></td>
                  <td class="muted"><?php echo htmlspecialchars($event['starts_at'], ENT_QUOTES, 'UTF-8'); ?></td>
                  <td class="muted"><?php echo htmlspecialchars($event['ends_at'], ENT_QUOTES, 'UTF-8'); ?></td>
                  <td><span class="badge"><?php echo htmlspecialchars(formatReward($event), ENT_QUOTES, 'UTF-8'); ?></span> (<?php echo (int)$event['winners_count']; ?> vencedor(es))</td>
                  <td><?php echo isset($event['tickets']) ? (int) $event['tickets'] : 0; ?><?php
                    if (!empty($event['max_tickets_per_player'])) {
                        echo ' / ' . (int) $event['max_tickets_per_player'];
                    }
                  ?></td>
                </tr>
              <?php endforeach; ?>
            </tbody>
          </table>
        <?php endif; ?>
      </div>

      <div class="card">
        <div class="card-header">
          <h2>Histórico de recompensas</h2>
        </div>
        <?php if ($characterId === 0): ?>
          <p class="muted">Informe <code>?character_id</code> para visualizar suas recompensas.</p>
        <?php elseif (empty($history)): ?>
          <p>Nenhuma recompensa encontrada para este personagem.</p>
        <?php else: ?>
          <table class="table">
            <thead>
              <tr>
                <th>Evento</th>
                <th>Recompensa</th>
                <th>Status</th>
                <th>Concedido em</th>
              </tr>
            </thead>
            <tbody>
              <?php foreach ($history as $reward): ?>
                <tr>
                  <td><?php echo htmlspecialchars($reward['title'], ENT_QUOTES, 'UTF-8'); ?></td>
                  <td><span class="badge"><?php echo htmlspecialchars(formatReward($reward), ENT_QUOTES, 'UTF-8'); ?></span></td>
                  <td class="muted"><?php echo $reward['delivered'] ? 'entregue' : 'pendente'; ?></td>
                  <td class="muted"><?php echo htmlspecialchars($reward['granted_at'], ENT_QUOTES, 'UTF-8'); ?></td>
                </tr>
              <?php endforeach; ?>
            </tbody>
          </table>
        <?php endif; ?>
      </div>
    </div>
  </main>
</body>
</html>
