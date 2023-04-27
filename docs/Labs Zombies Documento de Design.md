# Sumário

Jogo de sobrevivência de zumbi num mundo ZRPenho

O Labs Zombies é um jogo cooperativo de 4 jogadores, onde se deve sobreviver a uma horda de zumbis. O jogo é focado em cooperação e partidas rápidas onde os jogadores devem trabalhar juntos para vencer hordas de monstros que querem devorá-los.

# Núcleo

## Pilares

**Cooperação:** O jogo é baseado em cooperação, onde os jogadores devem trabalhar juntos para vencer os desafios. Todas mecânicas de jogo devem ser desenhadas para maximizar a cooperação entre os jogadores ou incentivá-la.

**Temas:** O jogo usa a identidade da ZRP como fonte de inspiração. Isto é, a identidade visual do jogo, o design dos jogadores e monstros e o tema dos mapas deve usar a identidade da ZRP como base.

## Loop de jogo

O jogo consiste em um gameloop baseado em "hordas". O objetivo dos jogadores é sobreviver a um número de hordas de monstros (10). Ao sobreviver as hordas, os jogadores vencem o jogo. Abaixo estão descritos o loop em detalhes.

- Jogo carrega o mapa, verifica quantos jogadores humanos tem e os coloca no mapa. Jogadores faltantes são preenchidos por bots. Até 4 jogadores permitidos em um mapa.
- O jogo inicia os jogadores com equipamentos básicos: uma pistola e munição suficiente para a primeira horda.
- Cada horda tem um número determinado de monstros, baseado no número de jogadores. Ao matar todos os monstros, a horda acaba.
- Ao acabar uma horda, inicia-se uma pausa no jogo e as lojas abrem.
- Os jogadores tem um tempo determinado para ir até a loja e adquirir novos equipamentos. Ao final do tempo, os jogadores são expulsos da área da loja e uma nova horda se inicia.
- O jogo segue este esquema por 10 hordas, com a última nascendo um monstro chefe - mais forte e resistente que os monstros normais e especiais.
- Ao matar o chefe, o jogo termina e os jogadores ganham a partida.

## Motivações e progressão

*TBD*

# Mecânicas

## Hordas

Uma horda é um espaço de tempo onde o controlador irá colocar em jogo monstros base, especiais e em certas ocasiões, um chefe. Cada horda é composta por um número determinado de monstros, dependendo do número de jogadores em jogo. Cada horda tem três turnos, que são ativados depois de um certo tempo ou quando todos zumbis em jogo são mortos. Cada turno coloca em jogo uma parte da horda completa, para não sobrecarregar os jogadores.

Ao não restar mais zumbis para serem colocados, a horda acaba.

Cada horda tem um número de vezes que pode ser colocado um zumbi especial em jogo.

## Moeda

Moeda é um recurso que os jogadores tem e pode ser trocado por itens nas Lojas. Jogadores ganham um número de Moeda com base no tipo de zumbi que for morto.

Ao ser morto, um zumbi derruba um item representando Moeda. O jogadore tem que coletar esse item no chão. Se demorar muito tempo, o item desaparece.

## Loja

*TBD*

# Conteúdo

## Narrativa

*TBD*

## Personagens

### Inimigos

**Base:** Um zumbi base, com aparência de um ser humano em decomposição. Seu comportamento é se dirigir até o jogador mais próximo e atacá-lo.

**Arranhador:** Um zumbi base com uma aparência e comportamento especial. O arranhador tem unhas longas e membros mais esguios. Seu comportamento é se dirigir ao jogador mais próximo, se esgueirando por entre os zumbis base, e atacar. Ao ser atacado, o jogador perde velocidade de movimento durante o ataque e por um tempo após deixar de ser atacado.

**Viciado:** Este zumbi tem uma aparência de barista desgrenhado, com um avental sujo e manchado de sangue. Seus olhos estão abertos de forma exagerada e ele tem uma barba longa e irregular. Sua característica mais marcante é o moedor de café que ele carrega nas costas, de onde ele pega as xícaras de café para arremessar. O moedor funciona como um morteiro, soltando fumaça e fazendo um som característico ao preparar o próximo ataque.  

**Sapo:** Este zumbi tem uma aparência anfíbia, com pele esverdeada e úmida, olhos saltados e uma língua longa e pegajosa que ele usa para raptar os jogadores. Suas pernas são longas e musculosas, permitindo que ele se mova rapidamente. O elemento marcante deste zumbi é a bolsa que ele tem em seu abdômen, onde ele guarda o jogador raptado, que fica visível e parcialmente exposto enquanto o Frog foge.  

**Blastozombi:** Este zumbi tem um aspecto inchado e esférico, com veias pulsantes visíveis sob sua pele. Seu rosto está coberto por uma máscara de gás danificada, e sua característica mais marcante é a quantidade de granadas e explosivos presos ao seu corpo, como se estivessem sendo usados para mantê-lo unido. Ele emite um som de chiado constante, aumentando a tensão enquanto se aproxima dos jogadores.  

**Pescador:** Este zumbi tem uma aparência de pescador cadavérico, com roupas de marinheiro rasgadas e um chapéu desgastado. Seu braço direito é substituído por um gancho de metal longo e afiado, que ele usa para puxar os jogadores. Sua característica mais marcante é a corrente que conecta o gancho ao seu braço, que emite um som metálico assustador ao ser arrastada pelo chão.  

**Saltador:** Este zumbi tem uma aparência atlética, com músculos definidos e vestindo roupas esportivas rasgadas. Suas pernas são incrivelmente longas e fortes, permitindo que ele pule grandes distâncias para atacar os jogadores. O elemento mais marcante do Jumper é a máscara que ele usa, semelhante à de um lutador de lucha libre, com padrões e cores vibrantes que contrastam com sua aparência sombria e degradada.

## Sumário dos níveis

*TBD*