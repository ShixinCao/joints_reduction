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
			private Matrix4x4 m0_s2d;
			private Matrix4x4 m0_d2s;
			public MapNodeTube(MapNode a_in, Transform a_out, Transform base_in, Transform base_out)
			{
				Debug.Assert(a_in.name == a_out.name);
				nodeIn = a_in;
				nodeOut = a_out;
				m0 = nodeOut.parent.worldToLocalMatrix * nodeOut.localToWorldMatrix;
				Matrix4x4 l2r_s = base_in.worldToLocalMatrix * nodeIn.src.localToWorldMatrix;
				Matrix4x4 r2l_d = nodeOut.worldToLocalMatrix * base_out.localToWorldMatrix;
				m0_s2d = r2l_d * l2r_s;
				m0_d2s = m0_s2d.inverse;
			}
			public void Update()
			{
				Debug.Assert(nodeIn.name == nodeOut.name);
				Matrix4x4 deltaM_s = nodeIn.DeltaM_local();
				if (DFN_DBGLOG)
				{
					string str = string.Format("{0}---{1}:\n{2}=>{3}", nodeIn.name, nodeIn.src.name, nodeOut.name, nodeOut.parent.name);
					str += string.Format( "\n\t{0,5:#.00}\t{1,5:#.00}\t{2,5:#.00}\t{3,5:#.00}" +
												"\n\t{4,5:#.00}\t{5,5:#.00}\t{6,5:#.00}\t{7,5:#.00}" +
												"\n\t{8,5:#.00}\t{9,5:#.00}\t{10,5:#.00}\t{11,5:#.00}" +
												"\n\t{12,5:#.00}\t{13,5:#.00}\t{14,5:#.00}\t{15,5:#.00}"
													, deltaM_s.m00, deltaM_s.m01, deltaM_s.m02, deltaM_s.m03
													, deltaM_s.m10, deltaM_s.m11, deltaM_s.m12, deltaM_s.m13
													, deltaM_s.m20, deltaM_s.m21, deltaM_s.m22, deltaM_s.m23
													, deltaM_s.m30, deltaM_s.m31, deltaM_s.m32, deltaM_s.m33);
					//Debug.Log(str);
				}
				Matrix4x4 deltaM_d = m0_s2d * deltaM_s * m0_d2s;
				Matrix4x4 mt = m0 * deltaM_d;
				nodeOut.localRotation = mt.rotation;
				nodeOut.localPosition = new Vector3(mt.m03, mt.m13, mt.m23);
			}

			public void UpdateCmp()
			{
				Debug.Assert(nodeIn.name == nodeOut.name);
				Matrix4x4 deltaM_s = nodeIn.DeltaM_localCmp();
				Matrix4x4 deltaM_d = m0_s2d * deltaM_s * m0_d2s;
				Matrix4x4 mt = m0 * deltaM_d;
				nodeOut.localRotation = mt.rotation;
				nodeOut.localPosition = new Vector3(mt.m03, mt.m13, mt.m23);
			}



		};
		private MapNodeTube m_rootTubeIgnore;
		private MapNodeTube m_rootTubeRedu;
		public new void Initialize(Transform rootSrc, Transform rootDstIgnore, Transform rootDstRedu
								, Transform baseSrc, Transform baseDstIgnore, Transform baseDstRedu
								, string[] j_ori, string[] j_redu)
		{
			base.Initialize(rootSrc, j_ori, j_redu);
			m_rootTubeRedu = ConstructTubeTree(rootDstRedu, baseSrc, baseDstRedu);
			m_rootTubeIgnore = ConstructTubeTree(rootDstIgnore, baseSrc, baseDstIgnore);
		}


		private MapNodeTube ConstructTubeTree(Transform root15, Transform baseSrc, Transform baseDstRedu)
		{
			Queue<MapNode> bfsQSrc = new Queue<MapNode>();
			Queue<MapNodeTube> bfsQDst = new Queue<MapNodeTube>();

			Debug.Assert(m_rootOut.name == root15.name);
			MapNodeTube rootTube = new MapNodeTube(m_rootOut, root15, baseSrc, baseDstRedu);
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
					MapNodeTube c_nodeTube = new MapNodeTube(c_nodeOut, p_nodeTube.nodeOut.Find(c_nodeOut.name), baseSrc, baseDstRedu);
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

		private void UpdateTubeCmp(MapNodeTube tube15)
		{
			Queue<MapNodeTube> bfsQ = new Queue<MapNodeTube>();
			bfsQ.Enqueue(tube15);
			while (bfsQ.Count > 0)
			{
				MapNodeTube p_tube = bfsQ.Dequeue();
				p_tube.UpdateCmp();
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
			UpdateTubeCmp(m_rootTubeIgnore);
		}

	}
}
