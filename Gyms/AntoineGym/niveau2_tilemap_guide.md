# Guide TileMap — niveau2.tscn (Grotte souterraine 5000×2500px)

## Contexte du projet
- **Moteur** : Godot 4, C#
- **Map** : 5000×2500 px, tiles 16×16
- **Grille** : 312×156 tuiles
- **7 zones** : Entrée, Caverne, Lac, Lave, Cristaux, Boss, Sortie
- **Assets Cave** : `Assets/Cave/Tiles.png` (12×6 tuiles), `Background.png`, `Water.png`, `Spikes.png`, `Vine_1.png`, `Bat.png`, `Coin.png`

---

## Étape 1 — Configurer le TileSet Cave

- **Tile Size** : 16×16
- `Tiles.png` = 2 blocs 96×96 côte à côte → 12 colonnes × 6 lignes
- Ajouter collisions : TileSet → Select → tuile → **Physics** → Add Physics Layer → carré plein
- Utiliser les **Terrains** (terrain "Cave_Rock") pour que Godot gère automatiquement coins/bords

---

## Étape 2 — Arbre de nœuds recommandé

```
niveau2 (Node2D)
├── Camera2D
│   └── limits: left=0, top=0, right=5000, bottom=2500
├── Background (Sprite2D ou CanvasLayer)
│   └── texture: Assets/Cave/Background.png
├── TileMapLayer_Sol          ← sol + murs solides
├── TileMapLayer_Plateformes  ← plateformes one-way
├── TileMapLayer_Danger       ← lave + spikes (visuels)
├── TileMapLayer_Eau          ← eau animée (Cave/Water.png)
├── Zones (Node2D)
│   ├── ZoneLava (Area2D + CollisionShape2D)
│   └── ZoneWater (Area2D + CollisionShape2D)
├── Character (instance)
├── PorteFinNiveau (instance)
└── Entities (Node2D)  ← pièces, battes, coffres
```

---

## Étape 3 — Zones en coordonnées tuiles (16×16)

| Zone | X début | X fin | Largeur |
|------|---------|-------|---------|
| ① Entrée grotte | 0 | 48 | 48 |
| ② Caverne sombre | 48 | 96 | 48 |
| ③ Lac souterrain | 96 | 140 | 44 |
| ④ Couloirs lave | 140 | 184 | 44 |
| ⑤ Caverne cristaux | 184 | 228 | 44 |
| ⑥ Salle du boss | 228 | 268 | 40 |
| ⑦ Sortie | 268 | 312 | 44 |

---

## Étape 4 — TileMapLayer_Sol

- **Sol principal** : colonnes 0→311, lignes **148→155**
- **Plafond** : colonnes 0→311, lignes **0→2**
- **Mur gauche** : colonnes 0→1, de haut en bas
- **Mur droit** : colonnes 310→311, de haut en bas
- **Séparateurs de zones** : aux frontières du tableau ci-dessus, **laisser un passage** lignes 90→120

---

## Étape 5 — TileMapLayer_Plateformes (one-way)

Configurer : Inspecteur TileMapLayer → Physics → `One Way` activé

| Zone | Ligne | Colonnes |
|------|-------|---------|
| ① | 140 | 2→4 |
| ① | 120 | 5→8 |
| ① | 105 | 3→5 |
| ② | 138 | 52→56 |
| ② | 122 | 58→64 |
| ② | 110 | 54→60 |
| ③ | 130 | 100→104 |
| ③ | 118 | 108→112 |
| ④ | 143 | 144→147 |
| ④ | 128 | 150→154 |
| ④ | 118 | 146→150 |
| ⑤ | 140 | 188→193 |
| ⑤ | 128 | 196→202 |
| ⑥ | symétrique | 234→240 et 248→254 |

---

## Étape 6 — Eau et lave

- **Eau** (Zone ③) : `TileMapLayer_Eau` avec `Assets/Cave/Water.png`, lignes **136→148**, col **96→140**
- **Lave** (Zone ④) : `TileMapLayer_Danger`, lignes **140→148**, col **140→184**, modulation rouge

---

## Étape 7 — Décorations et entités

| Asset | Usage |
|-------|-------|
| `Cave/Vine_1.png` | Sprite2D au plafond, zones ①②③ |
| `Cave/Spikes.png` | Sprite2D + Area2D au bord de la lave |
| `Cave/Bat.png` | Ennemi volant (script OscillationBat.cs déjà dispo) |
| `Cave/Coin.png` | Collectible sur les plateformes |

---

## Ordre de travail recommandé

1. Ouvrir `niveau2.tscn` dans Godot
2. Ajouter les 4 TileMapLayer, assigner `Cave/Tiles.png` à chacune
3. Configurer collisions dans le TileSet (une fois = appliqué partout)
4. Peindre sol + plafond + murs → espace jouable de base
5. Peindre plateformes zone par zone
6. Ajouter zones eau/lave (Area2D pour dégâts en C#)
7. Décorer + placer entités
