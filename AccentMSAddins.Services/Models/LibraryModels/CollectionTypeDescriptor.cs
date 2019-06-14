namespace AccentMSAddins.Services.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class CollectionTypeDescriptor<T, TD> : CollectionBase, ICustomTypeDescriptor, IEnumerable<T>
        where T : class
        where TD : CollectionDescriptor, new()
    {
        public void Add(T t)
        {
            this.List.Add(t);
        }

        public void AddRange(IEnumerable<T> otherList)
        {
            foreach (T t in otherList)
            {
                this.List.Add(t);
            }
        }

        public void AddRange(T[] array)
        {
            foreach (T t in array)
            {
                this.List.Add(t);
            }
        }

        public void Insert(int index, T t)
        {
            List.Insert(index, t);
        }

        public void InsertBefore(T target, T t)
        {
            int index = IndexOf(target);
            if (index > 0)
            {
                List.Insert(index - 1, t);
            }
            else
            {
                List.Insert(0, t);
            }
        }

        public void InsertAfter(T target, T t)
        {
            int index = IndexOf(target);
            if (index < List.Count && index >= 0)
            {
                List.Insert(index + 1, t);
            }
            else
            {
                List.Insert(0, t);
            }
        }

        public void Replace(T target, T t)
        {
            int index = IndexOf(target);
            if (index < List.Count && index >= 0)
            {
                List[index] = t;
            }
            else
            {
                List.Insert(0, t);
            }
        }

        public int IndexOf(T t)
        {
            return this.List.IndexOf(t);
        }

        public void Remove(T t)
        {
            this.List.Remove(t);
        }

        public T this[int i]
        {
            get
            {
                return (T)List[i];
            }
            set
            {
                List[i] = value;
            }
        }

        public T this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    return null;

                for (int i = 0; i < List.Count; i++)
                {
                    PropertyDescriptor d = CreateDescriptor(List, i);
                    if (d != null)
                    {
                        if (name.Equals(d.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return List[i] as T;
                        }
                    }
                }
                return null;
            }
        }

        public bool ContainsKey(string name)
        {
            return this[name] != null;
        }

        public String GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public String GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        public PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);
            for (int i = 0; i < this.List.Count; i++)
            {
                pds.Add(CreateDescriptor(this, i));
            }
            return pds;
        }

        protected virtual PropertyDescriptor CreateDescriptor(IList collection, int index)
        {
            CollectionDescriptor descriptor = new TD();
            descriptor.Collection = collection;
            descriptor.Index = index;
            return descriptor;
        }


        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            int enumPosition = 0;
            while (enumPosition < List.Count)
            {
                yield return List[enumPosition] as T;
                enumPosition++;
            }
        }

        public override string ToString()
        {
            return Count + " items";
        }

        public T[] ToArray()
        {
            List<T> list = new List<T>();
            foreach (T info in this)
            {
                list.Add(info);
            }
            return list.ToArray();
        }
    }
}
