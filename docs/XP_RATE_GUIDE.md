# Sugestão de exp_rate para upar do nível 1 ao 130

Este guia oferece um ponto de partida para definir um **exp_rate** que deixe o avanço até o nível 130 rápido, porém competitivo e sem pay-to-win.

## Meta de tempo
- **Objetivo recomendado:** 20–30 horas de jogo ativo para chegar ao nível 130.
- O ritmo é acelerado, mas ainda exige que o jogador explore missões, times e eventos.

## exp_rate sugerido
- **exp_rate base:** entre **8× e 12×** o ganho oficial.
  - 8× mantém algum grind e dá mais valor para farming em grupo.
  - 10×–12× deixa a progressão bem rápida sem queimar todo o conteúdo.

## Curva por faixa de nível
Para evitar paredes de XP e ainda recompensar a progressão linear, você pode seguir a curva solicitada mais recente ou testar alternativas.

### Curva ativa no código (solicitada): rápida no início e final, moderada no meio
- **Níveis 1–40:** exp_rate **20×** para um onboarding muito rápido.
- **Níveis 41–99:** exp_rate **12×** para manter o ritmo acelerado sem queimar todo o conteúdo.
- **Níveis 100–109:** exp_rate **8×** para uma leve freada de progressão (o nível 100 já entra aqui).
- **Níveis 110–130:** exp_rate **20×** retomando a aceleração até o cap (o nível 110 já sobe para 20×).

### Curva alternativa sugerida anteriormente
- **Níveis 1–70:** exp_rate efetivo **12×** (ou aumente o drop de quests repetíveis). Isso garante onboarding rápido e evita abandono inicial.
- **Níveis 70–110:** exp_rate **10×**. Aqui o jogador já domina o kit e pode testar builds em grupo/solo.
- **Níveis 110–130:** exp_rate **8×**. Mantém o jogo competitivo e abre espaço para que eventos diários e em grupo façam diferença.

## Bônus situacionais (sem pay-to-win)
- **Primeira vitória do dia / missão diária:** +25–50% XP na conclusão.
- **Rested XP:** pool pequeno (+50–100% até encher ~1–2 níveis). Ajuda casuais a não ficarem para trás.
- **XP em grupo:** +10–20% quando em party balanceada. Incentiva cooperação sem forçar solo ou leech.

## Ajustes finos
- Monitore **tempo médio por nível** e **abandono por faixa de nível**. Se jogadores travarem em 110–120, suba o exp_rate dessa faixa para 9×–10× ou aumente XP de dailies.
- Evite multiplicadores extremos (>15×); eles encurtam demais o ciclo de itens/eventos e reduzem a vida útil do servidor.
- Mantenha os multiplicadores consistentes em eventos para não gerar sensação de pay-to-win ou injustiça.

## TL;DR
- Curva ativa: **20× (1–40)**, **12× (41–99)**, **8× (100–109)**, **20× (110–130)**.
- Alternativa: **12× (1–70)**, **10× (70–110)** e **8× (110–130)**.
- Tempo-alvo até o 130: **20–30 horas**.
- Use bônus diários, rested XP e incentivos em grupo para ritmo saudável sem cash shop de poder.

## Como aplicar no código
- Ajuste as faixas em `Redux/Constants.cs` dentro de `LevelExpRateBands` para definir o multiplicador por nível.
  - Valor padrão no código: **20× (1–40)**, **12× (41–99)**, **8× (100–109)**, **20× (110–130)**. Os intervalos com limites iguais são avaliados na ordem listada, então o nível 40 fica com **20×** e o nível 100 com **8×** como solicitado.
- `Player.GainExperience` usa `Constants.GetExpRateForLevel` para aplicar o multiplicador correto sempre que o jogador ganha XP.
- A mensagem de XP em grupo (`Monster.Kill`) usa o mesmo cálculo para mostrar o valor real ganho.
