# Implementando eventos inclusivos com prêmios

Este guia transforma as ideias de eventos justos em passos técnicos para um servidor Conquer com backend MySQL e lógica em C#/PHP.

## Metas principais
- Dar chance real de vitória a jogadores de qualquer nível.
- Reduzir a vantagem de poder bruto em eventos competitivos.
- Garantir segurança básica (antifraude) e transparência.

## Estrutura de dados sugerida
Crie tabelas específicas para eventos e inscrições. Exemplos para MySQL 5.6:

```sql
CREATE TABLE event_config (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(64) NOT NULL,
  type ENUM('sorteio','prova','pvp_normalizado') NOT NULL,
  start_at DATETIME NOT NULL,
  end_at DATETIME NOT NULL,
  max_tickets_per_player TINYINT UNSIGNED DEFAULT 3,
  reward_summary VARCHAR(255) NOT NULL
);

CREATE TABLE event_entry (
  id INT AUTO_INCREMENT PRIMARY KEY,
  event_id INT NOT NULL,
  account_id INT NOT NULL,
  character_id INT NULL,
  tickets TINYINT UNSIGNED NOT NULL DEFAULT 1,
  score INT DEFAULT 0,
  created_at DATETIME NOT NULL,
  UNIQUE KEY uq_entry (event_id, account_id),
  FOREIGN KEY (event_id) REFERENCES event_config(id)
);

CREATE TABLE event_reward (
  id INT AUTO_INCREMENT PRIMARY KEY,
  event_id INT NOT NULL,
  account_id INT NOT NULL,
  character_id INT NULL,
  item_id INT NOT NULL,
  amount INT NOT NULL DEFAULT 1,
  rarity ENUM('raro','comum','participacao') NOT NULL,
  delivered TINYINT(1) NOT NULL DEFAULT 0,
  delivered_at DATETIME NULL,
  FOREIGN KEY (event_id) REFERENCES event_config(id)
);
```

## Fluxo geral do evento
1. **Inscrição** (via NPC ou página web) grava `event_entry` com `tickets = 1`.
2. **Miniobjetivos** aumentam tickets até `max_tickets_per_player` (ex.: 3). Cada objetivo valida duplicidade antes de dar ticket.
3. **Normalização** (para provas PvP/PvE) equipa todos com itens pré-setados e ajusta atributos ao entrar na área do evento.
4. **Apuração** executa sorteio ponderado por tickets e grava `event_reward`.
5. **Entrega** permite resgatar prêmios por NPC ou comando web, marcando `delivered = 1`.

## Lógica de sorteio ponderado (C# pseudo no servidor)
```csharp
var entries = db.EventEntries.Where(e => e.EventId == eventId).ToList();
var bag = new List<int>(); // guarda account_id repetidos conforme tickets
foreach (var entry in entries)
{
    for (int i = 0; i < entry.Tickets; i++) bag.Add(entry.AccountId);
}
var rng = new Random();
int winnerId = bag[rng.Next(bag.Count)];
// registrar na event_reward
```
- Garanta `max_tickets_per_player` no servidor para evitar manipulação via cliente.
- Para prêmios múltiplos, sorteie mais vezes removendo vencedores repetidos quando necessário.

## Normalização de combate (exemplo em C#)
```csharp
void ApplyEventLoadout(Character c)
{
    c.SetStats(str: 100, agi: 100, vit: 100, spi: 100);
    c.EquipStandardGear(eventGearSetId);
    c.RecalculateBattlePower();
}
```
- Chame a função ao teletransportar o jogador para o mapa do evento e reverta ao sair.
- Bloqueie uso de itens pessoais dentro do mapa do evento.

## Miniobjetivos fáceis de validar
- **Coleta**: matar N monstros específicos grava `score` na `event_entry`.
- **Quiz**: NPC entrega ticket se a resposta correta ainda não foi usada pelo jogador (use `event_entry` para marcar).
- **Tempo de reação**: registrar timestamp de início e fim; jogadores abaixo de um limite ganham ticket.

## Proteções antiabuso
- Índice único `uq_entry` impede multi-inscrição na mesma conta.
- Valide IP/dispositivo no servidor se possível; rejeite mais de X contas por IP na janela do evento.
- Limite de tickets controlado pelo servidor, não pelo cliente.

## Entrega de itens (exemplo PHP simplificado)
```php
// events_claim.php (esqueleto)
require 'db.php';
$accountId = requireLogin();
$rewards = $db->prepare('SELECT * FROM event_reward WHERE account_id = ? AND delivered = 0');
$rewards->execute([$accountId]);
foreach ($rewards as $reward) {
    giveItemToCharacter($reward['character_id'], $reward['item_id'], $reward['amount']);
    $db->prepare('UPDATE event_reward SET delivered = 1, delivered_at = NOW() WHERE id = ?')->execute([$reward['id']]);
}
```
- Substitua `giveItemToCharacter` pela chamada já usada pelo seu painel/servidor para criar itens.
- Exiba log para o jogador ver o que foi entregue.

## Transparência e UX
- Mostre `reward_summary` e probabilidade base do sorteio na página/NPC.
- Informe quantos tickets o jogador possui e os horários de encerramento (`start_at`/`end_at`).
- Anuncie automaticamente os vencedores lendo `event_reward` após a apuração.
- No servidor Redux, há um job interno que roda a apuração ponderada por tickets e um comando `/claimrewards` para resgatar itens pendentes, marcando `delivered`/`delivered_at` para auditoria.

## Checklist rápido para cada evento
- [ ] Cadastrar `event_config` com janela de tempo e limite de tickets.
- [ ] Implementar gatilhos dos miniobjetivos incrementando `tickets` com `LEAST(tickets+1, max)`.
- [ ] Teleportar jogadores para mapa com loadout padronizado (se houver combate).
- [ ] Rodar job de apuração que grava `event_reward`.
- [ ] Disponibilizar resgate e log público dos vencedores.

Seguindo esta estrutura, você consegue eventos repetíveis, auditáveis e equilibrados para jogadores de qualquer nível.
