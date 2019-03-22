﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JointsReduction
{
	class MapNode
	{
		public string name;
		public Transform src;
		public MapNode parent;
		public ArrayList children = new ArrayList();
		private Matrix4x4 m0Inv;
		public MapNode(Transform a_src, string a_name, MapNode a_parent)
		{
			src = a_src;
			name = a_name;
			parent = a_parent;
			if (null == parent)
				m0Inv = Matrix4x4.identity;
			else
				m0Inv = src.worldToLocalMatrix * parent.src.localToWorldMatrix;

		}

		public Matrix4x4 DeltaM()
		{
			if (null == parent)
				return Matrix4x4.identity;
			else
			{
				Matrix4x4 mt = parent.src.worldToLocalMatrix * src.localToWorldMatrix;
				return mt*m0Inv;
			}
		}
	};

	class JointsMap
	{
		public static bool DFN_DBGLOG = true;
		protected MapNode m_rootOut;
		static string [] s_map = {
			"CMU compliant skeleton",	"Armature",
			"Hips",						"base",
			"LowerBack",				"back",
			"LeftUpLeg",				"hip_l",
			"RightUpLeg",				"hip_r",
			"LeftLeg",					"knee_l",
			"RightLeg",					"knee_r",
			"LeftFoot",					"ankle_l",
			"Neck",						"cervical",
			"RightFoot",				"ankle_r",
			"LeftArm",					"shoulder_l",
			"RightArm",					"shoulder_r",
			"LeftForeArm",				"elbow_l",
			"RightForeArm",				"elbow_r",
			"LeftHand",					"wrist_l",
			"RightHand",				"wrist_r"
		};

		class TransNodeDFT
		{
			public Transform node_this;
			public int i_nextchild;
			public TransNodeDFT(Transform a_tran)
			{
				i_nextchild = 0;
				node_this = a_tran;
			}
			public Transform nextChild()
			{
				Transform ret = null;
				if (i_nextchild < node_this.childCount)
					ret = node_this.GetChild(i_nextchild);
				i_nextchild++;
				return ret;
			}
		};

		class MapNodeDFT
		{
			public MapNode n_this;
			public int i_nextchild;
			public MapNodeDFT(MapNode a_this)
			{
				i_nextchild = 0;
				n_this = a_this;
			}
			public MapNode nextChild()
			{
				MapNode child = null;
				if (i_nextchild < n_this.children.Count)
					child = (MapNode)n_this.children[i_nextchild];
				i_nextchild++;
				return child;
			}
		};

		public void Initialize(Transform root)
		{
			Dictionary<string, string> Ori2Redu = new Dictionary<string, string>();
			Dictionary<string, string> Redu2Ori = new Dictionary<string, string>();
			for (int i_map = 0; i_map < s_map.Length; i_map += 2)
			{
				string ori = s_map[i_map];
				string red = s_map[i_map + 1];
				Ori2Redu[ori] = red;
				Redu2Ori[red] = ori;
			}
			string name_c;
			bool verify = Ori2Redu.TryGetValue(root.name, out name_c);
			Debug.Assert(verify);
			Stack<MapNode> dfcSt = new Stack<MapNode>();
			MapNode n_dfc = new MapNode(root, name_c, null);
			dfcSt.Push(n_dfc);

			//depth first traversing the joint tree
			Stack<TransNodeDFT> dftSt = new Stack<TransNodeDFT>();
			TransNodeDFT n_dft = new TransNodeDFT(root);
			dftSt.Push(n_dft);


			string logSrc = null;
			if (DFN_DBGLOG)
				logSrc = string.Format("{0}\n", n_dft.node_this.name);
			while (dftSt.Count > 0)
			{
				Debug.Assert(dfcSt.Count > 0);
				TransNodeDFT p_node = dftSt.Peek();
				Transform c_tran = p_node.nextChild();
				if (null != c_tran)
				{
					if (DFN_DBGLOG)
					{
						for (int c_indent = 0; c_indent < dftSt.Count; c_indent++)
							logSrc += "\t";
						logSrc += string.Format("{0}\n", c_tran.name);
					}
					TransNodeDFT c_node = new TransNodeDFT(c_tran);
					dftSt.Push(c_node);

					if (Ori2Redu.TryGetValue(c_tran.name, out name_c))
					{
						MapNode p = dfcSt.Peek();
						MapNode c = new MapNode(c_tran, name_c, p);
						p.children.Add(c);
						dfcSt.Push(c);
					}
				}
				else
				{
					dftSt.Pop();
					if (Ori2Redu.TryGetValue(p_node.node_this.name, out name_c))
						dfcSt.Pop();
				}
			}

			m_rootOut = n_dfc;
			if (DFN_DBGLOG)
			{
				Debug.Log(logSrc);
				Stack<MapNodeDFT> dft = new Stack<MapNodeDFT>();
				MapNodeDFT dftNode = new MapNodeDFT(m_rootOut);
				dft.Push(dftNode);
				logSrc = string.Format("{0}\n", m_rootOut.name);
				while (dft.Count > 0)
				{
					MapNodeDFT p_nodeDFT = dft.Peek();
					MapNode c_node = p_nodeDFT.nextChild();
					if (null != c_node)
					{
						for (int i = 0; i < dft.Count; i ++)
							logSrc += "\t";
						logSrc += string.Format("{0}\n", c_node.name);
						MapNodeDFT c_nodeDFT = new MapNodeDFT(c_node);
						dft.Push(c_nodeDFT);
					}
					else
						dft.Pop();
				}
				Debug.Log(logSrc);
			}
		}
	};
}