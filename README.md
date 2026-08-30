![Descrption](bunblade.gif)

# Bunblade

A turn-based RPG battle system built in Unity, combining classic elemental combat
with a real-time parry mechanic — timing matters even in a turn-based fight.

## Core Systems

- **Elemental combat** — Fire, Ice, Water, and Earth elements with per-character
  weaknesses and resistances that affect damage calculations.
- **Real-time parrying** — incoming attacks resolve through a windup → parry
  window → late-hit sequence, with early/perfect/late timing detected frame by
  frame and rewarded with different damage multipliers.
- **Custom turn scheduler** — turn order is driven by a min-heap I implemented
  from scratch (`Algorithms (custom data structures)/heap.cs`) rather than a
  built-in priority queue, converting each actor's speed stat into a time value
  so faster characters act sooner.
- **Spells & mana** — `ScriptableObject`-based spell system with mana costs,
  elemental typing, and stacking effects.
- **Shop system** — a between-battle shop for spending gold on attack, defense,
  and ability power upgrades.

## Tech

- Unity 6 (6000.2.14f1)
- C#
- TextMesh Pro for UI text

## Status

In development — core battle loop (turn order, elemental damage, parry
resolution, shop) is functional.
