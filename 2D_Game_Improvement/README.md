# Space Blaster
**Game Programming Class Assignment**

---

## Change Log

**Starting point:** Basic 2D space shooter with one level (Level 1), one enemy type (Straight Shooter), a basic HUD, sound effects, pause screen, and Game Over / Victory screens already in place.

**Changes made for this assignment:**
- Added Instructions page content (controls and objective text)
- Added in-game Objective text ("Defeat X enemies to win!")
- Added Level 2 scene with increased difficulty:
  - EnemySpawnerChaser (auto-spawns chasing enemies)
  - EnemySpawnerDiagonal (auto-spawns diagonal shooting enemies)
  - Faster spawn rate than Level 1
- Added background music to Main Menu and both gameplay levels
- Added space background image to Main Menu
- Updated HUD: fixed High Score display and added Objective text
- Updated sound effects for key player actions
- Fixed page navigation flow between all scenes and screens

**Why these changes improve the player experience:**
- Players now understand the goal before and during gameplay
- Two levels give a sense of progression and increasing challenge
- HUD keeps players informed of their performance at all times
- Background music makes the game feel more complete and polished

---

## Credits

**Engine:**
- Unity (unity.com)
- TextMesh Pro (Unity Technologies)

**Art Assets:**
- Space backgrounds, planets, asteroids, nebula sprites: Provided by class instructor (starter project)
- Enemy sprites (Chaser, Straight Shooter, Diagonal Shooter): Provided by class instructor (starter project)
- Player sprite: Provided by class instructor (starter project)
- UI elements (buttons, backdrop): Provided by class instructor (starter project)

**Audio:**
- Background music (Menu.wav, SongA.wav, SongB.wav, SongC.wav): Provided by class instructor (starter project)
- Sound effects (player fire, hit, explosion, game over, etc.): Provided by class instructor (starter project)

**Code:**
- Starter scripts provided by class instructor
- Additional scripts written by Yanshuo Liu

---

## How to Play

**Controls:**
| Key | Action |
|-----|--------|
| WASD / Arrow Keys | Move |
| Mouse | Aim |
| Left Click | Shoot |
| ESC | Pause |

**Objective:**
Defeat the required number of enemies to clear each level.
- Level 1: Defeat 10 enemies
- Level 2: Defeat 30 enemies (harder enemies, faster spawns)
