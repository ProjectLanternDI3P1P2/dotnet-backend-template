# Génération procédurale du donjon

> US-DUNGEON-01 (un nouveau donjon à chaque exploration) et US-DUNGEON-02
> (un donjon reproductible à partir d'une seed).

## 1. Qui fait quoi

| Responsabilité | Où | Pourquoi |
| --- | --- | --- |
| Générer le donjon (salles, portes, obstacles, éléments) | Service Dungeon, couche Domain | ADR-GLOB-011 : Dungeon possède le `DungeonRun`. Combat et Inventory doivent lire les mêmes positions par seed (US-DUNGEON-02). |
| Valider un déplacement | Service Dungeon, `DungeonRun.MoveHero` | ADR-FE-008 : le backend est la seule source de vérité. |
| Choisir les tuiles du tileset, animer, brouillard de guerre | Front Nuxt, feature `dungeon` | Le choix d'un sprite est une décision de présentation, pas une règle métier. |

Le client ne génère jamais le donjon : il le reçoit, le dessine, et **prédit**
les déplacements avec la même règle de franchissabilité que le backend, qui
tranche.

## 2. L'algorithme

La génération est une fonction pure de `(seed, réglages, version)` : pas
d'horloge, pas de `System.Random`, pas de collection ordonnée par hachage, pas
de flottant.

Un donjon compte **4 étages de 10 salles** (40 salles), plus une salle
d'escalier sur chacun des 3 premiers étages. Chaque étage se termine par un
boss ; celui du dernier étage est le boss final.

1. **Graphe de salles** (`RoomGraphBuilder`). Les salles d'un étage sont posées
   sur une grille de 13 × 13 cases. On part du centre, et chaque nouvelle salle
   est **accrochée à une salle existante** : l'étage est connexe par
   construction et contient exactement le nombre de salles demandé, sans boucle
   de ré-essai qui pourrait échouer. Les cases touchant une seule salle sont
   préférées (60 %), ce qui produit des branches et des culs-de-sac.
2. **Boucles.** Deux salles voisines non reliées reçoivent une porte
   supplémentaire avec 30 % de chance : moins d'allers-retours.
3. **Salle du boss et salle d'escalier.** Choisies *après* les boucles : le boss
   attend au bout de la plus longue branche. Sur les étages qui ne sont pas les
   derniers, la salle d'escalier est ajoutée **juste au nord** de la salle du
   boss, reliée à elle seule. La porte entre les deux est une **grille**
   (`gate`) : fermée tant que le boss de l'étage n'est pas vaincu. Si la salle du
   boss n'a de place qu'au sud, l'étage entier est retourné de haut en bas.
4. **Types de salles par quotas** : par étage, 1 départ, 1 boss, 1 trésor (en
   priorité dans un cul-de-sac), 1 salle vide et 6 salles de combat.
5. **Salles dessinées à la main** (`RoomTemplates`). Chaque salle reçoit un
   gabarit, tiré au sort parmi ceux de son type, en miroir ou non ; un même
   gabarit n'est jamais utilisé deux fois sur un étage. Les gabarits reprennent
   les idées des cartes d'exemple du pack :

   | Type | Gabarits |
   | --- | --- |
   | Départ | antichambre, octogone, carrefour à colonnes |
   | Combat | colonnade, fosses jumelles, salle en croix, chambre latérale, cellule nord (mur épais et passage), allées de pics, salle aux grilles d'égout, caserne, balcon à balustrade, réduit, salle en L effondrée, forêt de colonnes, galeries murées |
   | Trésor | cage de fer, sanctuaire à colonnes, trésor derrière un cercle de pics |
   | Vide | puits à grilles, réserve, galerie à colonnes (et deux gabarits de combat) |
   | Boss | salle du trône, arène à balustrades, arène entre deux fosses |
   | Escalier | escalier contre le mur nord entre deux colonnes, palier à balustrade, escalier entre quatre colonnes |

   Légende d'un gabarit : `.` sol, `#` mur, espace = fosse, `o` tonneau ou
   jarre, `I` colonne, `=` balustrade, `g` grille d'égout (praticable), `e`
   emplacement d'ennemi, `t` piège, `$` trésor, `B` boss, `>` escalier, `<`
   arrivée. Tout gabarit a des dimensions impaires, un centre praticable et une
   tuile praticable au milieu de chaque côté ; un test vérifie chacun.
6. **Tuiles** (`FloorBuilder`). Chaque colonne de la grille est aussi large que
   sa plus grande salle (plus une marge de 3 tuiles de chaque côté), chaque
   ligne aussi haute que sa plus grande salle. Chaque salle est centrée dans sa
   case : les salles d'une même ligne partagent la même ligne centrale, celles
   d'une même colonne la même colonne centrale. Le couloir entre deux voisines
   est donc une ligne droite qui arrive au milieu d'un côté. Les murs sont
   posés ensuite autour de tout ce qui n'est pas du vide : fosses, cloisons et
   balustrades comprises.
7. **Contenu.** Posé sur les emplacements des gabarits : le boss sur `B`, les
   trésors sur `$`, tous les pièges `t` (élément `trap`, franchissable : c'est
   au service Combat d'en appliquer les dégâts), et 1 à 4 ennemis tirés parmi
   les emplacements `e`, plus nombreux loin du départ.
8. **Validation** (`DungeonValidator`). Chaque donjon généré est revérifié :
   10 salles par étage, un boss par étage dans sa salle, salle d'escalier reliée
   à la seule salle du boss, **escalier inaccessible sans passer la grille**,
   toute tuile praticable atteignable depuis l'entrée, éléments sur des tuiles
   libres de leur salle. Une violation lève une exception plutôt que de livrer
   un donjon cassé.

## 3. Seed et déterminisme

- **Format.** 64 bits, partagés en 13 caractères base32 de Crockford
  (`0KX4M2T9QZ7PA`). Pas de I, L, O ni U ambigus. Les tirets et les minuscules
  sont acceptés à la saisie.
- **Nouvelle seed.** Tirée par `RandomNumberGenerator` (imprévisible), puis
  vérifiée contre la base : deux explorations n'ont jamais la même seed.
- **PRNG.** xoshiro256\*\*, initialisé par SplitMix64, avec un tirage borné sans
  biais (méthode de Lemire). Ses sorties sont figées par des tests contre les
  algorithmes de référence.
- **Pièges évités.** `System.Random(seed)` : son algorithme peut changer entre
  versions de .NET. `string.GetHashCode()` / `HashCode` : aléatoires à chaque
  démarrage du processus. `double` : arrondis variables selon la plateforme.
  Itération d'un `HashSet`/`Dictionary` : ordre non garanti.
- **Flux indépendants.** Chaque étape (plan, formes, contenu) et chaque étage
  tire dans son propre flux dérivé de la seed. Ajouter un tonneau ne décale pas
  le plan, et un étage inférieur peut être généré sans ceux du dessus.
- **Version.** `DungeonGenerator.CurrentVersion` est stockée avec chaque run.
  Un test *golden master* échoue dès qu'une modification change le donjon
  d'une seed existante : il faut alors incrémenter la version (et garder
  l'ancien algorithme si les anciennes runs doivent rester rejouables).
  Version actuelle : **3** (salles dessinées à la main, un boss par étage qui
  garde l'escalier). Une run créée dans une version antérieure répond 409 : il
  faut lancer une nouvelle exploration.
- **Seed inconnue.** `GET /dungeons/{seed}/...` répond 404 pour une seed
  qu'aucune run n'a utilisée, et 422 pour une seed mal formée. Le donjon
  n'est jamais stocké : il est régénéré à la demande (environ 13 ms pour les
  4 étages) et gardé
  en cache mémoire.
- **Rejouer.** `POST /dungeon-runs` avec une `seed` crée une nouvelle run sur
  le même donjon, avec les réglages et la version de la run d'origine.

## 4. API

| Méthode et route | Rôle | Codes |
| --- | --- | --- |
| `POST /api/v1/dungeon-runs` `{ gameSessionId, runId?, seed? }` | Crée une run et génère son donjon. Idempotent par `runId`. | 201, 409, 422 |
| `GET /api/v1/dungeon-runs/{runId}` | État de la run : héros, tour, salle courante, éléments sous le héros. | 200, 404 |
| `POST /api/v1/dungeon-runs/{runId}/moves` `{ direction }` | Une tuile, un tour. Murs, obstacles, colonnes, balustrades, vide, hors carte et grille d'un boss encore debout refusés. | 200, 404, 409, 422 |
| `POST /api/v1/dungeon-runs/{runId}/descents` | Prend l'escalier sous le héros. | 200, 404, 409 |
| `POST /api/v1/dungeon-runs/{runId}/boss-defeats` | Enregistre la victoire sur le boss de l'étage : la grille s'ouvre ; sur le dernier étage, la run est gagnée (`won`). À appeler par Combat ; le héros doit être dans la salle du boss. | 200, 404, 409 |
| `GET /api/v1/dungeons/{seed}/map?floor=0` | Étage complet : `rows` (1 caractère par tuile, décodé par `legend`), salles, éléments. | 200, 404, 422 |
| `GET /api/v1/dungeons/{seed}/cell?x=&y=&floor=0` | Type d'une tuile, sa salle et ses éléments (Combat, Inventory). | 200, 404, 422 |

Types de tuiles (`legend`) : `void`, `floor`, `wall`, `door`, `obstacle`,
`pillar`, `fence`, `stairsDown`, `stairsUp`, `gate`, `grate`. Sont praticables
`floor`, `door`, `grate`, les escaliers, et `gate` une fois le boss de l'étage
vaincu (champ `floorBossDefeated` de la run). Types d'éléments : `enemy`,
`boss` (un par étage), `item`, `trap`.

Un étage de 40 salles pèse environ 40 Ko en JSON (3 Ko compressé), contre
environ 1 Mo avec un objet JSON par tuile. Les réponses `map` et `cell` sont
immuables pour une seed donnée : elles portent
`Cache-Control: public, max-age=31536000, immutable`.

Deux déplacements simultanés sur la même run : `Turn` sert de jeton de
concurrence, le second reçoit un 409 au lieu d'écraser le premier.

## 5. Étages, boss et escaliers

Réglage par défaut, modifiable par configuration (les runs existantes gardent
leurs réglages) :

```json
"Dungeon": { "Generation": { "RoomCount": 40, "FloorCount": 4 } }
```

- Les 40 salles sont réparties entre les étages : 10 par étage. Les salles
  d'escalier ne comptent pas.
- Chaque étage a son boss, au bout de sa plus longue branche. Sur les 3 premiers
  étages, il garde la grille de la salle d'escalier, au nord de sa salle.
- Vaincre le boss (`POST …/boss-defeats`, par Combat) ouvre la grille ; le
  héros descend alors par l'escalier et arrive sur l'escalier montant de
  l'étage suivant, où la grille est de nouveau fermée.
- Vaincre le boss du dernier étage gagne la run (`status: won`).
- Les étages sont indépendants : le front précharge l'étage suivant dès
  l'arrivée, la descente est instantanée.
- Le combat n'existe pas encore : le front affiche un bouton « Fight the boss »
  qui appelle cet endpoint, en attendant le service Combat.

## 6. Vérifications effectuées

- 20 000 donjons de 4 étages générés : tous valides, tous différents, 13 ms par
  donjon. Par étage en moyenne : 12 ennemis, 12 pièges.
- 635 cas de tests du domaine (déterminisme, règles de l'US-01, gabarits,
  grille du boss, déplacements, escaliers, PRNG contre les valeurs de
  référence) et 19 tests des handlers concernés.
- Partie complète dans le navigateur : 4 étages, 3 grilles franchies après
  leur boss, victoire sur le boss final ; 729 tours, positions client et
  serveur identiques à chaque pas.

## 7. Suite

- Contrat gRPC `CreateDungeonRun` pour le service Player (ADR-GLOB-011) ; pour
  l'instant la création passe par le REST.
- Événement `DungeonRunEnded` à la victoire, défaite ou abandon.
- Pièges : définir avec l'équipe Combat l'effet d'un `trap` (dégâts,
  désamorçage) ; le Dungeon ne fait que les placer.
- Combat : appeler `POST /dungeon-runs/{runId}/boss-defeats` à la victoire sur
  un boss (idéalement par gRPC interne), puis retirer le bouton de test du
  front et restreindre l'endpoint au réseau interne.
