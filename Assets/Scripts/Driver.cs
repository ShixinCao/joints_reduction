using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JointsReduction;
public class Driver : MonoBehaviour {

	// Use this for initialization
	public GameObject [] m_drivens;
	public GameObject [] m_drivensJR;
	public GameObject    m_drivingJR;
	public GameObject m_camera;
	public string [] m_oriJs;
	public string [] m_reduJs;
	private Vector3 m_t0Cam;
	private Vector3 [] m_t0sDst;
	private Quaternion [] m_r0sDst;
	private Vector3 m_t0Src;
	private Quaternion m_r0InvSrc;
	private JointsMapInternal m_jointsmap = new JointsMapInternal();
	void Start () {
		//fixme: set up the 0 position for 3 avatars
		m_t0Src = transform.position;
		Quaternion r0 = transform.rotation;
		m_r0InvSrc = Quaternion.Inverse(r0);
		string log = string.Format("[{0, 4:F2} {1, 4:F2} {2, 4:F2} {3, 4:F2}]=>[{4, 4:F2} {5, 4:F2} {6, 4:F2} {7, 4:F2}]"
								, r0.w, r0.x, r0.y, r0.z
								, m_r0InvSrc.w, m_r0InvSrc.x, m_r0InvSrc.y, m_r0InvSrc.z);
		Debug.Log(log);
		m_t0sDst = new Vector3[m_drivens.Length];
		m_r0sDst = new Quaternion[m_drivens.Length];
		Transform [] rootDst = new Transform[2] {m_drivensJR[0].transform, m_drivensJR[1].transform};
		for (int i_driven = 0; i_driven < m_drivens.Length; i_driven ++)
		{
			Transform trans = m_drivens[i_driven].transform;
			m_t0sDst[i_driven] = trans.position;
			m_r0sDst[i_driven] = trans.rotation;
		}

		Transform rootSrc = m_drivingJR.transform;
		Debug.Assert(null != rootSrc);
		m_jointsmap.Initialize(rootSrc, rootDst[0], rootDst[1]
			, transform, m_drivens[0].transform, m_drivens[1].transform
			, m_oriJs, m_reduJs);

		m_t0Cam = m_camera.transform.position;
	}

	// Update is called once per frame
	void Update () {
		Vector3 dT = transform.position - m_t0Src;
		Quaternion dR = m_r0InvSrc * transform.rotation;

		for (int i_driven = 0; i_driven < m_drivens.Length; i_driven++)
		{
			Transform trans = m_drivens[i_driven].transform;
			trans.position = m_t0sDst[i_driven] + dT;
			trans.rotation = m_r0sDst[i_driven] * dR;
		}
		m_jointsmap.Update();

		m_camera.transform.position = m_t0Cam + dT;
	}
}
