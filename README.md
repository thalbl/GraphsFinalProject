# 🏰 Node of Madness - Dungeon Crawler

> *"A cada passo, um custo. A cada escolha, uma consequência."*

**Node of Madness** é um jogo de exploração de masmorras procedurais que desafia o jogador a gerenciar recursos limitados enquanto navega por uma dungeon repleta de perigos. Desenvolvido como projeto final da disciplina de **Algoritmos e Grafos**, o jogo demonstra aplicações práticas de algoritmos e estruturas de grafos em game design.

---

## 🎮 Conceito

Você é um aventureiro preso em uma dungeon misteriosa. Seu objetivo é simples: alcançar a sala do Boss para escapar. Mas cada movimento exige sacrifícios. Quanto mais longe você vai, mais recursos consome. Quanto mais riscos toma, mais sua mente enfraquece.

O jogo coloca você diante de **escolhas constantes**: 
- Qual caminho seguir?
- Qual recurso sacrificar?
- Vale a pena explorar mais ou correr direto para o objetivo?

---

## ⚔️ Mecânicas Principais

### Sistema de Três Recursos

| Recurso | O que representa | Consequência de perder tudo |
|---------|------------------|----------------------------|
| **Vida (HP)** | Sua saúde física | **Game Over** - Morte definitiva |
| **Sanidade (SP)** | Sua estabilidade mental | **Trait** - Desenvolve uma aflição ou virtude |
| **Suprimentos** | Tempo e provisões | Representa o custo temporal das viagens |

Cada corredor entre salas consome esses recursos. Alguns caminhos são mais curtos mas drenam mais vida. Outros preservam seu corpo mas destroem sua mente.

### Sistema de Traits (Aflições e Virtudes)

Quando sua sanidade chega a zero, sua mente colapsa... mas não necessariamente para pior. Há 50% de chance de desenvolver:

**Aflições (Negativas)**
- 🔴 **Paranóico** - Custos de sanidade dobrados
- 🔴 **Imprudente** - Mais dano, menos tempo
- 🔴 **Hesitante** - Custos de tempo dobrados
- 🔴 **Frágil** - Todos os custos aumentam 30%
- 🔴 **Ganancioso** - Sanidade maior, tempo menor
- 🔴 **Claustrofóbico** - Pânico constante em movimento

**Virtudes (Positivas)**
- 🟢 **Estoico** - Mente resiliente (-30% sanidade)
- 🟢 **Vigoroso** - Corpo resistente (-30% vida)
- 🟢 **Ligeiro** - Movimentos ágeis (-30% tempo)
- 🟢 **Estrategista** - Mestre tático (-15% em tudo)

Após desenvolver um trait, sua sanidade é completamente restaurada, permitindo continuar a jornada... com novas regras.

### Tipos de Sala

| Sala | Ícone | Função |
|------|-------|--------|
| **Spawn** | 🟢 | Ponto de partida |
| **Combat** | ⚔️ | Desafios de combate |
| **Treasure** | 💰 | Recursos e recompensas |
| **Camp** | 🔥 | Restaura vida e sanidade |
| **Event** | ❓ | Eventos aleatórios |
| **Boss** | 💀 | Objetivo final - VITÓRIA |

---

## 🧭 Sistema GPS

Perdido na dungeon? O sistema GPS permite sacrificar **Sanidade** para revelar o **caminho ótimo** até qualquer sala escolhida. 

Ao ativar o GPS, você escolhe qual métrica otimizar:
- **Vida** - Caminho que preserva mais HP
- **Sanidade** - Caminho que preserva mais SP
- **Tempo** - Caminho mais rápido

O algoritmo A* calcula a rota ideal em tempo real, destacando as arestas do grafo com visualização animada.

---

## 📊 Métricas de Desempenho

Ao final de cada partida (vitória ou derrota), o jogo analisa suas decisões:

### Path Optimality (Otimalidade do Caminho)
Compara sua rota com o caminho perfeito calculado pelo A*. Você foi eficiente ou deu muitas voltas?

### Exploration Index (Índice de Exploração)
Quantas salas você visitou? Exploradores complecionistas vs. rushadores pragmáticos.

### Backtracking Cost (Custo de Retroação)
Quantas vezes você voltou pelo mesmo caminho? Indica indecisão ou becos sem saída.

### Risk Profile (Perfil de Risco)
Baseado em qual recurso você mais gastou:
- **O Equilibrado** - Gastos proporcionais
- **O Mártir** - Sacrificou muito HP
- **O Louco** - Sacrificou muita sanidade
- **O Hesitante** - Perdeu muito tempo

---

## 🎭 Sistema Narrativo

O jogo apresenta **Flavor Texts** contextuais que reagem a:
- Tipo de sala atual
- Condição do jogador (vida/sanidade baixa ou alta)
- Traits ativos
- Ações específicas (seleção, movimento, dano)

Cada jogada conta uma história diferente baseada nas suas escolhas e no seu estado mental.

---

## 🎓 Teoria dos Grafos Aplicada

O jogo é uma demonstração prática de conceitos de grafos:

### Estrutura
- **Vértices** = Salas da dungeon
- **Arestas** = Corredores conectando salas
- **Pesos** = Custos de movimento (Vida, Sanidade, Tempo)

### Algoritmos
- **A\* Pathfinding** - Encontra caminhos ótimos
- **DFS** - Geração procedural do layout
- **BFS** - Cálculo de distâncias do spawn

### Análise
- Otimalidade de caminho (comparação jogador vs. A*)
- Índice de exploração (cobertura do grafo)
- Backtracking (revisitação de vértices)

---

## 🎯 O Objetivo

Navegue pela dungeon procedural, gerencie seus recursos sabiamente e alcance a sala do Boss para vencer. Mas lembre-se:

> *A dungeon não perdoa hesitação. Cada escolha errada deixa cicatrizes - no corpo ou na mente.*

Boa sorte, aventureiro.

---

## 👥 Créditos

Projeto desenvolvido para a disciplina de **Algoritmos e Grafos**.
