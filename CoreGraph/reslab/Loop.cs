using System;
using System.Collections;
using System.Collections.Generic;

namespace reslab
{
    class Tli<T>
    {
        public Tli<T> tliPrev;
        public Tli<T> tliNext;
        public T sprItem;

        public override string ToString()
        {
            return "*" + sprItem;
        }
    }

    class EnLoop<T> : IEnumerator<T>
    {
        public T Current;
        Tli<T> tliPlace;

        object IEnumerator.Current => throw new NotImplementedException();

        T IEnumerator<T>.Current => Current;

        public EnLoop(Tli<T> tliFirst)
        {
            tliPlace = tliFirst;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if ( tliPlace != null)
            {
                Current = tliPlace.sprItem;
                tliPlace = tliPlace.tliNext;
                return true;
            }
            else
                return false;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    public class Loop<T> : IEnumerable<T> where T : class
    {

        Tli<T> tliFirst;
        Tli<T> tliLast;
        Tli<T> tliCurrent;

        // Move element i to its proper heap position in a min heap of length i+1
        public void Add(T tNewElement)
        {
            Tli<T> tliNew = new Tli<T>();
            tliNew.sprItem = tNewElement;
            if (tliFirst == null)
                tliFirst = tliNew;
            else
                tliLast.tliNext = tliNew;
            tliNew.tliPrev = tliLast;
            tliLast = tliNew;
            if (tliCurrent == null)
                tliCurrent = tliNew;
        }

        public void Clear()
        {
            tliFirst = null;
            tliLast = null;
            tliCurrent = null;
        }

        // Remove lowest element from heap of length len
        public T tPeek()
        {
            if (tliCurrent == null)
                return null;
            return tliCurrent.sprItem;
        }

        public bool fOnlyActive(Spr spr)
        {
            return tliFirst == tliLast && tliLast.sprItem == spr;
        }

        public void Remove()
        {
            if (tliCurrent.tliPrev == null)
            {
                tliFirst = tliCurrent.tliNext;
                if (tliFirst == null)
                {
                    tliLast = null;
                    tliCurrent = null;
                    return;
                }
            }
            else
                tliCurrent.tliPrev.tliNext = tliCurrent.tliNext;
            if (tliCurrent.tliNext != null)
                tliCurrent.tliNext.tliPrev = tliCurrent.tliPrev;
            else
                tliLast = tliCurrent.tliPrev;
        }

        public void MoveToNext()
        {
            if (tliCurrent == null)
                return;
            tliCurrent = tliCurrent.tliNext;
            if (tliCurrent == null)
                tliCurrent = tliFirst;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new EnLoop<T>(tliFirst);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
