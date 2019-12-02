using System;
using System.Collections;
using System.Collections.Generic;

namespace reslab
{
    public interface Icmp
    {
        /// <summary>
        /// Lower numbers are done first
        /// </summary>
        Ipr iprPriority();

    }

    public class Heap<T> where T : Icmp
    {
        const int nGrowth = 2;
        const int nInitSize = 8;
        public T[] rgtContents = new T[nInitSize];
        public int nNumElements = 0;
        
        public void Validate()
        {
            for (int nPos = 1; nPos < nNumElements; nPos++)
            {
                int ip = ((nPos + 1) / 2) - 1;
                T tPrev = rgtContents[ip];
                T tPos = rgtContents[nPos];
                if (tPos.iprPriority().fLessThan(tPrev.iprPriority()))
                    throw new ArgumentException();
            }
        }


        /// <summary>
        // Move element i to its proper heap position in a min heap of length i+1
        /// </summary>
        public void Add(T tNewElement)
        {
            if (nNumElements >= rgtContents.Length)
            {
                T[] rgtNewContents = new T[nNumElements * nGrowth];
                for (int nCopy = 0; nCopy < nNumElements; nCopy++)
                    rgtNewContents[nCopy] = rgtContents[nCopy];
                rgtContents = rgtNewContents;
            }
            HeapifyToFront (nNumElements++, tNewElement);
        }

        /// <summary>
        /// Entry at nPos has changed value to be closer to front. Fix its position
        /// </summary>
        public void MoveUp (int nPos)
        {
            T tElement = rgtContents[nPos];
            HeapifyToFront (nPos, tElement);
        }

        private void HeapifyToFront (int nPos, T tNewElement)
        {
            Ipr iprNew = tNewElement.iprPriority();
            while (nPos > 0)
            {
                int ip = ((nPos + 1) / 2) - 1;
                T tPrev = rgtContents[ip];
                Ipr iprPriorityIp = tPrev.iprPriority();

                int nComparison = iprPriorityIp.nCompare(iprNew);
                if (nComparison <= 0)
                    break;
                rgtContents[nPos] = tPrev;
                nPos = ip;
            }
            rgtContents[nPos] = tNewElement;
        }

        public bool fEmpty()
        {
            return nNumElements == 0;
        }

        public int nSize()
        {
            return nNumElements;
        }

        public T tPeek()
        {
            return rgtContents[0];
        }

        public T tPeekAt(int nPos)
        {
            return rgtContents[nPos];
        }

        /// <summary>
        // Remove lowest element from heap 
        /// </summary>
        public T tGet()
        {
            T tResult = rgtContents[0];
            nNumElements--;
            rgtContents[0] = rgtContents[nNumElements];
            // restore heap property
            HeapifyBack(0);
            return tResult;
        }

        /// <summary>
        /// Move element at nPlace towards the back where it belongs
        /// </summary>
        public void HeapifyBack(int nPlace)
        { 
            T s = rgtContents[nPlace];
            Ipr iprS = s.iprPriority();
            while (true)
            {
                int c1 = (2 * (nPlace + 1)) - 1;
                int c2 = c1 + 1;
                if (c1 >= nNumElements)
                    break;

                T t1 = rgtContents[c1];
                Ipr iprT1 = t1.iprPriority();
                if (c2 < nNumElements)
                {
                    T t2 = rgtContents[c2];
                    Ipr iprT2 = t2.iprPriority();
                    if (iprT2.fLessThan(iprT1))
                    {
                        t1 = t2;
                        c1 = c2;
                        iprT1 = iprT2;
                    }
                }
                if (iprS.fLessThan(iprT1))
                    break;
                rgtContents[nPlace] = t1;
                rgtContents[c1] = s;
                nPlace = c1;
            }
            rgtContents[nPlace] = s;
        }
    }

    /// <summary>
    /// Iterate through a Heap<typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class EnHeap<T> : IEnumerator<T> where T : Icmp
    {
        public T tCurrent;
        Heap<T> heap;
        int nPlace = 0;

        public EnHeap(Heap<T> heap)
        {
            this.heap = heap;
        }

        object IEnumerator.Current => throw new NotImplementedException();

        T IEnumerator<T>.Current => tCurrent;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (nPlace >= heap.nNumElements)
                return false;
            tCurrent = heap.rgtContents[nPlace];
            nPlace++;
            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Iterate through a HeapLoop<typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class EnHeapLoop<T> : IEnumerator<T> where T : Icmp
    {
        T tCurrent;
        EnHeap<T> enheapPending;
        EnHeap<T> enheapProcessed;

        object IEnumerator.Current => tCurrent;

        public T Current => tCurrent;

        public EnHeapLoop(HeapLoop<T> heaploop)
        {
            enheapPending = new EnHeap<T>(heaploop.heapPending);
            enheapProcessed = new EnHeap<T>(heaploop.heapProcessed);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (enheapPending != null)
            {
                if (enheapPending.MoveNext())
                {
                    tCurrent = enheapPending.tCurrent;
                    return true;
                }
                enheapPending = null;
            }
            if ( enheapProcessed.MoveNext())
            {
                tCurrent = enheapProcessed.tCurrent;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    public class HeapLoop<T> : IEnumerable<T> where T : Icmp
    {
        public Heap<T> heapPending = new Heap<T>();
        public Heap<T> heapProcessed = new Heap<T>();
        int nCounter = 0;
        const int nInitLimit = 10;
        int nLimit = nInitLimit;

        public void Add(T tNewElement, bool fNewItem)
        {
            if (fNewItem)
                heapPending.Add(tNewElement);
            else
                heapProcessed.Add(tNewElement);
        }

        public bool fEmpty()
        {
            return heapPending.fEmpty() && heapProcessed.fEmpty();
        }

        public T tGet()
        {
            if (!heapPending.fEmpty())
            {
                if (nCounter >= nLimit)
                {
                    // Add gradually increasing subsets from processed back into pending.
                    while (!heapProcessed.fEmpty() && nCounter-- >= 0)
                        heapPending.Add(heapProcessed.tGet());
                    nCounter = 0;
                    nLimit *= 2;
                }
                else
                    nCounter++;
            }
            else
            {
                Heap<T> heapSwap = heapProcessed;
                heapProcessed = heapPending;
                heapPending = heapSwap;
                nCounter = 0;
                nLimit = nInitLimit;
            }
            return heapPending.tGet();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new EnHeapLoop<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}