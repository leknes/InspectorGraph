# InspectorGraph
Extends Unity with the inspector graph, an editor window in which components can be edited, just like in the inspector, but within a graph view with ports representing component references and also support for interface serialization.

![Example Picture](https://github.com/user-attachments/assets/9fd8d9e3-40d0-49aa-90ed-5f86e1e3f84f)

## Installation
You can use the inspector graph, by just importing the most recent Unity package from the Releases page (https://github.com/leknes/InspectorGraph/releases/tag/v1.1.0).

## Getting started
You can find the inspector graph under Window -> General -> Inspector Graph. Then, using it should be quite intutive, as you can just drag around nodes resembling components, and even connect component references with ports, just like in the Unity Shader Graph. This also includes lists and arrays of component. Also you can expand/collapse the nodes, to hide component intrinsics. Nicely, whether a components is collapsed or not, and where the components has been dragged to, is saved to the project settings, so that the inspector graph stays consistent even after exiting the editor.


https://github.com/user-attachments/assets/ba99f59d-2bfe-4f49-8540-1020796ba1d1


## Interface serialization

Through the integrated `Interface<>` wrapper class, or the `InterfaceAttribute` attribute from the `Leknes.InterfaceSerialization` namespace, it is also possible to display any interface both in the inspector and in the inspector graph. 

When using the attribute this can look just like this:

`[SerializeField, Interface(typeof(ISomeInterface))] private Object _someObject;`
`[Interface(typeof(ISomeInterface))] public Object[] _someArray;`

`((ISomeInterface)_someObject).SomeInterfaceMethod();`

And the wrapper class meanwhile can be used the following:

`[SerializeField] private Interface<ISomeInterface> _someObject;`
`public Interface<ISomeInterface>[] _someArray;`

`_someObject.Value.SomeInterfaceMethod();`
