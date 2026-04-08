using System;

namespace Archivarius
{
    public abstract class AnyBackend : IAnyBackend
    {
        private int _sectionDepth;
        private bool _competed;
        
        public CapabilityState SectionsSupport { get; }
        public CapabilityState TypeSafetySupport { get; }
        public bool FlushSupport { get; }
        
        protected abstract bool WasOperationsPerformed { get; }
        
        protected abstract bool SetSectionUsage(bool enable);
        protected abstract bool SetTypeSafety(bool enable);
        
        protected AnyBackend(CapabilityState sectionsSupport, CapabilityState typeSafetySupport, bool flushSupport)
        {
            SectionsSupport = sectionsSupport;
            TypeSafetySupport = typeSafetySupport;
            FlushSupport = flushSupport;
        }
        
        protected void MarkCompleted()
        {
            _competed = true;
        }

        public bool TrySetSectionUsage(bool useSections)
        {
            if (WasOperationsPerformed)
            {
                return false;
            }
            
            switch (SectionsSupport)
            {
                case CapabilityState.NotSupported:
                    return !useSections;
                case CapabilityState.Supported:
                    return SetSectionUsage(useSections);
                case CapabilityState.Mandatory:
                    return useSections;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual void BeginSection()
        {
            if (_competed)
            {
                throw new InvalidOperationException("Cannot begin a section after completion.");
            }
            _sectionDepth += 1;
        }

        public virtual bool EndSection()
        {
            if (_competed)
            {
                throw new InvalidOperationException("Cannot end a section after completion.");
            }
            if (_sectionDepth == 0)
            {
                throw new InvalidOperationException("No section to end.");
            }
            _sectionDepth -= 1;
            return true;
        }

        public bool TrySetTypeSafety(bool enabled)
        {
            if (WasOperationsPerformed)
            {
                return false;
            }
            
            switch (SectionsSupport)
            {
                case CapabilityState.NotSupported:
                    return !enabled;
                case CapabilityState.Supported:
                    return SetTypeSafety(enabled);
                case CapabilityState.Mandatory:
                    return enabled;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}