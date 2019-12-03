using System;

namespace NStuff.Tessellation
{
    internal class PriorityQueue<TItem> where TItem : PriorityQueue<TItem>.Item, IComparable<TItem>
    {
        internal class Item
        {
            internal int queueHandle;
        }

        private TItem[] nodes;

        public int Count { get; private set; }

        public PriorityQueue(int capacity)
        {
            Count = 0;
            nodes = new TItem[capacity];
        }

        public void Enqueue(TItem node)
        {
            var nodes = this.nodes;
            if (Count >= nodes.Length - 1)
            {
                var t = new TItem[Count * 2];
                Array.Copy(nodes, t, nodes.Length);
                this.nodes = nodes = t;
            }

            Count++;
            nodes[Count] = node;
            node.queueHandle = Count;
            CascadeUp(node);
        }

        private void CascadeUp(TItem node)
        {
            int parent;
            if (node.queueHandle > 1)
            {
                parent = node.queueHandle >> 1;
                var parentNode = nodes[parent];
                if (HasHigherOrEqualPriority(parentNode, node))
                    return;

                nodes[node.queueHandle] = parentNode;
                parentNode.queueHandle = node.queueHandle;

                node.queueHandle = parent;
            }
            else
            {
                return;
            }
            while (parent > 1)
            {
                parent >>= 1;
                var parentNode = nodes[parent];
                if (HasHigherOrEqualPriority(parentNode, node))
                {
                    break;
                }

                nodes[node.queueHandle] = parentNode;
                parentNode.queueHandle = node.queueHandle;

                node.queueHandle = parent;
            }
            nodes[node.queueHandle] = node;
        }

        private void CascadeDown(TItem node)
        {
            var nodes = this.nodes;
            int finalQueueIndex = node.queueHandle;
            int childLeftIndex = 2 * finalQueueIndex;

            if (childLeftIndex > Count)
            {
                return;
            }

            int childRightIndex = childLeftIndex + 1;
            var childLeft = nodes[childLeftIndex];
            if (HasHigherPriority(childLeft, node))
            {
                if (childRightIndex > Count)
                {
                    node.queueHandle = childLeftIndex;
                    childLeft.queueHandle = finalQueueIndex;
                    nodes[finalQueueIndex] = childLeft;
                    nodes[childLeftIndex] = node;
                    return;
                }
                var childRight = nodes[childRightIndex];
                if (HasHigherPriority(childLeft, childRight))
                {
                    childLeft.queueHandle = finalQueueIndex;
                    nodes[finalQueueIndex] = childLeft;
                    finalQueueIndex = childLeftIndex;
                }
                else
                {
                    childRight.queueHandle = finalQueueIndex;
                    nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
            }
            else if (childRightIndex > Count)
            {
                return;
            }
            else
            {
                var childRight = nodes[childRightIndex];
                if (HasHigherPriority(childRight, node))
                {
                    childRight.queueHandle = finalQueueIndex;
                    nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
                else
                {
                    return;
                }
            }

            while (true)
            {
                childLeftIndex = 2 * finalQueueIndex;

                if (childLeftIndex > Count)
                {
                    node.queueHandle = finalQueueIndex;
                    nodes[finalQueueIndex] = node;
                    break;
                }

                childRightIndex = childLeftIndex + 1;
                childLeft = nodes[childLeftIndex];
                if (HasHigherPriority(childLeft, node))
                {
                    if (childRightIndex > Count)
                    {
                        node.queueHandle = childLeftIndex;
                        childLeft.queueHandle = finalQueueIndex;
                        nodes[finalQueueIndex] = childLeft;
                        nodes[childLeftIndex] = node;
                        break;
                    }
                    TItem childRight = nodes[childRightIndex];
                    if (HasHigherPriority(childLeft, childRight))
                    {
                        childLeft.queueHandle = finalQueueIndex;
                        nodes[finalQueueIndex] = childLeft;
                        finalQueueIndex = childLeftIndex;
                    }
                    else
                    {
                        childRight.queueHandle = finalQueueIndex;
                        nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                }
                else if (childRightIndex > Count)
                {
                    node.queueHandle = finalQueueIndex;
                    nodes[finalQueueIndex] = node;
                    break;
                }
                else
                {
                    var childRight = nodes[childRightIndex];
                    if (HasHigherPriority(childRight, node))
                    {
                        childRight.queueHandle = finalQueueIndex;
                        nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                    else
                    {
                        node.queueHandle = finalQueueIndex;
                        nodes[finalQueueIndex] = node;
                        break;
                    }
                }
            }
        }

        private bool HasHigherPriority(TItem higher, TItem lower) => higher.CompareTo(lower) < 0;

        private bool HasHigherOrEqualPriority(TItem higher, TItem lower) => higher.CompareTo(lower) <= 0;

        public TItem Dequeue()
        {
            var returnMe = nodes[1];
            if (Count == 1)
            {
                nodes[1] = default!;
                Count = 0;
                return returnMe;
            }

            var formerLastNode = nodes[Count];
            nodes[1] = formerLastNode;
            formerLastNode.queueHandle = 1;
            nodes[Count] = default!;
            Count--;

            CascadeDown(formerLastNode);
            return returnMe;
        }

        public TItem Peek() => nodes[1];

        public void Remove(TItem node)
        {
            if (node.queueHandle == Count)
            {
                nodes[Count] = default!;
                Count--;
                return;
            }

            var formerLastNode = nodes[Count];
            nodes[node.queueHandle] = formerLastNode;
            formerLastNode.queueHandle = node.queueHandle;
            nodes[Count] = default!;
            Count--;

            int parentIndex = formerLastNode.queueHandle >> 1;
            if (parentIndex > 0 && HasHigherPriority(formerLastNode, nodes[parentIndex]))
            {
                CascadeUp(formerLastNode);
            }
            else
            {
                CascadeDown(formerLastNode);
            }
        }
    }
}
