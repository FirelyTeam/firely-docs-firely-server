.. _vonk_reference_api_elementmodel:

ElementModel, manipulating IResource and ISourceNode
====================================================

As stated in :ref:`vonk_reference_api_iresource`, ``IResource`` is immutable, and so is ``ISourceNode`` that it is based on.
Still, there are cases where you would want to manipulate a resource, e.g. add, change or remove elements. The namespace ``Vonk.Core.ElementModel`` has methods to do so. All of these methods do *NOT* change the original structure (the input IResource or ISourceNode), but instead *return* an updated structure.

.. _vonk_reference_api_isourcenode:

ISourceNode manipulation
------------------------

All the ``ISourceNode`` extension methods can be used on ``IResource`` as well. If you need an ``IResource`` as result, just turn the resulting ``ISourceNode`` to an ``IResource`` again. So if you added an element to an ISourceNode:

.. code-block:: csharp

   IResource result = input.AddIfNotExists(SourceNode.Valued("active", "false")).ToIResource(input.InformationModel);

For some of the more common extension methods we provide an overload on IResource that does this for you, like ``IResource.Patch(...)``

All the methods below are in the namespace ``Vonk.Core.ElementModel.ISourceNodeExtensions``:

.. glossary::
  :sorted:

.. function:: Add(this ISourceNode original, ISourceNode newChild) -> ISourceNode

   Add the ``newChild`` as a child node to the ``original``. It will be added at the end of the Children.

.. function:: Add(this ISourceNode original, ITypedElement newChild) -> ISourceNode
   :no-index:

   Overload of Add(ISourceNode newChild) that lets you add an ITypedElement as new child.

.. function:: AddIf(this ISourceNode original, ISourceNode newChild, Func<ISourceNode, bool> addIf) -> ISourceNode

   Add the ``newChild`` as a child node to the ``original`` if the ``addIf`` predicate on ``original`` is met. It will be added at the end of the Children.

.. function:: Add(this ISourceNode original, TypedElement newChild, Func<ISourceNode, bool> addIf) -> ISourceNode
   :no-index:

   Similar to AddIf(ISourceNode newChild, Func<ISourceNode, bool> addIf) that lets you add an ITypedElement as new child.

.. function:: AddIfNotExists(this ISourceNode original, ISourceNode newChild) -> ISourceNode

   Add the ``newChild`` as a child node to the ``original`` if there is no child with the same name yet. It will be added at the end of the Children.

.. function:: AddIfNotExists(this ISourceNode original, ISourceNode newChild, Func<ISourceNode, bool> exists) -> ISourceNode
   :no-index:

   Add the ``newChild`` as a child node to the ``original`` if the ``exists`` predicate on ``original`` is not satisfied. This is like ``AddIfNotExist(ISourceNode newChild)``, but here you get to specify what 'exists' means. It will be added at the end of the Children.

.. function:: AddIfNotExists(this ISourceNode original, string location, ISourceNode newChild) -> ISourceNode
   :no-index:

   Navigate to ``location``. Then add the ``newChild`` as a child node to the ``original`` if there is no child with the same name yet.

.. function:: AddIfNotExists(this ISourceNode original, ISourceNode newChild, string location, Func<ISourceNode, bool> exists) -> ISourceNode
   :no-index:

   Navigate to ``location``. Then add the ``newChild`` as a child node if the ``exists`` predicate on the current node is not satisfied.

.. function:: AnnotateWith<T>(this ISourceNode original, T annotation, bool hideExisting = false) -> ISourceNode

   Add an annotation of type T to the ``original``. When hideExisting == true, any existing annotations of type T are not visible anymore on the returned ISourceNode.

.. function:: GetBoundAnnotation<T>(this ISourceNode original, ) where T : class, IBoundAnnotation -> T

   Retrieve an annotation that is bound directly to ``original``, not to any of the nodes it may decorate.
   (ISourceNode is immutable, to changes are usually a pile of wrappers around the ``original`` SourceNode, and each of the wrappers can add / replace annotations.)

.. function:: RemoveEmptyNodes(this ISourceNode original, ISourceNode newChild) -> ISourceNode

   Remove any nodes that have no value or children. This happens recursively: if a node has only children with empty values, it will be removed as well. This way the returned ISourceNode conforms to the invariant in the FHIR specification that an element either has a value or children.

.. function:: RemoveEmptyNodes(this ISourceNode original, ISourceNode newChild, string location) -> ISourceNode
   :no-index:

   Remove any nodes that have no value or children, from the specified ``location`` downwards. This happens recursively: if a node has only children with empty values, it will be removed as well.

.. function:: Child(this ISourceNode original, string name, int arrayIndex = 0) -> ISourceNode

   Convenience method to get the child with name ``name`` at position ``arrayIndex``. Usually used to get a child of which you know there is only one: ``patientNode.Child("active")``

.. function:: ChildString(this ISourceNode original, string name, int arrayIndex = 0) -> ISourceNode

   Convenience method to get the value of the child with name ``name`` at position ``arrayIndex``. Usually used to get a child of which you know there is only one: ``patientNode.ChildString("id")``

.. function:: ForceAdd(this ISourceNode original, string addAt, ISourceNode newChild) -> ISourceNode

   Add the ``newChild`` at location ``addAt``. Create the intermediate nodes if necessary .

.. function:: AddOrReplace(this ISourceNode original, Func<ISourceNode, bool> match, ISourceNode toAdd, Func<ISourceNode, ISourceNode> replace) -> ISourceNode

   Find any child nodes of ``original`` that match the ``match`` predicate. Apply ``replace`` to them.
   If none are found, add ``toAdd`` as new child.

.. function:: AddOrReplace(this ISourceNode original, ISourceNode toAdd, Func<ISourceNode, ISourceNode> replace) -> ISourceNode
    :no-index:

    Optimized overload of the previous method for matching on the node name.
    It will perform ``replace`` on any child node of ``original`` with the same name as ``toAdd``.
    If none are found it will add ``toAdd`` as new child node.

.. function:: Remove(this ISourceNode original, string location) -> ISourceNode

   Remove the node at ``location``, if any.
   If that results in parent nodes becoming empty (no Text, no Children), those are removed as well.

.. function:: SelectNodes(this ISourceNode original, string fhirPath) -> IEnumerable<ISourceNode>

   Run ``fhirPath`` over the ``original``, but with the limitations of untyped nodes. It will return the matching nodes.
   Use valueDateTime/valueBoolean instead of just 'value' for choice types.
   Only use this method if you are familiar with the differences in the naming of nodes between ISourceNode and ITypedElement.


.. function:: SelectText(this ISourceNode original, string fhirPath) -> string

   Run ``fhirPath`` over the ``original``, but with the limitations of untyped nodes. Returns the ``Text`` of the first matching node.
   Use valueDateTime/valueBoolean instead of just 'value' for choice types.
   Only use this method if you are familiar with the differences in the naming of nodes between ISourceNode and ITypedElement.

.. function:: Patch(this ISourceNode original, string location, Func<ISourceNode, ISourceNode> patch) -> ISourceNode

   Find any nodes at ``location`` and apply ``patch`` to them. For ``patch`` you can use other methods listed here like ``Rename``, ``Add`` or ``Revalue``. ``location`` is evaluated as a fhirpath statement, with the limitations of untyped nodes.

.. function:: Patch(this ISourceNode original, string[] locations, Func<ISourceNode, ISourceNode> patch) -> ISourceNode
   :no-index:

   Find any nodes having one of the ``locations`` as their Location and apply ``patch`` to them.
   If you don't know exact locations, use ``original.Patch(location, patch)``, see above.

.. function:: ForcePatch(this ISourceNode original, string forcePath, Func<ISourceNode, ISourceNode> patch) -> ISourceNode

   Enforce that ``forcePath`` exists. Then patch the resulting node(s) with ``patch``.

.. function:: ForcePatchAt(this ISourceNode original, string fromLocation, string forcePath, Func<ISourceNode, ISourceNode> patch) -> ISourceNode

   For each node matching the ``fromLocation``: enforce that ``fromLocation.forcePath`` exists, then patch the resulting node(s) with ``patch``.
   E.g. someBundle.ForcePatchAt("entry", "request", node => node.Add(SourceNode.Valued("url", "someUrl"))
   will add request.url with value "someUrl" to every entry.

.. function:: Relocate(this ISourceNode original, string newLocation) -> ISourceNode

   Set ``original.Location`` to the newLocation, and update all its descendants' ``Location`` properties recursively.

.. function:: Rename(this ISourceNode original, string newName) -> ISourceNode

   Set ``original.Name`` to the ``newName``.

.. function:: Revalue(this ISourceNode original, string newValue) -> ISourceNode

   Set ``original.Text`` to ``newValue``.

.. function:: Revalue(this ISourceNode original, Dictionary<string, string> replacements) -> ISourceNode
   :no-index:

   ``replacements`` is a dictionary of location + newValue. On each matching location under ``original``, the value will be set to the according newValue from ``replacements``.

.. function:: AnnotateWithSourceNode(this ISourceNode original) -> ISourceNode

   Add ``original`` as annotation to itself. Very specific use case.

.. _vonk_reference_api_itypedelement:

ITypedElement manipulation
--------------------------

All the methods below are in the namespace ``Vonk.Core.ElementModel.ITypedElementExtensions``:

.. function:: Add(this ITypedElement original, ITypedElement newChild, Func<ITypedElement, bool> addIf) -> ISourceNode
   :no-index:

   Add ``newChild`` as child to ``original`` if ``addIf`` on ``original`` evaluates to true.
   Convenience overload of ``ISourceNodeExtensions.Add(ISourceNode, ITypedElement, Func<ISourceNode, bool>)``

.. function:: Add(this ITypedElement original, ITypedElement newChild) -> ISourceNode
   :no-index:

   Add ``newChild`` as child to ``original``.
   Convenience overload of ``ISourceNodeExtensions.Add(ISourceNode, ITypedElement)``

.. function:: AddIfNotExists(this ITypedElement original, ITypedElement newChild) -> ISourceNode
   :no-index:

   Add ``newChild`` as child to ``original`` if no child with the same name exists yet.
   Convenience overload of ``ISourceNodeExtensions.AddIfNotExists(ISourceNode, ITypedElement)``

.. function:: AddIf(this ITypedElement original, ISourceNode newChild, Func<ITypedElement, bool> addIf) -> ISourceNode
   :no-index:

   Add ``newChild`` as child to ``original`` if ``addIf`` on ``original`` evaluates to true.
   Convenience overload of ``ISourceNodeExtensions.AddIf(ISourceNode, ISourceNode, Func<ISourceNode, bool>)``

.. :function:: Add(this ITypedElement original, ISourceNode newChild)

   Add ``newChild`` as child to ``original``.

.. :function:: AddIfNotExists(this ITypedElement original, ISourceNode newChild)

   Add ``newChild`` as child to ``original`` if no child with the same name exists yet.
   Convenience overload of ``AddIfNotExists(ITypedElement, ITypedElement)``

.. function:: Cache(this ITypedElement original) -> ITypedElement

   Prevent recalculation of the Children upon every access.

.. function:: Child(this ITypedElement element, string name, int arrayIndex = 0) -> ITypedElement
   :no-index:

   Returns n-th child with the specified ``name``, if any.

.. function:: ChildString(this ITypedElement element, string name, int arrayIndex = 0) -> string
   :no-index:

   Returns the value of the n-th child with the specified ``name`` as string, if any.

.. function:: DefinitionSummary(this ITypedElement element, IStructureDefinitionSummaryProvider provider) -> IStructureDefinitionSummary

   Returns the summary for the actual type of the element. Especially useful if the element is of a choicetype.

.. function:: AddParent(this ITypedElement element) -> ITypedElement

   Add ``Vonk.Core.ElementModel.IParentProvider`` annotations to ``element`` and its descendants.

.. function:: GetParent(this ITypedElement element) -> ITypedElement

   Get the parent of this element, through the ``Vonk.Core.ElementModel.IParentProvider`` annotation (if present).

.. function:: AddTreePath(this ITypedElement element) -> ITypedElement

   Add the ``Vonk.Core.ElementModel.ITreePathGenerator`` annotation. TreePath is the Location without any indexes (no [n] at the end).

.. function:: GetTreePath(this ITypedElement element) -> string

   Get the value of the ``Vonk.Core.ElementModel.ITreePathGenerator`` annotation, if present. TreePath is the Location without any indexes (no [n] at the end).
