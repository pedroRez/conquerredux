# Guia para abrir o Conquer com resoluções modernas

Este guia resume uma estratégia testada para rodar o cliente do Conquer nas resoluções 1024×768 (4:3), 1366×768 (16:9) e 1920×1080 (16:9) mantendo proporções e alinhamento correto dos cliques.

## Conceito
O cliente original é fixo em 800×600. A abordagem mais segura é fazer *upscaling* do quadro 800×600 para a resolução desejada com letterbox/pillarbox, reaproveitando o cálculo de interface do próprio cliente e mapeando o mouse de volta para o espaço 800×600. Assim não surgem bugs de UI porque nenhuma coordenada interna é recalculada.

## Passo a passo sugerido
1. **Encapsular a janela:** inicie o cliente em modo janela 800×600 (parâmetro `-window` ou via wrappers como DxWnd). Capture o `HWND` e desative o redimensionamento direto para evitar distorções acidentais.
2. **Criar backbuffer escalado:** injete uma DLL de *wrapper* Direct3D/DirectDraw ou utilize um proxy `d3d8.dll`/`d3d9.dll` que:
   - força a resolução lógica para 800×600;
   - renderiza para uma *render target* 800×600;
   - executa um blit escalado para a superfície real do monitor mantendo aspect ratio.
3. **Letterbox/Pillarbox automático:** calcule o fator de escala `scale = min(telaLargura/800, telaAltura/600)`. Para 1366×768 e 1920×1080, isso gera barras laterais, preservando 4:3 sem esticar a interface.
4. **Remapeamento de mouse:** intercepte `WM_MOUSEMOVE`, `WM_LBUTTONDOWN`, etc. e converta `x` e `y` reais para o espaço lógico do cliente:
   ```csharp
   var scale = Math.Min(viewWidth / 800f, viewHeight / 600f);
   var offsetX = (viewWidth - 800 * scale) / 2f;
   var offsetY = (viewHeight - 600 * scale) / 2f;
   var clientX = (int)((mouseX - offsetX) / scale);
   var clientY = (int)((mouseY - offsetY) / scale);
   ```
   Envie `clientX/clientY` para o cliente como se estivesse em 800×600. Dessa forma, os cliques continuam alinhados em qualquer uma das resoluções alvo.
5. **Bloquear alternância de resolução interna:** garanta que chamadas `Reset` de Direct3D usem sempre 800×600 para evitar corrupções de HUD.
6. **Testar layouts sensíveis:** valide mini-map, diálogos de NPC e barras de skills em cada resolução alvo com o remapeamento ativo; se algum elemento parecer fora do lugar, confira se o fator e offsets estão sendo recalculados após `WM_SIZE`.

## Observações
- Essa solução reproduz o comportamento de *integer scaling* encontrado em wrappers já usados pela comunidade, evitando modificações binárias extensas no cliente.
- Caso prefira tela cheia, crie uma janela sem borda (`WS_POPUP`) do tamanho do monitor e aplique o mesmo esquema de escala + letterbox.
- Mantendo a lógica interna em 800×600 você reduz o risco de “bugs” como áreas de clique deslocadas ou minimapas truncados.
