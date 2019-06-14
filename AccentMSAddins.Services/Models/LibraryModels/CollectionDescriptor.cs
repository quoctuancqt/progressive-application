namespace AccentMSAddins.Services.Models
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    public class CollectionDescriptor : PropertyDescriptor
    {
        protected IList _collection = null;
        protected int _index = -1;

        public CollectionDescriptor()
            : base("#", null)
        {
        }

        public CollectionDescriptor(IList collection, int index)
            : base("#" + index, null)
        {
            _collection = collection;
            this._index = index;
        }

        public IList Collection
        {
            get { return _collection; }
            set { _collection = value; }
        }

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public override AttributeCollection Attributes
        {
            get
            {
                return TypeDescriptor.GetAttributes(_collection[_index].GetType());
            }
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get
            {
                return _collection.GetType();
            }
        }

        public override string DisplayName
        {
            get
            {
                return _collection[_index].ToString();
            }
        }

        public override string Description
        {
            get
            {
                return _collection[_index].ToString();
            }
        }

        public override object GetValue(object component)
        {
            return this._collection[_index];
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override string Name
        {
            get { return DisplayName; }
        }

        public override Type PropertyType
        {
            get { return this._collection[_index].GetType(); }
        }

        public override void ResetValue(object component) { }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        public override void SetValue(object component, object value)
        {
            this._collection[_index] = value;
        }
    }
}
