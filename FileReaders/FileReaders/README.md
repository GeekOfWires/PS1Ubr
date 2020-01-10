# A set of basic tools to read the different PS1 files.


**MPOReader.cs**

This is used to read the supplemental map information such as facility names

**LSTReader.cs**

Reads the objects_map*.lst files which contain a list of lattice links between facilities and warpgates

**GameObjectsReader.cs**

Reads the game_objects*.lst files which contains a huge amount of configuration for game entities, including but not limited to weapons, vehicles, maps, facilities etc

**ZplReader.cs**

Reads the ugd*.zpl files which contain definitions for ziplines and teleporters (which are essentially ziplines with two points and a special zip_is_teleporter flag)