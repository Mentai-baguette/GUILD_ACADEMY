# Battle Scene Next Steps

## 1. Scene Wiring (Priority: High)
- [ ] Open `Assets/Scenes/Battle.unity` and place `BattleSceneBootstrap` on `BattleSceneRoot`.
- [ ] Create/assign `BattleUIManager` and `BattleBackdropController` references.
- [ ] Bind party root and enemy root transforms in inspector.
- [ ] Verify `SceneNames.Battle` load path works from field side.

## 2. UI Layout Setup (Priority: High)
- [ ] Place party slots (left side) and enemy slots (right side).
- [ ] Place command panel and battle message panel.
- [ ] Assign references for `BattleCommandMenuView` buttons and actor label.
- [ ] Assign references for each `BattleCombatantView` (name/hp/mp/atb/break texts and fills).

## 3. Rendering / Layer Order (Priority: High)
- [ ] Ensure world/effect objects render behind UI canvas.
- [ ] Put effect objects under `EffectLayer` and validate sorting.
- [ ] Validate camera settings for battle (orthographic size, clear color, clipping).

## 4. Data Flow / Runtime Check (Priority: Medium)
- [ ] Pass party/enemy data via `SceneTransitionData` keys:
  - `battle.party`
  - `battle.enemies`
- [ ] Verify fallback battle data works when no transition data is provided.
- [ ] Confirm ATB tick updates UI continuously.

## 5. Encounter Integration (Priority: Medium)
- [ ] Hook encounter trigger to load battle scene additively (or align with current transition flow).
- [ ] Ensure return path after battle end is defined (back to field or next scene).

## 6. Audio / Backdrop Switch (Priority: Medium)
- [ ] Connect scene transition with AudioManager BGM switching.
- [ ] Assign backdrop sprites for academy/shion/carlos phases.
- [ ] Verify backdrop selection by battle phase.

## 7. Verification Checklist (Before PR)
- [ ] Battle scene is in Build Settings and loads successfully.
- [ ] Party/enemy slots and command UI appear correctly.
- [ ] Command input (attack/skill/item/defend) executes and updates state.
- [ ] Battle end event is fired and transition behavior is correct.
- [ ] No compile errors in battle MonoBehaviours.
