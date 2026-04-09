# Tilemap Layouts -- Hackathon Version

GUILD ACADEMY: ABYSS CODE -- hackathon demo (1 chapter, up to week 2).

## Legend

```
# = Wall
. = Floor (walkable)
D = Door / entrance
@ = NPC placement
T = Treasure chest
< = Stairs up
> = Stairs down
S = Save point
P = Player start
E = Enemy spawn
B = Boss placement
~ = Water
* = Decoration (flower bed, statue, etc.)
```

---

## 1. Ray's House (village)

**Size**: 16 x 16 tiles (2 floors, each 16x16)

### 1F -- Living Room / Kitchen

```
################
#..........#...#
#..........#.T.#
#..........D...#
#..........#...#
#..........#####
#..............#
#......*.......#
#..............#
#..............#
#....@.........#
#..............#
#..............#
#..............#
#.P......<.....#
######DD########
```

### 2F -- Ray's Room

```
################
#..............#
#..............#
#...*..........#
#..............#
#..............#
#........*.....#
#..............#
#..............#
#..............#
#..T...........#
#..............#
#..............#
#..............#
#.P......>.....#
################
```

- **Placement**
  - 1F @: Mother NPC (farewell dialogue)
  - 1F T: Lunch box (recovery item)
  - 2F T: Old diary (flavor text)
  - 2F *: Bookshelf, bed
  - 1F *: Dining table
- **Connections**: 1F south door -> Village Road ; stairs connect 1F/2F
- **BGM**: `bgm_village_calm`

---

## 2. Village Road / Departure

**Size**: 32 x 16 tiles

```
################################
#~~............................#
#~~............................#
#......*........@..............#
#..............................#
#..............................#
#...............*..............#
#..............................#
#..............................#
#..............................#
#..............................#
#..............................#
#..@...........................#
#..............................#
#..............................#
D.P...........................D#
################################
```

- **Placement**
  - Left @: Villager NPC (flavor text)
  - Center @: Yuna (joins party, farewell scene)
  - *: Well, signpost
  - West D: Ray's House
  - East D: Carriage stop (scene transition to academy)
- **Connections**: West -> Ray's House ; East -> Carriage cutscene -> Academy Entrance
- **BGM**: `bgm_village_calm`

---

## 3. Academy Entrance -> Auditorium -> Classroom (Enrollment Route)

**Size**: 32 x 32 tiles (single connected map)

```
################################
#.............DD...............#
#..............................#
#..............................#
#..@...........................#
#..............................#
#..............................#
#..*........*........*.........#
#..............................#
#..............................#
#######D########D########D#####
#......#........#..........#..#
#......#........#..........#..#
#......#...@....#..........#..#
#......#........#....@.....#..#
#......#........#..........#..#
#......D........D..........D..#
#......#........#..........#..#
#......#........#..........#..#
#..@...#........#..........#..#
#......#........#..........#..#
#######D########D########D#####
#..............................#
#..............................#
#..............S...............#
#..............................#
#..............................#
#..............................#
#..*...........*...........*..#
#..............................#
#.P...........................D#
################################
```

- **Placement**
  - Top DD: Auditorium entrance (enrollment ceremony)
  - Row of 3 rooms (center band): Classroom A / Classroom B / Classroom C
  - Classroom A @: Teacher NPC
  - Classroom B @: Mio NPC (joins party here)
  - Bottom area @: Kaito NPC (first encounter)
  - S: Save point in corridor
  - *: Statues, bulletin boards, flower beds
- **Connections**
  - South P: From carriage arrival
  - South-East D -> Academy Main Area
  - Top DD: Auditorium (cutscene trigger)
- **BGM**: `bgm_academy_main`

---

## 4. Academy Main Area (Hub)

**Size**: 32 x 32 tiles

```
################################
#..............................#
#..####D####..####D####........#
#..#........#..#........#......#
#..#.Dorm...#..#.Dining.#......#
#..#........#..#...@....#......#
#..#..@.....#..#........#......#
#..#........#..#........#......#
#..##########..##########......#
#..............................#
#..............................#
#..*........*........*.........#
#..............................#
#.............S................#
#..............................#
D..............................D
#..............................#
#..............................#
#..*........*........*.........#
#..............................#
#..............................#
#..####D####..####D####........#
#..#........#..#........#......#
#..#.Class..#..#.Train..#......#
#..#.Wing...#..#.Ground.#......#
#..#..@.....#..#...@....#......#
#..#........#..#........#......#
#..#........#..#........>......#
#..##########..##########......#
#..............................#
#.P............................#
################################
```

- **Placement**
  - Top-Left room: Dormitory (bed = rest / heal)
  - Top-Right room: Dining Hall (NPC conversations, trust events)
  - Bottom-Left room: Classroom Wing (lectures, info flag events)
  - Bottom-Right room: Training Ground entrance ( > stairs to dungeon B1)
  - Courtyard (center): Decorations *, save point S
  - Dorm @: Roommate NPC
  - Dining @: Mio / Yuna (lunch event)
  - Class Wing @: Teacher NPC (info flags)
  - Training Ground @: Shion NPC (training trigger)
- **Connections**
  - West D: Academy Entrance map
  - East D: Town
  - South P: Spawn point (after enrollment)
  - Training Ground >: Dungeon B1
- **BGM**: `bgm_academy_main`

---

## 5. Town (Simplified)

**Size**: 24 x 20 tiles

```
########################
#......................#
#..####D####...........#
#..#........#..........#
#..#..Shop..#..........#
#..#...@....#..........#
#..##########..........#
#......................#
#......................#
#..####D####...........#
#..#........#..........#
#..#..Inn...#..........#
#..#...@....#....*..~..#
#..##########....*..~..#
#......................#
#......................#
#.S....................#
#......................#
#.P....................#
D......................D
########################
```

- **Placement**
  - Shop: Equipment/item merchant @
  - Inn: Rest/save, innkeeper @
  - *: Fountain, benches
  - ~: Decorative pond
  - S: Save point
- **Connections**
  - West D: Academy Main Area
  - East D: (unused in hackathon, future expansion)
- **BGM**: `bgm_town`

---

## 6. Training Ground Dungeon

### B1 -- Linear Tutorial Floor

**Size**: 20 x 20 tiles

```
####################
#..................#
#.<................#
#..................#
######D#############
#..................#
#..................#
#..E...............#
#..................#
#..................#
######D#############
#..................#
#..............T...#
#..................#
#..........E.......#
#..................#
######D#############
#..................#
#.P..........S.....#
####################
```

- **Placement**
  - P: Entry from Academy (appears at bottom)
  - <: Stairs up (return to Academy Training Ground room)
  - E x2: Slime / Training Dummy (weak enemies, 1 each)
  - T: Potion x2
  - S: Save point near entrance
  - 3 rooms connected by doors D, linear progression north
- **Flow**: Enter south -> fight E -> open door -> fight E -> open door -> find T -> stairs < to reach B2 entrance
  - Note: < here leads UP to the academy; a separate trigger at the north end transitions to B2
- **Connections**
  - P (bottom): Academy Training Ground
  - North (top area near <): Also serves as transition trigger to B2 (contextual: "go deeper" prompt)
- **BGM**: `bgm_dungeon_training`

### B2 -- Branching Floor with Mini-Boss

**Size**: 24 x 24 tiles

```
########################
#......................#
#.P....................#
#......................#
######D######D##########
#..........#.........T.#
#..........#...........#
#..........#...........#
#....E.....#...........#
#..........#...........#
####D#######...........#
#..........#...........#
#..........####D########
#..........#...........#
#..........#...........#
#..T.......#.....B.....#
#..........#...........#
#..........#...........#
####D######D############
#......................#
#......................#
#..............S.......#
#......................#
########################
```

- **Placement**
  - P: Entry from B1 (top-left)
  - Left branch: E (Training Golem - weak), T (Ether)
  - Right branch: T (Bronze Sword), dead-end treasure room
  - Center-south large room: B (Training Golem Boss -- Shion training fight trigger)
  - S: Save point before boss
  - Branching: left path has enemy + treasure; right path has treasure only; both converge at boss room
- **Boss**: Training Golem (mini-boss, controlled by Shion for practice)
  - HP: 150, ATK: 12, DEF: 8
  - Purpose: Teach break system mechanics
  - Shion provides hints during battle (trust point event)
- **Connections**
  - P: From B1
  - After boss defeat: Auto-return to Academy (cutscene)
- **BGM**: `bgm_dungeon_training` (boss: `bgm_boss_training`)

---

## Map Connection Diagram

```
Ray's House <-> Village Road --[carriage]--> Academy Entrance
                                                  |
                                                  v
                                          Academy Main Area (Hub)
                                         /        |        \
                                   Dormitory   Classroom   Training Ground
                                   Dining Hall  Wing            |
                                        |                       v
                                       Town              Dungeon B1
                                                              |
                                                              v
                                                        Dungeon B2 (Boss)
```

---

## Implementation Notes

- All maps use Unity Tilemap (Grid + Tilemap Renderer).
- Collider tiles on `#` walls; walkable on `.` floors.
- Door `D` tiles: Trigger zone that loads the connected map or opens a passage.
- NPC `@` positions are spawn points for prefabs; actual NPC prefab is placed at runtime or in scene.
- Enemy `E` / Boss `B` tiles: Trigger encounter on contact (transition to battle scene).
- Keep tile palette minimal for hackathon: wall, floor, door, stairs, water, decoration (6-8 tile types).
- Each map = 1 Unity Scene or 1 section within a scene using Tilemap + Cinemachine confiner.
