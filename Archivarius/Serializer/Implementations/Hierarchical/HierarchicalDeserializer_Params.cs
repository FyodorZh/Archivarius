using System;
using System.Collections.Generic;
using Archivarius.Constructors;

namespace Archivarius
{
    public interface IHierarchicalDeserializer_ParamsView
    {
        public IReader Source { get; }
        public bool AutoPrepare { get; }
        public IConstructorFactory? ObjectConstructorFactory { get; }
        public bool? IsPolymorphic { get; }
        public IHierarchicalDeserializer_PolymorphicParamsView? AsPolymorphic();
        public IHierarchicalDeserializer_MonomorphicParamsView? AsMonomorphic();
    }
    
    public interface IHierarchicalDeserializer_MonomorphicParamsView : IHierarchicalDeserializer_ParamsView
    {
    }
    
    public interface IHierarchicalDeserializer_PolymorphicParamsView : IHierarchicalDeserializer_ParamsView
    {
        ITypeDeserializer? TypeDeserializer { get; }
        ISerializerExtensionsFactory? ExtensionsFactory { get; }
        Func<int, IReadOnlyList<Type>>? DefaultTypeSetProvider { get; }
    }   
    
    public interface IHierarchicalDeserializer_Params
    {        
        IHierarchicalDeserializer_Params SetCtorFactory(IConstructorFactory ctorFactory);
        IHierarchicalDeserializer_Params SetAutoPrepare(bool autoPrepare);
        
        IHierarchicalDeserializer_MonomorphicParams SetMonomorphic();
        IHierarchicalDeserializer_PolymorphicParams SetPolymorphic(ITypeDeserializer typeDeserializer);
    }

    public interface IHierarchicalDeserializer_Builder
    {
        HierarchicalDeserializer Build();
    }
    
    public interface IHierarchicalDeserializer_MonomorphicParams : IHierarchicalDeserializer_Params, IHierarchicalDeserializer_Builder
    {
        new IHierarchicalDeserializer_MonomorphicParams SetCtorFactory(IConstructorFactory ctorFactory);
        new IHierarchicalDeserializer_MonomorphicParams SetAutoPrepare(bool autoPrepare);
    }
    
    public interface IHierarchicalDeserializer_PolymorphicParams : IHierarchicalDeserializer_Params, IHierarchicalDeserializer_Builder
    {
        IHierarchicalDeserializer_PolymorphicParams SetExtensionsFactory(ISerializerExtensionsFactory extensionsFactory);
        IHierarchicalDeserializer_PolymorphicParams SetDefaultTypeSet(Func<int, IReadOnlyList<Type>> defaultTypeSetProvider);
        new IHierarchicalDeserializer_PolymorphicParams SetCtorFactory(IConstructorFactory ctorFactory);
        new IHierarchicalDeserializer_PolymorphicParams SetAutoPrepare(bool autoPrepare);
    }
    
    public class HierarchicalDeserializer_Params : 
        IHierarchicalDeserializer_MonomorphicParams, IHierarchicalDeserializer_PolymorphicParams,
        IHierarchicalDeserializer_MonomorphicParamsView, IHierarchicalDeserializer_PolymorphicParamsView
    {
        public IReader Source { get; }
        public bool AutoPrepare { get; private set; } = true;
        public IConstructorFactory? ObjectConstructorFactory { get; private set; }
        public bool? IsPolymorphic { get; private set; }
        public IHierarchicalDeserializer_PolymorphicParamsView? AsPolymorphic() => IsPolymorphic == true ? this : null;
        public IHierarchicalDeserializer_MonomorphicParamsView? AsMonomorphic() => IsPolymorphic == false ? this : null;
        
        public ITypeDeserializer? TypeDeserializer { get; private set; }

        public ISerializerExtensionsFactory? ExtensionsFactory { get; private set; }

        public Func<int, IReadOnlyList<Type>>? DefaultTypeSetProvider { get; private set; }

        internal HierarchicalDeserializer_Params(IReader source)
        {
            Source = source;
        }

        IHierarchicalDeserializer_PolymorphicParams IHierarchicalDeserializer_PolymorphicParams.SetDefaultTypeSet(Func<int, IReadOnlyList<Type>> defaultTypeSetProvider)
        {
            DefaultTypeSetProvider = defaultTypeSetProvider;
            return this;
        }

        IHierarchicalDeserializer_Params IHierarchicalDeserializer_Params.SetCtorFactory(IConstructorFactory ctorFactory)
        {
            ObjectConstructorFactory = ctorFactory;
            return this;
        }
        IHierarchicalDeserializer_PolymorphicParams IHierarchicalDeserializer_PolymorphicParams.SetCtorFactory(IConstructorFactory ctorFactory)
        {
            ObjectConstructorFactory = ctorFactory;
            return this;
        }
        IHierarchicalDeserializer_MonomorphicParams IHierarchicalDeserializer_MonomorphicParams.SetCtorFactory(IConstructorFactory ctorFactory)
        {
            ObjectConstructorFactory = ctorFactory;
            return this;
        }
        
        
        IHierarchicalDeserializer_PolymorphicParams IHierarchicalDeserializer_PolymorphicParams.SetAutoPrepare(bool autoPrepare)
        {
            AutoPrepare = autoPrepare;
            return this;
        }

        public HierarchicalDeserializer Build()
        {
            return new HierarchicalDeserializer(this);
        }

        IHierarchicalDeserializer_MonomorphicParams IHierarchicalDeserializer_MonomorphicParams.SetAutoPrepare(bool autoPrepare)
        {
            AutoPrepare = autoPrepare;
            return this;
        }
        IHierarchicalDeserializer_Params IHierarchicalDeserializer_Params.SetAutoPrepare(bool autoPrepare)
        {
            AutoPrepare = autoPrepare;
            return this;
        }

        IHierarchicalDeserializer_MonomorphicParams IHierarchicalDeserializer_Params.SetMonomorphic()
        {
            if (IsPolymorphic != null)
                throw new InvalidOperationException();
            IsPolymorphic = false;
            return this;
        }
        
        IHierarchicalDeserializer_PolymorphicParams IHierarchicalDeserializer_Params.SetPolymorphic(ITypeDeserializer typeDeserializer)
        {
            if (IsPolymorphic != null)
                throw new InvalidOperationException();
            IsPolymorphic = true;
            TypeDeserializer = typeDeserializer;
            return this;
        }
        
        public IHierarchicalDeserializer_PolymorphicParams SetExtensionsFactory(ISerializerExtensionsFactory extensionsFactory)
        {
            ExtensionsFactory = extensionsFactory;
            return this;
        }
    }
}