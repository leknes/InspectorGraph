# InspectorGraph
Extends Unity with the inspector graph, an editor window in which components can be edited, just like in the inspector, but within a graph view with ports representing component references and also support for interface serialization.

## Installation
You can use the inspector graph, by just importing the most recent Unity package from the Releases page

## Getting started
You can find the inspector graph under Window -> General -> Inspector Graph. 

![](https://github.com/user-attachments/assets/10c9e261-4568-4968-8a18-ac225209e9ce)

 
## TODO

* I should put all the code into an assembly definition.
* There is a minor issue, that when adding multiple components they first are stacked on each other.
* Minor error when adding components? (You cannot call GetLast immediately after beginning a group.)
* There is a little issue, that when reloading the graph view, the nodes are not displayed in the right order.
