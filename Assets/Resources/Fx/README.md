# Spell effect prefabs

Drop a Unity Particle System (or any prefab) here to replace the
generated element particles. Play looks up:

`Assets/Resources/Fx/{Family}{Kind}.prefab`

then

`Assets/Resources/Fx/{Family}.prefab`

| Family | Kind |
|---|---|
| Fire, Flame, Water, Ice, Earth, Air, Lightning, Spark, Fog, Poison, Plant, Dark, Light, Steam, Lava | Burst, Stream, Linger |

Examples: `Fx/FireBurst`, `Fx/FireStream`, `Fx/Ice`. A family prefab is used when the kind file is missing.

The generated particles stay as the fallback when nothing is here.
