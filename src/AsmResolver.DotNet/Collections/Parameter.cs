using System;
using AsmResolver.DotNet.Blob;

namespace AsmResolver.DotNet.Collections
{
    /// <summary>
    /// Represents a single parameter of a method. This is a matching of a parameter definition and its parameter type
    /// defined in the associated method signature.
    /// </summary>
    public class Parameter : INameProvider
    {
        private ParameterCollection _parentCollection;
        private TypeSignature _parameterType;

        internal Parameter(ParameterCollection parentCollection, int index, int methodSignatureIndex)
        {
            _parentCollection = parentCollection ?? throw new ArgumentNullException(nameof(parentCollection));
            Index = index;
            MethodSignatureIndex = methodSignatureIndex;
        }
        
        /// <summary>
        /// Gets the index of the parameter.
        /// </summary>
        public int Index
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the sequence number of the parameter, as used in the parameter definition list of the method definition.
        /// </summary>
        public ushort Sequence => (ushort) (MethodSignatureIndex + 1);

        /// <summary>
        /// Gets the index of the parameter within the method's signature. 
        /// </summary>
        public int MethodSignatureIndex
        {
            get;
        }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        public TypeSignature ParameterType
        {
            get => _parameterType;
            set
            {
                _parameterType = value;
                if (_parentCollection != null)
                    _parentCollection.PushParameterUpdateToSignature(this);
            }
        }

        /// <summary>
        /// Gets the associated definition of the parameter, if available.
        /// </summary>
        public ParameterDefinition Definition => _parentCollection.GetParameterDefinition(Sequence);

        /// <inheritdoc />
        public string Name => Definition?.Name ?? "A_" + MethodSignatureIndex;

        internal void Remove()
        {
            _parentCollection = null;
            Index = -1;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ParameterType} {Name}";
        }
    }
}