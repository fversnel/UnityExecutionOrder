//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//
//namespace UnityExecutionOrder {
//    public class Lazy<T> {
//        private bool _isRealized;
//        private T _value;
//        private readonly Func<T> _constructValue;
//
//        public Lazy(Func<T> constructValue) {
//            _constructValue = constructValue;
//            _isRealized = false;
//            _value = default (T);
//        }
//
//
//        public T Value {
//            get {
//                if (!_isRealized) {
//                    _value = _constructValue();
//                    _isRealized = true;
//                }
//                return _value;
//            }
//        }
//    }
//}
