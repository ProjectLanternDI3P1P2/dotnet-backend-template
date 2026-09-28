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

1. **Graphe de salles** (`RoomGraphBuilder`). Les salles sont posées sur une
   grille de 13 × 13 cases. On part du centre, et chaque nouvelle salle est
   **accrochée à une salle existante** : le donjon est connexe par construction
   et contient exactement le nombre de salles demandé, sans boucle de
   ré-essai qui pourrait échouer. Les cases touchant une seule salle sont
   préférées (60 %), ce qui produit des branches et des culs-de-sac plutôt
   qu'un bloc compact.
2. **Boucles.** Deux salles voisines non reliées reçoivent une porte
   supplémentaire avec 30 % de chance (environ 2 boucles par donjon) : moins
   d'allers-retours pour le joueur.
3. **Salle finale.** Choisie *après* les boucles : c'est le cul-de-sac le plus
   éloigné du départ. Aucune boucle ne peut donc la rapprocher : sur 20 000
   seeds, le boss est toujours au bout d'une branche, à au moins 5 salles du
   départ, et à 9 en moyenne.
4. **Types de salles par quotas**, pas par probabilité : pour 40 salles,
   1 départ, 1 boss, 4 trésors (en priorité dans les culs-de-sac), 6 salles vides
   et 28 salles de combat. Aucun donjon n'est « malchanceux » dans sa répartition.
5. **Tuiles** (`FloorBuilder`). Chaque case fait 19 × 15 tuiles. L'intérieur
   d'une salle couvre toujours la ligne et la colonne centrales de sa case :
   le couloir entre deux voisines est donc une ligne droite qui ne peut ni
   rater une porte ni traverser une autre salle. Les tailles varient
   franchement : 30 % de petites salles (5–7 × 4–5), 45 % de moyennes
   (8–11 × 5–6), 25 % de grandes (12–15 × 7–8) ; le boss a toujours une grande
   salle (15 × 8). Une salle qui n'a de voisins que d'un côté est poussée vers
   eux : les couloirs restent courts.
6. **Aménagement** (`RoomLayouts`). Chaque salle reçoit une disposition, pour
   qu'aucune ne ressemble à la précédente :

   | Disposition | Salles | Ce qui est creusé ou posé |
   | --- | --- | --- |
   | Simple | départ, petites salles | rien, un peu de mobilier |
   | Échancrée | moyennes et grandes | 1, 2 ou 4 coins retirés (salles en L ou en croix) |
   | Fosses | grandes | 1 ou 2 trous dans le sol, entourés de murs |
   | Cloisonnée | grandes | un mur intérieur percé d'une porte : deux sous-pièces |
   | Colonnade | boss, escalier, grandes | deux rangées de colonnes |
   | Cage | trésor | grille de fer autour du coffre, ouverte au sud |
   | Réserve | moyennes | tonneaux et jarres entassés dans les coins |

   Chaque modification est annulée si elle rend la salle mal formée : le
   centre et chaque entrée doivent rester reliés par l'intérieur de la salle.
   Les murs sont posés ensuite autour de tout ce qui est praticable, fosses
   comprises.
7. **Contenu.** Les obstacles (tonneaux, jarres) ne sont jamais posés sur la
   croix centrale, et chaque pose est vérifiée par un parcours en largeur :
   aucun obstacle ne peut isoler une partie de salle. Les ennemis sont plus
   nombreux quand on s'éloigne du départ (1 à 4 par salle). Un tiers des
   salles de combat a une ligne de 2 à 4 pièges à pics (élément `trap`,
   franchissable : c'est au service Combat d'en appliquer les dégâts).
8. **Validation** (`DungeonValidator`). Le donjon généré est revérifié :
   nombre de salles, un seul boss dans la salle finale, toute tuile praticable
   atteignable depuis l'entrée, éléments sur des tuiles libres de leur salle.
   Une violation lève une exception plutôt que de livrer un donjon cassé.

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
  Version actuelle : **2** (salles variées, aménagements, colonnes, grilles,
  pièges). Une run créée en version 1 répond 409 : il faut lancer une
  nouvelle exploration.
- **Seed inconnue.** `GET /dungeons/{seed}/...` répond 404 pour une seed
  qu'aucune run n'a utilisée, et 422 pour une seed mal formée. Le donjon
  n'est jamais stocké : il est régénéré à la demande (environ 2,3 ms) et gardé
  en cache mémoire.
- **Rejouer.** `POST /dungeon-runs` avec une `seed` crée une nouvelle run sur
  le même donjon, avec les réglages et la version de la run d'origine.

## 4. API

| Méthode et route | Rôle | Codes |
| --- | --- | --- |
| `POST /api/v1/dungeon-runs` `{ gameSessionId, runId?, seed? }` | Crée une run et génère son donjon. Idempotent par `runId`. | 201, 409, 422 |
| `GET /api/v1/dungeon-runs/{runId}` | État de la run : héros, tour, salle courante, éléments sous le héros. | 200, 404 |
| `POST /api/v1/dungeon-runs/{runId}/moves` `{ direction }` | Une tuile, un tour. Murs, obstacles, colonnes, grilles, vide et hors carte refusés. | 200, 404, 409, 422 |
| `POST /api/v1/dungeon-runs/{runId}/descents` | Prend l'escalier sous le héros. | 200, 404, 409 |
| `GET /api/v1/dungeons/{seed}/map?floor=0` | Étage complet : `rows` (1 caractère par tuile, décodé par `legend`), salles, éléments. | 200, 404, 422 |
| `GET /api/v1/dungeons/{seed}/cell?x=&y=&floor=0` | Type d'une tuile, sa salle et ses éléments (Combat, Inventory). | 200, 404, 422 |

Types de tuiles (`legend`) : `void`, `floor`, `wall`, `door`, `obstacle`,
`pillar`, `fence`, `stairsDown`, `stairsUp`. Seuls `floor`, `door` et les
escaliers sont praticables. Types d'éléments : `enemy`, `boss`, `item`, `trap`.

Un étage de 40 salles pèse environ 40 Ko en JSON (3 Ko compressé), contre
environ 1 Mo avec un objet JSON par tuile. Les réponses `map` et `cell` sont
immuables pour une seed donnée : elles portent
`Cache-Control: public, max-age=31536000, immutable`.

Deux déplacements simultanés sur la même run : `Turn` sert de jeton de
concurrence, le second reçoit un 409 au lieu d'écraser le premier.

## 5. Étages et escaliers

C'est prévu par le modèle et activable par configuration :

```json
"Dungeon": { "Generation": { "RoomCount": 40, "FloorCount": 3 } }
```

- Les 40 salles sont réparties entre les étages (14, 13 et 13 pour 3 étages).
- Chaque étage sauf le dernier a une salle d'escalier au bout de sa plus longue
  branche. Le héros arrive à l'étage suivant sur un escalier montant.
- Seul le dernier étage a le boss final : la règle « exactement un boss » tient
  sur l'ensemble du donjon.
- Les étages sont indépendants : le front précharge l'étage suivant dès
  l'arrivée, la descente est instantanée.

**Décision à prendre avec le PO :** « exactement 40 salles » s'entend-il pour
tout le donjon (comportement actuel) ou par étage ? Pour passer à 40 salles par
étage, il suffit de modifier `DungeonSettings.RoomCountForFloor`. Les runs
existantes gardent leurs réglages.

## 6. Vérifications effectuées

- 20 000 seeds générées : toutes valides, toutes différentes, 2,3 ms par
  donjon. En moyenne par donjon : 88 tuiles creusées (échancrures, fosses,
  cloisons), 28 colonnes, 28 grilles, 21 pièges.
- 409 cas de tests du domaine exécutés (déterminisme, règles de l'US-01,
  variété des salles, déplacements, escaliers, PRNG contre les valeurs de
  référence).
- Parcours complet dans le navigateur d'un donjon à 3 étages jusqu'au boss :
  190 tours, positions client et serveur identiques à chaque pas.

## 7. Suite

- Contrat gRPC `CreateDungeonRun` pour le service Player (ADR-GLOB-011) ; pour
  l'instant la création passe par le REST.
- Événement `DungeonRunEnded` à la victoire, défaite ou abandon.
- Renommer la solution `Combat.*` en `Dungeon.*` dans une PR dédiée : le dépôt
  vient du template et porte encore l'ancien nom.
- Pièges : définir avec l'équipe Combat l'effet d'un `trap` (dégâts,
  désamorçage) ; le Dungeon ne fait que les placer.
