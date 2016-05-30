using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityExecutionOrder {
    public abstract class Run : Attribute, IEquatable<Run> {

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public sealed class Before : Run {
            public Before(Type type) : base(type) {}
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public sealed class After : Run {
            public After(Type type) : base(type) {}
        }

        public readonly Type Type;

        private Run(Type type) {
            Type = type;
        }

        public bool Equals(Run other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(Type, other.Type);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Run) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (base.GetHashCode() * 397) ^ (Type != null ? Type.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Run left, Run right) {
            return Equals(left, right);
        }

        public static bool operator !=(Run left, Run right) {
            return !Equals(left, right);
        }
    }
}
