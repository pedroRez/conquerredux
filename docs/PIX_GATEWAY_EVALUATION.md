# Avaliação de gateways Pix com API

Este guia resume as opções para integrar o método de pagamento Pix via API, descrevendo requisitos de sandbox, ativação do método, obtenção de credenciais e configuração de webhooks para os principais provedores: Mercado Pago, Pagar.me, Stripe (Brasil) e PagSeguro.

## Mercado Pago
- **Conta sandbox**: crie uma conta de desenvolvedor e ative o modo teste. Para testes, use as chaves `TEST` e cartões/CPFs de sandbox disponibilizados no painel de desenvolvedor.
- **Ativar Pix**: no painel de desenvolvedor, vá em *Configurações > Pagamentos on-line* e habilite Pix para o modo teste e produção. Verifique se o recebedor está validado (dados bancários completos).
- **Credenciais**: gere `Public Key` e `Access Token` (há versões `TEST` e `PROD`). Em produção, o Pix exige certificado ICP-Brasil para o recebedor; no sandbox o certificado não é necessário.
- **Webhooks**: cadastre a URL em *Notificações Webhook* (ex.: `https://seusite.com/webhooks/pix`). Selecione eventos de pagamento (`payment` ou `merchant_order`). Confirme que a assinatura está ativa com um `ping` do painel.

## Pagar.me
- **Conta sandbox**: crie conta e habilite o ambiente *Sandbox* no dashboard. Utilize as chaves de teste (`ak_test_xxx`/`sk_test_xxx`).
- **Ativar Pix**: em *Configurações > Métodos de pagamento*, habilite Pix. Para produção, é necessário cadastro de conta bancária e validação de KYC.
- **Credenciais**: obtenha `API Key` e `Encryption Key` (se estiver usando o checkout). Para Pix via API, use as chaves públicas/secretas específicas do ambiente (teste ou produção). Em produção, o provedor exige certificado para o PSP emissor e token de aplicação.
- **Webhooks**: registre a URL em *Configurações > Webhooks* e selecione eventos `transaction_status_changed` e `pix_transaction`. Teste o envio com o botão de simulação do painel.

## Stripe (Pix no Brasil)
- **Conta sandbox**: crie conta e mantenha o modo *Test* ativado. Pix é disponibilizado para contas brasileiras; verifique a liberação em *Settings > Payment Methods*.
- **Ativar Pix**: habilite Pix na seção de métodos de pagamento e conclua as validações de conta (informações bancárias e documentos). Em modo teste, o Pix funciona sem certificação adicional.
- **Credenciais**: use as chaves `pk_test_xxx`/`sk_test_xxx` no modo teste e `pk_live_xxx`/`sk_live_xxx` em produção. O método Pix é chamado via PaymentIntent com `payment_method_types: ["pix"]`.
- **Webhooks**: cadastre o endpoint no painel em *Developers > Webhooks* (ex.: `https://seusite.com/webhooks/pix`) e selecione eventos `payment_intent.succeeded`, `payment_intent.payment_failed` e `charge.updated`. Baixe a `Signing secret` para validar a assinatura dos eventos.

## PagSeguro
- **Conta sandbox**: crie conta de testes no PagSeguro e habilite o ambiente *Sandbox* no *PagBank for Developers*. Utilize o token de sandbox para chamadas.
- **Ativar Pix**: no painel de desenvolvedor, habilite Pix em *Meios de pagamento*. Em produção, é necessário concluir validação de conta e cadastrar dados bancários.
- **Credenciais**: use `Client ID` e `Client Secret` para OAuth 2.0 (ou token de aplicação, dependendo da API). Há valores distintos para sandbox e produção. Para cobrar Pix, utilize a API de cobranças imediatas (códigos QR ou copia e cola) com autenticação via bearer token.
- **Webhooks**: configure notificações em *Notificações* apontando para sua URL (ex.: `https://seusite.com/webhooks/pix`) e selecione eventos de transações Pix. Guarde o `notificationId`/token enviado para validar chamadas.

## Checklist de integração
1. Criar/ativar conta de sandbox em cada provedor e registrar chaves de teste em variáveis de ambiente seguras.
2. Ativar Pix no painel do provedor e confirmar requisitos de verificação de conta (KYC, dados bancários e certificado quando necessário).
3. Registrar `client_id/client_secret` ou `API key` para o ambiente correto (teste e produção) e armazenar de forma segura.
4. Configurar webhook em `https://seusite.com/webhooks/pix` (ou endpoint equivalente) e habilitar os eventos Pix relevantes.
5. Usar ferramentas do painel para enviar webhooks de teste e validar a assinatura/segurança (token, signing secret, certificado) em todos os ambientes.
