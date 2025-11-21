# MapEdgeWalls

Use este utilitário para criar paredes (colisores) nas bordas do mapa que só colidem com o jogador.

Passos:
1. Abra a cena de mapa (ex: `Map_first` / `Map_div`) e selecione o GameObject principal do mapa.
2. Adicione o componente `MapEdgeWalls` (Assets/Filizola/_Scripts/MapEdgeWalls.cs).
3. Configure um `Tilemap` em `sampleTilemap` ou um `BoxCollider2D` com as dimensões do mapa, ou defina `manualCenter` e `manualSize`.
4. Pressione `Filizola -> Map Edge -> Ensure Player Layers` para criar as layers `Player` e `PlayerWall` automaticamente (Editor).
5. Selecione o GameObject Player e use `Filizola -> Map Edge -> Set Selected GameObject as Player Layer` para definir a layer `Player`.

Como funciona:

Nota:
 - Se seus mapas usam 900x600 (como informado), o script já tem `manualSize = (900,600)` como fallback em `MapEdgeWalls`.
	 Use `sampleTilemap` ou `sampleBoundsCollider` quando possível para evitar problemas de unidade (World Units vs pixels).
 - Para projetos onde o mapa é composto por vários GameObjects (objetos de decoração, obstáculos etc), ative `useChildrenBounds` para que as bordas usem a união das bounds de `Renderer` e `Collider2D` dos filhos do GameObject — isto configura as paredes exatamente nas bordas dos GameObjects.
 - Se as paredes top/bottom aparecem longe demais (ex.: y=8.16) e você quer aproximá-las (ex.: y=6.7), ajuste `verticalInset` no componente `MapEdgeWalls` (ex.: 1.46). `horizontalInset` também está disponível para reduzir a largura.
 - Se o jogador não tiver um Collider2D, ative `autoAddColliderToPlayer` para adicionar um `CircleCollider2D` automaticamente (ajuste o `radius` no inspector depois).
