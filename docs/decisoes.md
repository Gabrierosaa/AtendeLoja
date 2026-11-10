# Decisões em aberto, resolvidas

As decisões principais (D01–D13) estão no [README](../README.md#decisões-principais). Este arquivo registra as decisões em aberto (A01–A06) à medida que forem resolvidas.

## A01 — Provedor de IA

- **Status:** decidida
- **Decisão:** Ollama local em desenvolvimento, atrás das abstrações `IChatClient` e `IEmbeddingGenerator` (`Microsoft.Extensions.AI`, D06). Provedores de nuvem entram só trocando `AI_PROVIDER` e `AI_API_KEY`, sem mudar o código dos casos de uso.
- **Modelos iniciais:** chat `llama3.2`, embeddings `nomic-embed-text` (768 dimensões).
- **Motivo:** custo zero, funciona offline e não envia dados da loja para terceiros durante o desenvolvimento.
- **Consequências:**
  - O Ollama roda no perfil `ollama` do Docker Compose e, sem GPU, fica lento (CPU).
  - Trocar o modelo de embeddings muda a dimensão dos vetores e exige reindexar os documentos.
  - A avaliação (fase 4) deve ser repetida ao trocar de provedor, porque a qualidade das respostas muda.
- **Configuração:** variáveis `AI_PROVIDER`, `AI_CHAT_MODEL`, `AI_EMBEDDING_MODEL` no `.env`.

## Ainda em aberto

A02 modelo de embeddings · A03 tamanho de trecho/k/limiar · A04 Controllers vs Minimal APIs · A05 hospedagem da demo · A06 retenção das perguntas.
