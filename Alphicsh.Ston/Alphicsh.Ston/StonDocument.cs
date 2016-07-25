using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    using Equivalence;
    using Tokenization;

    using IndexBinding = KeyValuePair<IStonBindingIndex, IStonEntity>;

    /// <summary>
    /// Represents a STON document.
    /// </summary>
    public sealed class StonDocument : IStonDocument
    {
        /// <summary>
        /// Gets the STON document core.
        /// </summary>
        public IStonValuedEntity Core { get; }

        #region Constructors

        /// <summary>
        /// Builds a STON document using a copy of a given entity as a source.
        /// </summary>
        /// <param name="coreSource">The entity to copy core's structure from.</param>
        public StonDocument(IStonValuedEntity coreSource) : this(coreSource, null, null, null, null) { }

        /// <summary>
        /// Builds a STON document using a copy of a given entity as a source, with whitelists of known application extensions.
        /// </summary>
        /// <param name="coreSource">The entity to copy core's structure from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        public StonDocument(IStonValuedEntity coreSource, IEnumerable<string> knownApplicationExtensionTypes, IEnumerable<string> knownApplicationExtensionMembers)
            : this(coreSource, knownApplicationExtensionTypes, knownApplicationExtensionMembers, null, null) { }

        /// <summary>
        /// Builds a STON document using a copy of a given entity as a source, with rules determining valid application extension names.
        /// </summary>
        /// <param name="coreSource">The entity to copy core's structure from.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        public StonDocument(IStonValuedEntity coreSource, Func<string, bool> extensionTypesRule, Func<string, bool> extensionMembersRule)
            : this(coreSource, null, null, extensionTypesRule, extensionMembersRule) { }

        /// <summary>
        /// Builds a STON document using a copy of a given entity as a source, with whitelists of known application extensions and rules determining valid application extension names.
        /// </summary>
        /// <param name="coreSource">The entity to copy core's structure from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        public StonDocument(
            IStonValuedEntity coreSource,
            IEnumerable<string> knownApplicationExtensionTypes, IEnumerable<string> knownApplicationExtensionMembers,
            Func<string, bool> extensionTypesRule, Func<string, bool> extensionMembersRule
            )
        {
            Core = StonValuedEntity.Copy(coreSource);
            GlobalEntities.Add("", Core);

            var resolver = new DocumentResolver(
                this,
                knownApplicationExtensionTypes ?? Enumerable.Empty<string>(), knownApplicationExtensionMembers ?? Enumerable.Empty<string>(),
                extensionTypesRule ?? (s => false), extensionMembersRule ?? (s => false)
                );
            resolver.Resolve();

            GlobalEntities.Remove("");
        }

        #endregion

        #region Hierarchy of contexts

        /// <summary>
        /// Gets the parent (defining) context of a given entity context, as defined in the hierarchy of contexts.
        /// </summary>
        /// <param name="context">The entity context whose parent should be found.</param>
        /// <returns>The parent (defining) context of the entity context.</returns>
        public IStonComplexEntity GetParentContext(IStonEntity context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return Parents[context];
        }

        // backing source of contexts parents
        private IDictionary<IStonEntity, IStonComplexEntity> Parents { get; } = new Dictionary<IStonEntity, IStonComplexEntity>();

        #endregion

        #region Globally identified entities

        /// <summary>
        /// Gets the globally identified entity with a given name.
        /// </summary>
        /// <param name="globalIdentifier">The identifier of the entity.</param>
        /// <returns>The globally identified entity.</returns>
        public IStonEntity GetGlobalEntity(string globalIdentifier) => GlobalEntities[globalIdentifier];

        // backing source of globally identified entities
        private IDictionary<string, IStonEntity> GlobalEntities { get; } = new Dictionary<string, IStonEntity>();

        #endregion

        #region Members access

        /// <summary>
        /// Gets a member entity of a complex entity by a given name or index. If the key is not present, returns null.
        /// </summary>
        /// <param name="entity">The entity to get the member of.</param>
        /// <param name="memberKey">The name or index of the member.</param>
        /// <returns>The member entity associated with the name or index.</returns>
        public IStonEntity GetMember(IStonComplexEntity entity, IStonBindingKey memberKey)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (memberKey == null) throw new ArgumentNullException("memberKey");
            if (!MemberMappings.ContainsKey(entity)) return null; 
            var entityMappings = MemberMappings[entity];
            IStonEntity result;
            return entityMappings.TryGetValue(memberKey, out result) ? result : null;
        }

        // backing source of the member mappings
        private IDictionary<IStonComplexEntity, IDictionary<IStonBindingKey, IStonEntity>> MemberMappings { get; }
            = new Dictionary<IStonComplexEntity, IDictionary<IStonBindingKey, IStonEntity>>();

        #endregion

        #region Resolved references

        /// <summary>
        /// Gets the value referenced by a given reference entity from the document.
        /// </summary>
        /// <param name="reference">The reference entity.</param>
        /// <returns>The referenced value.</returns>
        public IStonValuedEntity GetReferencedValue(IStonReferenceEntity reference)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            if (ReferenceMappings.ContainsKey(reference)) return ReferenceMappings[reference];
            else return null;
        }

        // backing source of the resolved references mappings
        private IDictionary<IStonReferenceEntity, IStonValuedEntity> ReferenceMappings { get; }
            = new Dictionary<IStonReferenceEntity, IStonValuedEntity>();

        #endregion

        #region Construction order

        /// <summary>
        /// Enumerates the construction order of the document's valued entities.
        /// </summary>
        /// <returns>One of valid construction orders.</returns>
        public IEnumerable<IStonValuedEntity> GetConstructionOrder()
        {
            return GetConstructionOrder(Core, new HashSet<IStonValuedEntity>());
        }

        // gets construction order for the value of a given entity
        private IEnumerable<IStonValuedEntity> GetConstructionOrder(IStonEntity entity, HashSet<IStonValuedEntity> constructed) => GetConstructionOrder(GetValue(entity), constructed);

        // gets construction order for the given value entity
        private IEnumerable<IStonValuedEntity> GetConstructionOrder(IStonValuedEntity entity, HashSet<IStonValuedEntity> constructed)
        {
            if (constructed.Contains(entity)) yield break;

            // simple entities are created just like that
            if (entity is IStonSimpleEntity)
            {
                constructed.Add(entity);
                yield return entity;
                yield break;
            }
            var complexEntity = entity as IStonComplexEntity;

            // dependencies
            if (complexEntity.Construction != null)
            {
                foreach (var parameter in complexEntity.Construction.PositionalParameters)
                {
                    // ppce = positional parameter constructed entity
                    foreach (var ppce in GetConstructionOrder(parameter, constructed)) yield return ppce;
                }
                foreach (var namedParameter in complexEntity.Construction.NamedParameters)
                {
                    // npce = named parameter constructed entity
                    foreach (var npce in GetConstructionOrder(namedParameter.Value, constructed)) yield return npce;
                }
            }

            // entity itself
            constructed.Add(complexEntity);
            yield return complexEntity;

            // member indices and values
            if (complexEntity.MemberInit != null)
            {
                foreach (var memberBinding in complexEntity.MemberInit.MemberBindings)
                {
                    if (memberBinding.Key is IStonBindingIndex)
                    {
                        foreach (var parameter in (memberBinding.Key as IStonBindingIndex).Parameters)
                        {
                            // pce = parameter constructed entity
                            foreach (var pce in GetConstructionOrder(parameter, constructed)) yield return pce;
                        }
                    }
                    // vce = value constructed entity
                    foreach (var vce in GetConstructionOrder(memberBinding.Value, constructed)) yield return vce;
                }
            }

            // collections
            if (complexEntity.CollectionInit != null)
            {
                foreach (var element in complexEntity.CollectionInit.Elements)
                {
                    // eve = element constructed entity
                    foreach (var ece in GetConstructionOrder(element, constructed)) yield return ece;
                }
            }
        }

        #endregion

        // gets a value represented by a STON entity
        // entity itself it it's a valued entity
        // or a referenced value if it's a reference entity
        private IStonValuedEntity GetValue(IStonEntity entity)
        {
            if (entity is IStonValuedEntity) return entity as IStonValuedEntity;
            else return GetReferencedValue(entity as IStonReferenceEntity);
        }

        // a class for processing and building a STON document before making it officially complete
        private class DocumentResolver
        {
            // the processed document itself
            private StonDocument Document { get; }

            // allowed application-side extensions
            private IEnumerable<string> KnownApplicationExtensionTypes { get; }
            private IEnumerable<string> KnownApplicationExtensionMembers { get; }

            private Func<string, bool> ExtensionTypesRule { get; }
            private Func<string, bool> ExtensionMembersRule { get; }

            // reference entities whose referenced value are yet unknown
            private ICollection<IStonReferenceEntity> UnresolvedReferences { get; } = new List<IStonReferenceEntity>();

            // member bindings whose keys have not been yet decided
            private IDictionary<IStonComplexEntity, IList<IList<IndexBinding>>> UnresolvedMembers { get; }
                = new Dictionary<IStonComplexEntity, IList<IList<IndexBinding>>>();

            // indicate whether specific complex entities define single reference indices, implicitly typed numbers indices or implicitly typed binary indices
            // for the purpose of determining whether the index path segment can be used as collection element path segment or not
            private IDictionary<IStonComplexEntity, bool> DefinesReferenceIndices { get; } = new Dictionary<IStonComplexEntity, bool>();
            private IDictionary<IStonComplexEntity, bool> DefinesImplicitNumberIndices { get; } = new Dictionary<IStonComplexEntity, bool>();
            private IDictionary<IStonComplexEntity, bool> DefinesImplicitBinaryIndices { get; } = new Dictionary<IStonComplexEntity, bool>();

            // compares the binding keys
            private IStonBindingKeyEquivalenceComparer BindingKeyComparer { get; }

            // creates a new resolver for a given document
            public DocumentResolver(
                StonDocument document,
                IEnumerable<string> knownApplicationExtensionTypes, IEnumerable<string> knownApplicationExtensionMembers,
                Func<string, bool> extensionTypesRule, Func<string, bool> extensionMembersRule
                )
            {
                Document = document;
                BindingKeyComparer = new StonBindingKeyEquivalenceComparer(new StonSemanticEntityEquivalenceComparer(Document));

                KnownApplicationExtensionTypes = knownApplicationExtensionTypes;
                KnownApplicationExtensionMembers = knownApplicationExtensionMembers;

                ExtensionTypesRule = extensionTypesRule;
                ExtensionMembersRule = extensionMembersRule;
            }

            // resolves the document based on the earlier provided data
            public void Resolve()
            {
                RegisterEntity(Document.Core, null);
                ResolveReferences();
                ResolveMembers();
                VerifyConstructionOrder();
            }

            // registering entities
            // resolving hierarchy of contexts
            // checking for unknown application extensions
            // listing reference entities to resolve

            #region Entity registration

            // registers the entitity in the hierarchy of contexts
            // checks its validity
            // and possibly creates additional structures used for resolving the document
            private void RegisterEntity(IStonEntity entity, IStonComplexEntity parent)
            {
                Document.Parents[entity] = parent;
                if (entity.GlobalIdentifier != null)
                {
                    if (Document.GlobalEntities.ContainsKey(entity.GlobalIdentifier)) throw new StonDuplicateGlobalEntityException(Document.GlobalEntities[entity.GlobalIdentifier], entity);
                    Document.GlobalEntities.Add(entity.GlobalIdentifier, entity);
                }

                if (entity is IStonValuedEntity)
                {
                    CheckTypeExtensions((entity as IStonValuedEntity).Type);

                    if (entity is IStonComplexEntity) RegisterEntity(entity as IStonComplexEntity, parent);
                }
                else if (entity is IStonReferenceEntity) RegisterEntity(entity as IStonReferenceEntity, parent);
            }

            // registers a complex entity in the hierarchy of contexts
            private void RegisterEntity(IStonComplexEntity entity, IStonComplexEntity parent)
            {
                // variables affecting whether an indexed member path segment can be used as collection element path segment
                DefinesReferenceIndices[entity] = false;
                DefinesImplicitBinaryIndices[entity] = false;
                DefinesImplicitNumberIndices[entity] = false;

                // registering entities defined in the complex entity's components

                if (entity.Construction != null) RegisterConstruction(entity, parent);

                var resolved = new Dictionary<IStonBindingKey, IStonEntity>(BindingKeyComparer);
                Document.MemberMappings.Add(entity, resolved);

                if (entity.MemberInit != null) RegisterMemberInit(entity, resolved);

                if (entity.CollectionInit != null) RegisterCollectionInit(entity);
            }

            // registering entities in construction
            // and building construction dependencies data at the same time
            private void RegisterConstruction(IStonComplexEntity entity, IStonComplexEntity parent)
            {
                var dependencies = new List<IStonEntity>();
                foreach (var parameter in entity.Construction.PositionalParameters)
                {
                    RegisterEntity(parameter, parent);
                    if (parameter is IStonReferenceEntity) dependencies.Add(parameter);
                    if (parameter is IStonComplexEntity && ConstructionDependencies.ContainsKey(parameter as IStonComplexEntity)) dependencies.Add(parameter);
                }

                foreach (var namedParameter in entity.Construction.NamedParameters)
                {
                    var parameter = namedParameter.Value;
                    RegisterEntity(parameter, parent);
                    if (parameter is IStonReferenceEntity) dependencies.Add(parameter);
                    if (parameter is IStonComplexEntity && ConstructionDependencies.ContainsKey(parameter as IStonComplexEntity)) dependencies.Add(parameter);
                }
                if (dependencies.Any()) ConstructionDependencies.Add(entity, dependencies);
            }

            // registering entities in member init
            private void RegisterMemberInit(IStonComplexEntity entity, IDictionary<IStonBindingKey, IStonEntity> resolved)
            {
                var unresolved = new List<IList<IndexBinding>>();
                UnresolvedMembers.Add(entity, unresolved);

                foreach (var memberBinding in entity.MemberInit.MemberBindings)
                {
                    // binding names are checked for extension validity
                    // also, they are instantly resolved
                    if (memberBinding.Key is IStonBindingName)
                    {
                        var bindingName = memberBinding.Key as IStonBindingName;
                        if (bindingName.IsExtension && !KnownApplicationExtensionMembers.Contains(bindingName.Name) && !ExtensionMembersRule(bindingName.Name)) throw new StonUnknownExtensionMemberException(bindingName);
                        if (resolved.ContainsKey(memberBinding.Key))
                        {
                            throw new StonDuplicateMemberException(entity, Document, resolved.Keys.First(k => BindingKeyComparer.Equals(k, memberBinding.Key)), memberBinding.Key);
                        }
                        resolved.Add(memberBinding.Key, memberBinding.Value);
                    }
                    // binding indices are resolved only when their references are resolved
                    // at the beginning, no reference entity is resolved
                    else if (memberBinding.Key is IStonBindingIndex)
                    {
                        var bindingIndex = memberBinding.Key as IStonBindingIndex;

                        bool hasReference = false;
                        foreach (var parameter in bindingIndex.Parameters)
                        {
                            // index parameters must have their entities registered, too
                            RegisterEntity(parameter, entity);
                            if (parameter is IStonReferenceEntity) hasReference = true;
                        }

                        // indices with no references as parameters are immediately resolved
                        if (!hasReference)
                        {
                            if (resolved.ContainsKey(memberBinding.Key))
                            {
                                throw new StonDuplicateMemberException(entity, Document, resolved.Keys.First(k => BindingKeyComparer.Equals(k, memberBinding.Key)), memberBinding.Key);
                            }
                            resolved.Add(memberBinding.Key, memberBinding.Value);
                        }
                        // indices with references as parameters must be resolved later
                        else
                        {
                            var idx = bindingIndex.Parameters.Count();
                            while (unresolved.Count <= idx) unresolved.Add(new List<IndexBinding>());
                            unresolved[idx].Add(new IndexBinding(bindingIndex, memberBinding.Value));
                        }

                        // single-parameter indices can affect usage of index path segments as collection element path segments
                        if (!bindingIndex.Parameters.Skip(1).Any())
                        {
                            var parameter = bindingIndex.Parameters.First();
                            if (parameter is IStonReferenceEntity) DefinesReferenceIndices[entity] = true;
                            else if (parameter is IStonSimpleEntity && (parameter as IStonSimpleEntity).Type == null)
                            {
                                var parameterValue = (parameter as IStonSimpleEntity).Value;
                                if (parameterValue.DataType == StonDataType.Number) DefinesImplicitNumberIndices[entity] = true;
                                if (parameterValue.DataType == StonDataType.Binary) DefinesImplicitBinaryIndices[entity] = true;
                            }
                        }
                    }

                    // registering the value
                    RegisterEntity(memberBinding.Value, entity);
                }
            }

            // registering entities in collection init
            private void RegisterCollectionInit(IStonComplexEntity entity)
            {
                foreach (var element in entity.CollectionInit.Elements)
                {
                    RegisterEntity(element, entity);
                }
            }

            // checks whether the given type contains no unknown extension types
            private void CheckTypeExtensions(IStonType type)
            {
                if (type is IStonNamedType)
                {
                    var namedType = type as IStonNamedType;
                    if (namedType.IsExtension && !KnownApplicationExtensionTypes.Contains(namedType.Name) && !ExtensionTypesRule(namedType.Name)) throw new StonUnknownExtensionTypeException(namedType);
                    foreach (var parameter in namedType.TypeParameters) CheckTypeExtensions(parameter);
                }
                else if (type is IStonCollectionType)
                {
                    CheckTypeExtensions((type as IStonCollectionType).ElementType);
                }
                else if (type is IStonUnionType)
                {
                    foreach (var permittedType in (type as IStonUnionType).PermittedTypes) CheckTypeExtensions(permittedType);
                }
            }

            // registers a reference entity in the hierarchy of contexts
            private void RegisterEntity(IStonReferenceEntity entity, IStonComplexEntity parent)
            {
                foreach (var segment in entity.Address.RelativePath)
                {
                    // binding indices must have their inner entities registered
                    if (segment is IStonMemberPathSegment && (segment as IStonMemberPathSegment).BindingKey is IStonBindingIndex)
                    {
                        var indexBinding = (segment as IStonMemberPathSegment).BindingKey as IStonBindingIndex;
                        foreach (var parameter in indexBinding.Parameters)
                        {
                            RegisterEntity(parameter, parent);
                        }
                    }
                    // collection element path segment must have its element index entity registered
                    else if (segment is IStonCollectionElementPathSegment)
                    {
                        var elementIndex = (segment as IStonCollectionElementPathSegment).ElementIndex;
                        RegisterEntity(elementIndex, parent);
                    }
                }

                // at the beginning, all reference entities are unresolved
                UnresolvedReferences.Add(entity);
            }

            #endregion

            // resolving reference entities

            #region Reference resolving

            // resolves all earlier registered references
            private void ResolveReferences()
            {
                while (UnresolvedReferences.Any())
                {
                    var reference = UnresolvedReferences.First();
                    if (GetReferencedValue(reference) == null)
                    {
                        var state = ReferenceStates[reference];
                        throw state.MakeUnknownReferenceException();
                    }
                }
            }

            // keeps track of references that started being resolved
            // but have yet to proceed through all path segments
            private Dictionary<IStonReferenceEntity, ReferenceState> ReferenceStates { get; } = new Dictionary<IStonReferenceEntity, ReferenceState>();

            // tracks resolving of a single reference
            // in particular, the most recent context reached
            // the current path segment to resolve
            // and optionally the next destination reference
            private class ReferenceState
            {
                internal IStonReferenceEntity Reference { get; }

                internal IStonValuedEntity Context { get; set; }
                internal IEnumerator<IStonPathSegment> RemainingSegments { get; }
                internal IStonReferenceEntity NextContextReference { get; set; }

                internal bool Incomplete { get; set; }

                // starts tracking resolving of the given reference entity
                internal ReferenceState(IStonReferenceEntity reference)
                {
                    Reference = reference;

                    Context = null;
                    RemainingSegments = (new IStonPathSegment[] { null }).Concat(Reference.Address.RelativePath).GetEnumerator();
                    RemainingSegments.MoveNext();
                    NextContextReference = null;

                    Incomplete = true;
                }

                // makes an unknown reference exception for the current state
                internal StonUnknownReferenceException MakeUnknownReferenceException() => new StonUnknownReferenceException(Reference, Context, RemainingSegments.Current);

                // makes an unknown reference exception for the current state, with a given message
                internal StonUnknownReferenceException MakeUnknownReferenceException(string message) => new StonUnknownReferenceException(Reference, Context, RemainingSegments.Current, message);
            }

            // keeps track of reference chains
            // which allows finding inevitable circular references
            private class ReferenceBranch
            {
                private IStonReferenceEntity Tail { get; set; }
                private IList<KeyValuePair<IStonReferenceEntity, IStonPathSegment>> _Path { get; }
                private HashSet<IStonReferenceEntity> _FoundReferences { get; }

                // creates a new reference branch, starting from a given reference entity
                public ReferenceBranch(IStonReferenceEntity reference)
                {
                    Tail = reference;
                    _Path = new List<KeyValuePair<IStonReferenceEntity, IStonPathSegment>>();
                    _FoundReferences = new HashSet<IStonReferenceEntity>();
                }

                // adds a new reference to the chain
                // and throws circular reference exception if applicable
                public void PushOrFail(IStonPathSegment segment, IStonReferenceEntity reference)
                {
                    _Path.Add(new KeyValuePair<IStonReferenceEntity, IStonPathSegment>(Tail, segment));
                    _FoundReferences.Add(Tail);
                    Tail = reference;

                    if (_FoundReferences.Contains(Tail)) throw new StonCircularReferenceException(_Path.SkipWhile(kvp => kvp.Key != Tail));
                }

                // removes the latest reference from the chain
                public void Pop()
                {
                    Tail = _Path.Last().Key;
                    _FoundReferences.Remove(Tail);
                    _Path.RemoveAt(_Path.Count - 1);
                }
            }

            // retrieves the value referenced by a given entity
            // it might be considered in a context of a specific reference branch
            private IStonValuedEntity GetReferencedValue(IStonReferenceEntity reference, ReferenceBranch branch = null)
            {
                // the value has been already resolved
                if (Document.ReferenceMappings.ContainsKey(reference)) return Document.ReferenceMappings[reference];

                // the value is yet to be resolved, but is already processed
                // following that branch would lead to circular reference
                if (!UnresolvedReferences.Contains(reference)) return null;

                // processing the entity, possibly opening a new branch and state
                // the branch is used for tracking references chains and loops
                UnresolvedReferences.Remove(reference);
                branch = branch ?? new ReferenceBranch(reference);
                if (!ReferenceStates.ContainsKey(reference)) ReferenceStates.Add(reference, new ReferenceState(reference));
                var state = ReferenceStates[reference];

                // going through the state, step by step
                while (state.Incomplete)
                {
                    if (!ResolveStep(state, branch))
                    {
                        // if the entity cannot be resolved yet
                        // it's left for later processing
                        UnresolvedReferences.Add(reference);
                        return null;
                    }
                }

                // adding the found entity
                ReferenceStates.Remove(reference);
                Document.ReferenceMappings.Add(reference, state.Context);
                return state.Context;
            }

            // tries to advance the reference state by one context (path segment)
            private bool ResolveStep(ReferenceState state, ReferenceBranch branch)
            {
                if (state.NextContextReference != null) return JumpTo(state, branch, state.NextContextReference);
                else if (state.RemainingSegments.Current == null) return ResolveInitialContext(state, branch, state.Reference.Address.InitialContext);
                else return ResolvePathSegment(state, branch, state.RemainingSegments.Current);
            }

            // tries to resolve a reference which points to the next destination context
            private bool JumpTo(ReferenceState state, ReferenceBranch branch, IStonEntity entity)
            {
                if (entity is IStonValuedEntity)
                {
                    state.Context = entity as IStonValuedEntity;
                    if (!state.RemainingSegments.MoveNext()) state.Incomplete = false;
                    state.NextContextReference = null;
                    return true;
                }
                else
                {
                    state.NextContextReference = entity as IStonReferenceEntity;
                    branch.PushOrFail(state.RemainingSegments.Current, state.NextContextReference);
                    var value = GetReferencedValue(state.NextContextReference, branch);
                    branch.Pop();
                    if (value == null) return false;
                    else return JumpTo(state, branch, value);
                }
            }

            // tries to resolve the reference entity's initial context
            private bool ResolveInitialContext(ReferenceState state, ReferenceBranch branch, IStonInitialContext initialContext)
            {
                if (initialContext is IStonAncestorInitialContext) return ResolveInitialContext(state, branch, initialContext as IStonAncestorInitialContext);
                else return ResolveInitialContext(state, branch, initialContext as IStonGlobalEntityInitialContext);
            }

            // tries to resolve an ancestor context
            private bool ResolveInitialContext(ReferenceState state, ReferenceBranch branch, IStonAncestorInitialContext initialContext)
            {
                // if a void context (parent of the document core) is reached
                // the next context cannot be determined
                var context = Document.Parents[state.Reference];
                if (context == null) throw state.MakeUnknownReferenceException();
                for (int i = 0; i < initialContext.AncestorOrder; i++)
                {
                    context = Document.Parents[context];
                    if (context == null) throw state.MakeUnknownReferenceException();
                }
                return JumpTo(state, branch, context);
            }

            // tries to resolve a global entity context
            private bool ResolveInitialContext(ReferenceState state, ReferenceBranch branch, IStonGlobalEntityInitialContext initialContext)
            {
                if (!Document.GlobalEntities.ContainsKey(initialContext.GlobalIdentifier)) throw new StonUnknownReferenceException(state.Reference, null, null);
                return JumpTo(state, branch, Document.GlobalEntities[initialContext.GlobalIdentifier]);
            }

            // tries to resolve a path segment of the reference entity
            private bool ResolvePathSegment(ReferenceState state, ReferenceBranch branch, IStonPathSegment segment)
            {
                if (segment is IStonAncestorPathSegment) return ResolvePathSegment(state, branch, segment as IStonAncestorPathSegment);
                else if (segment is IStonMemberPathSegment) return ResolvePathSegment(state, branch, segment as IStonMemberPathSegment);
                else return ResolvePathSegment(state, branch, segment as IStonCollectionElementPathSegment);
            }

            // tries to resolve an ancestor path segment
            private bool ResolvePathSegment(ReferenceState state, ReferenceBranch branch, IStonAncestorPathSegment segment)
            {
                // if a void context (parent of the document core) is reached
                // the next context cannot be determined
                var context = state.Context;
                for (int i = 0; i < segment.AncestorOrder; i++)
                {
                    context = Document.Parents[context];
                    if (context == null) throw state.MakeUnknownReferenceException();
                }
                return JumpTo(state, branch, context);
            }

            // tries to resolve a member path segment
            private bool ResolvePathSegment(ReferenceState state, ReferenceBranch branch, IStonMemberPathSegment segment)
            {
                if (!(state.Context is IStonComplexEntity)) throw state.MakeUnknownReferenceException();
                IStonBindingKey bindingKey = segment.BindingKey;
                var context = state.Context as IStonComplexEntity;

                // retrieving a resolved member, if any
                IStonEntity value;
                IDictionary<IStonBindingKey, IStonEntity> memberLookup = Document.MemberMappings[context];
                if (memberLookup.TryGetValue(bindingKey, out value)) return JumpTo(state, branch, value);

                // since binding names are always resolved
                // the binding key must be an index
                var index = bindingKey as IStonBindingIndex;

                // if it's not an index, it means that there is and won't be any member with a given name
                if (index == null) throw state.MakeUnknownReferenceException();

                // ensuring that all index parameters are known
                // otherwise they cannot be compared against the member index parameters anyhow
                foreach (var parameter in index.Parameters)
                {
                    if (parameter is IStonReferenceEntity && GetReferencedValue(parameter as IStonReferenceEntity, branch) == null) return false;
                }

                // using the indexed member path segment as collection element path segment, if possible
                if (!DefinesReferenceIndices[context] && !index.Parameters.Skip(1).Any())
                {
                    IStonValuedEntity parameter = GetValue(index.Parameters.First());
                    if (parameter.Type == null && parameter is IStonSimpleEntity)
                    {
                        var parameterValue = (parameter as IStonSimpleEntity).Value;
                        if (parameterValue.DataType == StonDataType.Binary && !DefinesImplicitBinaryIndices[context]) return JumpTo(state, branch, GetCollectionElement(state, parameterValue));
                        else if (parameterValue.DataType == StonDataType.Number && !DefinesImplicitNumberIndices[context]) return JumpTo(state, branch, GetCollectionElement(state, parameterValue));
                    }
                }

                // if the segment cannot be handled as a collection element path segment
                // the segment refers to the indexed member instead
                var resolved = Document.MemberMappings[context];

                var unresolvedLookup = UnresolvedMembers[context];
                var lookupIdx = index.Parameters.Count();
                if (unresolvedLookup.Count <= lookupIdx) throw state.MakeUnknownReferenceException();   // there are no unresolved indices with a given count
                                                                                                        // so resolving won't help
                var unresolved = UnresolvedMembers[context][lookupIdx];

                bool anyResolved = true;
                while (anyResolved)
                {
                    anyResolved = false;
                    var unresolvedCopy = unresolved.ToList();
                    foreach (var memberBinding in unresolvedCopy)
                    {
                        // it's possible that at some point a nested reference resolving call
                        // will take care of earlier copied member binding
                        // in such case, the already-resolved member binding must be skipped
                        if (!unresolved.Remove(memberBinding)) continue;

                        bool isResolved = true;

                        // all inner index references must be resolved to values
                        foreach (var parameter in memberBinding.Key.Parameters)
                        {
                            if (parameter is IStonReferenceEntity)
                            {
                                if (GetReferencedValue(parameter as IStonReferenceEntity, null) == null)
                                {
                                    isResolved = false;
                                    break;
                                }
                            }
                        }

                        // all parameters have known values
                        if (isResolved)
                        {
                            anyResolved = true;
                            if (resolved.ContainsKey(memberBinding.Key))
                            {
                                throw new StonDuplicateMemberException(context, Document, resolved.Keys.First(b => BindingKeyComparer.Equals(b, memberBinding.Key)), memberBinding.Key);
                            }
                            resolved.Add(memberBinding.Key, memberBinding.Value);
                        }
                        // at least one parameter has unresolved value
                        else unresolved.Add(memberBinding);
                    }
                }

                // attempting to get an indexed member
                // which might or might not have been one of the just resolved members
                IStonEntity anotherValue;
                if (memberLookup.TryGetValue(bindingKey, out anotherValue)) return JumpTo(state, branch, anotherValue);
                else return false;
            }

            // tries to resolve a collection element path segment
            private bool ResolvePathSegment(ReferenceState state, ReferenceBranch branch, IStonCollectionElementPathSegment segment)
            {
                if (!(state.Context is IStonComplexEntity)) throw state.MakeUnknownReferenceException();

                // making sure that the element index can be resolved
                // if the index cannot be resolved yet, resolving is left for later
                IStonEntity preIndex = segment.ElementIndex;
                IStonValuedEntity index;
                if (preIndex is IStonValuedEntity) index = preIndex as IStonValuedEntity;
                else
                {
                    index = GetReferencedValue(preIndex as IStonReferenceEntity, branch);
                    if (index == null) return false;
                }

                // basic index validation
                if (index is IStonComplexEntity) throw state.MakeUnknownReferenceException("The collection element index must be a simple value.");
                if (index.Type != null) throw state.MakeUnknownReferenceException("The collection element index must be implicitly typed.");

                return JumpTo(state, branch, GetCollectionElement(state, (index as IStonSimpleEntity).Value));
            }

            // finds a collection element entity, given a simple value representing the entity's index
            private IStonEntity GetCollectionElement(ReferenceState state, IStonSimpleValue value)
            {
                // eliminating basic invalid cases
                if (value.DataType != StonDataType.Binary && value.DataType != StonDataType.Number)
                {
                    throw state.MakeUnknownReferenceException("The collection element index value must be of number or binary data type.");
                }
                if (value.Content[0] == '-') throw state.MakeUnknownReferenceException("The collection element index must not be negative.");

                var context = state.Context as IStonComplexEntity;
                if (context.CollectionInit == null) throw state.MakeUnknownReferenceException();

                long index = 0;
                // binary-represented index
                if (value.DataType == StonDataType.Binary)
                {
                    foreach (int c in value.Content)
                    {
                        index = (index << 4) ^ c.GetBase16Value();
                        if (index > int.MaxValue) throw new NotSupportedException("The collection element index cannot be larger than maximum 32-bit signed integer value (2147483647).");
                    }
                }
                // number-represented index
                else
                {
                    if (value.Content == "0") return context.CollectionInit.Elements.ElementAt(0);

                    IEnumerator<char> contentChars = value.Content.GetEnumerator();
                    contentChars.MoveNext();
                    char c;

                    // iterating through the significand
                    while ((c = contentChars.Current) != 'e')
                    {
                        index = index * 10 + (c - '0');
                        if (index > int.MaxValue) throw new NotSupportedException("The collection element index cannot be larger than maximum 32-bit signed integer value (2147483647).");
                        contentChars.MoveNext();
                    }
                    contentChars.MoveNext();

                    // handling the exponent
                    c = contentChars.Current;
                    if (c == '-') throw state.MakeUnknownReferenceException("The collection element index must not be fractional.");
                    for (int i = 0; i < c - '0'; i++)
                    {
                        index = index * 10;
                        if (index > int.MaxValue) throw new NotSupportedException("The collection element index cannot be larger than maximum 32-bit signed integer value (2147483647).");
                    }
                    // if exponent has more than one digit, it must represent a number too large
                    if (contentChars.MoveNext()) throw new NotSupportedException("The collection element index cannot be larger than maximum 32-bit signed integer value (2147483647).");
                }

                // finding the element with the given index
                var result = context.CollectionInit.Elements.ElementAtOrDefault((int)index);
                if (result == null) throw state.MakeUnknownReferenceException();
                return result;
            }

            #endregion

            // resolving the remaining members after all references were already resolved
            // and thus the index parameter values are all known

            #region Resolving remaining members

            private void ResolveMembers()
            {
                foreach (var entityListPair in UnresolvedMembers)
                {
                    var resolved = Document.MemberMappings[entityListPair.Key];
                    var memberBindings = entityListPair.Value.SelectMany(l => l);
                    foreach (var memberBinding in memberBindings)
                    {
                        if (resolved.ContainsKey(memberBinding.Key))
                        {
                            throw new StonDuplicateMemberException(entityListPair.Key, Document, resolved.Keys.First(b => BindingKeyComparer.Equals(b, memberBinding.Key)), memberBinding.Key);
                        }
                        resolved.Add(memberBinding.Key, memberBinding.Value);
                    }
                }
            }

            #endregion

            // checking that there are no loops in construction dependencies

            #region Verifying construction order

            // keeps track of construction chains
            // which allows finding circular dependencies
            private class ConstructionBranch
            {
                private IStonDocument Document { get; }
                private IStonComplexEntity Tail { get; set; }
                private IList<KeyValuePair<IStonComplexEntity, int>> _Path { get; }
                private HashSet<IStonComplexEntity> _FoundEntities { get; }

                // creates a new construction branch, starting from a given constructed entity
                internal ConstructionBranch(IStonDocument document, IStonComplexEntity constructedEntity)
                {
                    Document = document;
                    Tail = constructedEntity;
                    _Path = new List<KeyValuePair<IStonComplexEntity, int>>();
                    _FoundEntities = new HashSet<IStonComplexEntity>();
                }

                // adds a new constructed entity to the chain
                // and throws circular reference exception if applicable
                internal void PushOrFail(int position, IStonComplexEntity entity)
                {
                    _Path.Add(new KeyValuePair<IStonComplexEntity, int>(Tail, position));
                    _FoundEntities.Add(Tail);
                    Tail = entity;

                    if (_FoundEntities.Contains(Tail)) throw new StonCircularConstructionException(Document, _Path.SkipWhile(kvp => kvp.Key != Tail));
                }

                // removes the latest constructed entity from the chain
                internal void Pop()
                {
                    Tail = _Path.Last().Key;
                    _FoundEntities.Remove(Tail);
                    _Path.RemoveAt(_Path.Count - 1);
                }
            }

            // stores entities that have yet their dependencies to be resolved
            private IDictionary<IStonComplexEntity, IEnumerable<IStonEntity>> ConstructionDependencies { get; }
                = new Dictionary<IStonComplexEntity, IEnumerable<IStonEntity>>();

            // resolves all construction dependencies
            private void VerifyConstructionOrder()
            {
                while (ConstructionDependencies.Any())
                {
                    var entityDependency = ConstructionDependencies.First();
                    var branch = new ConstructionBranch(Document, entityDependency.Key);
                    HandleDependency(entityDependency.Key, entityDependency.Value, branch);
                }
            }

            // resolves construction dependencies of a single entity
            private void HandleDependency(IStonComplexEntity entity, IEnumerable<IStonEntity> dependencies, ConstructionBranch branch)
            {
                int position = 0;
                foreach (var parameter in entity.Construction.PositionalParameters)
                {
                    if (dependencies.Contains(parameter)) HandleParameter(parameter, position, branch);
                    position++;
                }
                foreach (var namedParameter in entity.Construction.NamedParameters)
                {
                    if (dependencies.Contains(namedParameter.Value)) HandleParameter(namedParameter.Value, position, branch);
                    position++;
                }
                ConstructionDependencies.Remove(entity);
            }

            // resolves a construction dependency related to a give parameter
            private void HandleParameter(IStonEntity parameter, int position, ConstructionBranch branch)
            {
                var value = GetValue(parameter) as IStonComplexEntity;

                if (value == null) return;      // the value is a simple entity
                                                // and has no dependencies at all

                if (ConstructionDependencies.ContainsKey(value))
                {
                    branch.PushOrFail(position, value);
                    HandleDependency(value, ConstructionDependencies[value], branch);
                    branch.Pop();
                }
            }

            #endregion

            // gets a value represented by a STON entity
            // entity itself it it's a valued entity
            // or a referenced value if it's a reference entity
            private IStonValuedEntity GetValue(IStonEntity entity)
            {
                if (entity is IStonValuedEntity) return entity as IStonValuedEntity;
                else return GetReferencedValue(entity as IStonReferenceEntity);
            }
        }
    }
}
