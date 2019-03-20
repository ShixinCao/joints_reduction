using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JointsReduction
{
    class JointsMapInternal : JointsMap
    {
        class MapNodeTube
        {
            public MapNode nodeIn;
            public Transform nodeOut;
            public MapNodeTube [] children;
            public MapNodeTube(MapNode a_in, Transform a_out)
            {
                nodeIn = a_in;
                nodeOut = a_out;
            }
            public void Update()
            {
                nodeOut.localRotation = nodeIn.r;
                nodeOut.localPosition = nodeIn.t;
            }

        };
        private MapNodeTube m_rootTubeIgnore;
        private MapNodeTube m_rootTubeRedu;
        public void Initialize(Transform rootSrc, Transform rootDstIgnore, Transform rootDstRedu)
        {
            base.Initialize(rootSrc);
            //m_rootTubeIgnore = IntializeTubeTree(rootDstIgnore.Find("Armature/base"));
        }

        private MapNodeTube IntializeTubeTree(Transform root15)
        {
            Queue<MapNode> bfsQSrc = new Queue<MapNode>();
            Queue<MapNodeTube> bfsQDst = new Queue<MapNodeTube>();

            MapNodeTube rootTube = new MapNodeTube(m_rootOut, root15);
            bfsQSrc.Enqueue(m_rootOut);
            bfsQDst.Enqueue(rootTube);
            //bind tree(m_rootOut) with transform tree(rootDstRedu)
            while (bfsQSrc.Count > 0)
            {
                Debug.Assert(bfsQDst.Count > 0);
                MapNode p_nodeOut = bfsQSrc.Dequeue();
                MapNodeTube p_nodeTube = bfsQDst.Dequeue();
                p_nodeTube.children = new MapNodeTube[p_nodeOut.children.Count];
                for (int i_childSrc = 0; i_childSrc < p_nodeOut.children.Count; i_childSrc ++)
                {
                    MapNode c_nodeOut = (MapNode)p_nodeOut.children[i_childSrc];
                    MapNodeTube c_nodeTube = new MapNodeTube(c_nodeOut, p_nodeTube.nodeOut.Find(c_nodeOut.name));
                    p_nodeTube.children[i_childSrc] = c_nodeTube;
                    bfsQSrc.Enqueue(c_nodeOut);
                    bfsQDst.Enqueue(c_nodeTube);
                }
            }
            return rootTube;
        }

        private void UpdateTube(MapNodeTube tube15)
        {
            Queue<MapNodeTube> bfsQ = new Queue<MapNodeTube>();
            bfsQ.Enqueue(tube15);
            while (bfsQ.Count > 0)
            {
                MapNodeTube p_tube = bfsQ.Dequeue();
                p_tube.Update();
                for (int i_child = 0; i_child < p_tube.children.Length; i_child ++)
                {
                    MapNodeTube c_tube = p_tube.children[i_child];
                    bfsQ.Enqueue(c_tube);
                }
            }
        }

        public new void Update()
        {
            //base.Update();
            ////traverse the tubetree to send output to transform
            //UpdateTube(m_rootTubeIgnore);
        }

    }
}
