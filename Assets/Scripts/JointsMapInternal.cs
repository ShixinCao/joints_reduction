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
			private Matrix4x4 m0;
			public MapNodeTube(MapNode a_in, Transform a_out)
			{
				Debug.Assert(a_in.name == a_out.name);
				nodeIn = a_in;
				nodeOut = a_out;
				m0 = nodeOut.parent.worldToLocalMatrix * nodeOut.localToWorldMatrix;
			}
			public void Update()
			{
				Debug.Assert(nodeIn.name == nodeOut.name);
				Matrix4x4 deltaM = nodeIn.DeltaM();
				if (DFN_DBGLOG)
				{
					string str = string.Format("{0}: \n\t{1,5:#.00}\t{2,5:#.00}\t{3,5:#.00}\t{4,5:#.00}" +
													"\n\t{5,5:#.00}\t{6,5:#.00}\t{7,5:#.00}\t{8,5:#.00}" +
													"\n\t{9,5:#.00}\t{10,5:#.00}\t{11,5:#.00}\t{12,5:#.00}" +
													"\n\t{13,5:#.00}\t{14,5:#.00}\t{15,5:#.00}\t{16,5:#.00}"
													, nodeOut.name
													, deltaM.m00, deltaM.m01, deltaM.m02, deltaM.m03
													, deltaM.m10, deltaM.m11, deltaM.m12, deltaM.m13
													, deltaM.m20, deltaM.m21, deltaM.m22, deltaM.m23
													, deltaM.m30, deltaM.m31, deltaM.m32, deltaM.m33);
					Debug.Log(str);
				}
				Matrix4x4 mt = deltaM * m0;
				nodeOut.localRotation = mt.rotation;
				nodeOut.localPosition = new Vector3(mt.m03, mt.m13, mt.m23);
			}

		};
		private MapNodeTube m_rootTubeIgnore;
		private MapNodeTube m_rootTubeRedu;
		public new void Initialize(Transform rootSrc, Transform rootDstIgnore, Transform rootDstRedu)
		{
			base.Initialize(rootSrc);
			m_rootTubeRedu = ConstructTubeTree(rootDstRedu);
		}


		private MapNodeTube ConstructTubeTree(Transform root15)
		{
			Queue<MapNode> bfsQSrc = new Queue<MapNode>();
			Queue<MapNodeTube> bfsQDst = new Queue<MapNodeTube>();

			Debug.Assert(m_rootOut.name == root15.name);
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
					Debug.Assert(null != p_nodeTube.nodeOut.Find(c_nodeOut.name));
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

		public void Update()
		{
			//traverse the tubetree to send output to transform
			UpdateTube(m_rootTubeRedu);
		}

	}
}
