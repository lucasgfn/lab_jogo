# Trabalho 1: Escape Room em Realidade Virtual (Laboratório de Química)

## 📝 Descrição do Projeto
O objetivo deste projeto é desenvolver um jogo de simulação de **Escape Room** em Realidade Virtual (RV) utilizando a Unity. O cenário consiste em um laboratório de química onde o jogador deve realizar interações e resolver puzzles para progredir entre as salas e finalizar o jogo.

## 🧪 Regras de Negócio (RN) Implementadas

### Sistema de Líquidos e Misturas
* **RN01 - Volume:** O tubo de ensaio perde líquido enquanto o copo de béquer aumenta o volume proporcionalmente (ajuste no eixo Y), respeitando o limite máximo do béquer.
* **RN02 - Mistura de Cores:** O líquido no béquer assume a cor do primeiro tubo derramado; ao adicionar o segundo e o terceiro tubos, as cores se combinam sequencialmente.
* **RN03 - Áudio de Interação:** Um efeito sonoro é ativado durante o derramamento e interrompido quando a ação termina.

### Puzzles e Progressão
* **RN04 - Puzzle da Primeira Sala:** Desafio original desenvolvido pela dupla. A resolução ativa um som de vitória e o efeito de *Outline* na porta. A porta permanece trancada até o acerto, e o *Outline* desaparece ao abri-la.
* **RN05 - Puzzle da Segunda Sala:** O jogador deve pressionar o botão central três vezes para liberar a porta de correr, seguindo a mesma lógica de feedback (som e *Outline*) da sala anterior.
* **RN06 - Vitória Final:** O jogo termina com uma animação de comemoração quando o jogador sai da área do laboratório.
