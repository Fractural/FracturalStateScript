# Fractural State Script 📜

Fratural State Script is a Godot 3.x C# addon that adds state scripts into Godot. This is based off of Overwatch's implementation.

See Dan Reed's GDC talk on [Networking Scripted Weapons and Abilities in Overwatch](https://www.youtube.com/watch?v=5jP0z7Atww4&t=553s).

## Dependencies
- GodotRollbackNetcode - Snopek games' rollback netcode addon in GDScript
- GodotRollbackNetcodeMono - C# wrapper for Snpoek games' addon
- FracturalInject - Dependency injection plugin for Godot
- FracturalCommmons - Common utility classes for Godot game + plugin development

## Architecture

Gameplay logic is written in StateScript, a visual node-based scripting langauge. When a StateScript graph is ran, the logic is ran. A StateScript graph is stored as a node in the scene, and StateScript nodes are stored as children of the StateScript. A StateScript graph itself is also a StateScript node, meaning StateScripts can be used within other StateScripts, creating a tree like structure of nodes. 

Each node is implemented in C# by the user, and should represent simple behaviours, such as waiting, shooting, etc. StateScript provides lifetime guarantees. This means each node cleans up after itself, including when the state is rolled back.

Types of Nodes
- Entry - An entry into the state. A StateScript can have multiple entry point
- State - Node that does something over time, such as waiting, etc.
- Action - Node that does something immediately.

### Node Variables

State and Action nodes have settings that are configured by Node Variables, or NodeVars for short. NodeVars are Godot nodes that represent a variable. For example there are boolean, float, integer, and string NodeVars. These NodeVars implement `FracturalInject`'s Dependency under the hood, so they can both represent some initial value or point to another NodeVar.

There are two types of NodeVars
- Get/Set - The NodeVar can be read and written to from the outside. 
- Get - The NodeVar can only be read from the outside. Only the State/Action node itself can modify the NodeVar.
- Set - The NodeVar can be set outside